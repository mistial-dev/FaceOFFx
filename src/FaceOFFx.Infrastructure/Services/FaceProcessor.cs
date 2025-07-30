using CSharpFunctionalExtensions;
using FaceOFFx.Core.Abstractions;
using FaceOFFx.Core.Domain.Common;
using FaceOFFx.Core.Domain.Detection;
using FaceOFFx.Core.Domain.Transformations;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FaceOFFx.Infrastructure.Services;

/// <summary>
/// Face processing API for PIV and TWIC compatible image transformation
/// </summary>
[PublicAPI]
public static class FaceProcessor
{
    /// <summary>
    /// Process image bytes to PIV-compatible JPEG 2000 format
    /// </summary>
    /// <param name="imageData">Input image data (JPEG, PNG, etc.)</param>
    /// <param name="options">Processing options. Uses PIV standard if not specified.</param>
    /// <param name="logger">Optional logger for processing information. Uses NullLogger if not provided.</param>
    /// <returns>Processing result with encoded image data and metadata</returns>
    public static async Task<Result<ProcessingResult>> ProcessAsync(
        byte[] imageData,
        Maybe<ProcessingOptions> options = default,
        ILogger? logger = null
    )
    {
        if (imageData == null)
        {
            throw new ArgumentNullException(nameof(imageData));
        }

        var processingOptions = options.GetValueOrDefault(ProcessingOptions.PivBalanced);

        logger ??= NullLogger.Instance;

        return await LoadImage(imageData)
            .Bind(image => ProcessImageInternal(image, processingOptions, logger));
    }

    /// <summary>
    /// Process image bytes for TWIC card compatibility (14KB maximum)
    /// </summary>
    /// <param name="imageData">Input image data</param>
    /// <param name="logger">Optional logger for processing information. Uses NullLogger if not provided.</param>
    /// <returns>Processing result optimized for TWIC requirements</returns>
    /// <remarks>
    /// TWIC cards have strict size constraints. This method targets 14KB maximum
    /// to fit within the card storage limits.
    /// </remarks>
    public static async Task<Result<ProcessingResult>> ProcessForTwicAsync(
        byte[] imageData,
        ILogger? logger = null
    ) => await ProcessAsync(imageData, ProcessingOptions.TwicMax, logger);

    /// <summary>
    /// Process image bytes for PIV card compatibility (20KB target)
    /// </summary>
    /// <param name="imageData">Input image data</param>
    /// <param name="logger">Optional logger for processing information. Uses NullLogger if not provided.</param>
    /// <returns>Processing result optimized for PIV requirements</returns>
    /// <remarks>
    /// Standard PIV processing that balances file size with image quality.
    /// Suitable for most government ID card applications.
    /// </remarks>
    public static async Task<Result<ProcessingResult>> ProcessForPivAsync(
        byte[] imageData,
        ILogger? logger = null
    ) => await ProcessAsync(imageData, ProcessingOptions.PivBalanced, logger);

    /// <summary>
    /// Process image bytes with custom target file size
    /// </summary>
    /// <param name="imageData">Input image data</param>
    /// <param name="targetSizeBytes">Target file size in bytes</param>
    /// <param name="logger">Optional logger for processing information. Uses NullLogger if not provided.</param>
    /// <returns>Processing result targeting specified file size</returns>
    /// <remarks>
    /// Uses stepped compression rates to achieve the target file size.
    /// May not achieve exact size due to the discrete nature of JPEG 2000 compression.
    /// </remarks>
    public static async Task<Result<ProcessingResult>> ProcessToSizeAsync(
        byte[] imageData,
        int targetSizeBytes,
        ILogger? logger = null
    )
    {
        var options = ProcessingOptions.PivBalanced with
        {
            Strategy = EncodingStrategy.TargetSize(targetSizeBytes),
        };
        return await ProcessAsync(imageData, options, logger);
    }

    /// <summary>
    /// Process image bytes with fixed compression rate
    /// </summary>
    /// <param name="imageData">Input image data</param>
    /// <param name="compressionRate">Compression rate in bits per pixel</param>
    /// <param name="logger">Optional logger for processing information. Uses NullLogger if not provided.</param>
    /// <returns>Processing result using specified compression rate</returns>
    /// <remarks>
    /// Provides predictable compression behavior when file size constraints are flexible.
    /// Higher rates produce larger files with better quality.
    /// </remarks>
    public static async Task<Result<ProcessingResult>> ProcessWithRateAsync(
        byte[] imageData,
        float compressionRate,
        ILogger? logger = null
    )
    {
        if (compressionRate <= 0)
        {
            return Result.Failure<ProcessingResult>($"Invalid compression rate: {compressionRate}. Rate must be greater than 0.");
        }

        var options = ProcessingOptions.PivBalanced with
        {
            Strategy = EncodingStrategy.FixedRate(compressionRate),
        };
        return await ProcessAsync(imageData, options, logger);
    }

    private static Result<Image<Rgba32>> LoadImage(byte[] imageData)
    {
        return Result.Try(() =>
        {
            using var stream = new MemoryStream(imageData);
            return Image.Load<Rgba32>(stream);
        });
    }

    private static async Task<Result<ProcessingResult>> ProcessImageInternal(
        Image<Rgba32> image,
        ProcessingOptions options,
        ILogger logger
    )
    {
        using (image)
        {
            var servicesResult = CreateServices(logger);
            if (servicesResult.IsFailure)
                return Result.Failure<ProcessingResult>(servicesResult.Error);

            using var services = servicesResult.Value;
            return await ProcessWithServices(image, options, services, logger);
        }
    }

    private static Result<ProcessingServices> CreateServices(ILogger logger)
    {
        return Result.Try(() => new ProcessingServices(logger));
    }

    private static async Task<Result<ProcessingResult>> ProcessWithServices(
        Image<Rgba32> image,
        ProcessingOptions options,
        ProcessingServices services,
        ILogger logger
    )
    {
        var startTime = DateTime.UtcNow;

        // Step 1: Face detection
        logger.LogDebug("Starting face detection");
        var faceResult = await DetectPrimaryFace(image, options, services, logger);
        if (faceResult.IsFailure)
        {
            logger.LogWarning("Face detection failed: {Error}", faceResult.Error);
            return Result.Failure<ProcessingResult>(faceResult.Error);
        }

        var detectedFace = faceResult.Value;
        logger.LogDebug("Face detected with confidence: {Confidence}", detectedFace.Confidence);

        // Step 2: Transform to PIV format
        logger.LogDebug("Starting PIV transformation");
        var transformResult = await TransformImage(image, detectedFace, options, services, logger);
        if (transformResult.IsFailure)
        {
            logger.LogWarning("PIV transformation failed: {Error}", transformResult.Error);
            return Result.Failure<ProcessingResult>(transformResult.Error);
        }

        var (pivImage, roiSet, transformData) = transformResult.Value;

        // Step 3: Encode using strategy
        logger.LogDebug(
            "Starting JPEG 2000 encoding with strategy: {Strategy}",
            options.Strategy.GetType().Name
        );
        var encodingResult = options.Strategy.Execute(pivImage, roiSet, services.Encoder, options, logger);
        if (encodingResult.IsFailure)
        {
            logger.LogError("JPEG 2000 encoding failed: {Error}", encodingResult.Error);
            return Result.Failure<ProcessingResult>(encodingResult.Error);
        }

        var encoding = encodingResult.Value;
        var processingTime = DateTime.UtcNow - startTime;
        logger.LogInformation(
            "Processing completed successfully in {ProcessingTime}ms. Output size: {FileSize} bytes",
            processingTime.TotalMilliseconds,
            encoding.Data.Length
        );

        // Step 4: Build result
        var metadata = new ProcessingMetadata(
            transformData.OutputDimensions,
            transformData.RotationApplied,
            detectedFace.Confidence,
            encoding.Data.Length,
            processingTime
        )
        {
            CompressionRate = encoding.ActualRate,
            TargetSize = encoding.TargetSize,
            Warnings = transformData.Warnings,
            AdditionalData = transformData.AdditionalData,
        };

        return Result.Success(new ProcessingResult(encoding.Data, metadata));
    }

    private static async Task<Result<DetectedFace>> DetectPrimaryFace(
        Image<Rgba32> image,
        ProcessingOptions options,
        ProcessingServices services,
        ILogger logger
    )
    {
        var facesResult = await services.Detector.DetectFacesAsync(image);
        if (facesResult.IsFailure)
        {
            logger.LogWarning("Face detection service failed: {Error}", facesResult.Error);
            return Result.Failure<DetectedFace>(facesResult.Error);
        }

        var faces = facesResult.Value;
        logger.LogDebug("Detected {FaceCount} faces in image", faces.Count);

        if (faces.Count == 0)
        {
            logger.LogWarning("No faces detected in the image");
            return Result.Failure<DetectedFace>("No faces detected in the image");
        }

        if (options.RequireSingleFace && faces.Count > 1)
        {
            logger.LogWarning(
                "Multiple faces detected ({FaceCount}) but single face required",
                faces.Count
            );
            return Result.Failure<DetectedFace>(
                $"Multiple faces detected ({faces.Count}). Single face required."
            );
        }

        // Use highest confidence face
        var primaryFace = faces
            .Where(f => f.Confidence >= options.MinFaceConfidence)
            .OrderByDescending(f => f.Confidence)
            .FirstOrDefault();

        if (primaryFace != null)
        {
            logger.LogDebug(
                "Selected primary face with confidence: {Confidence}",
                primaryFace.Confidence
            );
            return Result.Success(primaryFace);
        }

        logger.LogWarning(
            "No faces meet minimum confidence threshold of {MinConfidence}. Best confidence: {BestConfidence}",
            options.MinFaceConfidence,
            faces.Max(f => f.Confidence)
        );
        return Result.Failure<DetectedFace>(
            $"No faces meet minimum confidence threshold of {options.MinFaceConfidence}"
        );
    }

    private static async Task<
        Result<(Image<Rgba32> PivImage, FacialRoiSet RoiSet, TransformationData Data)>
    > TransformImage(
        Image<Rgba32> image,
        DetectedFace face,
        ProcessingOptions options,
        ProcessingServices services,
        ILogger logger
    )
    {
        // Convert ProcessingOptions to PivProcessingOptions for existing pipeline
        var pivOptions = new PivProcessingOptions
        {
            BaseRate = 0.7f, // Not used in this path, encoding handled separately
            RoiStartLevel = options.RoiStartLevel,
            MinFaceConfidence = options.MinFaceConfidence,
            RequireSingleFace = options.RequireSingleFace,
            PreserveExifMetadata = options.PreserveMetadata,
        };

        // Use existing PivLandmarkProcessor
        var result = await PivLandmarkProcessor.ProcessAsync(
            image,
            face,
            services.LandmarkExtractor,
            pivOptions
        );

        if (result.IsFailure)
            return Result.Failure<(Image<Rgba32>, FacialRoiSet, TransformationData)>(result.Error);

        var pivData = result.Value;

        var transformData = new TransformationData(
            pivData.Dimensions,
            pivData.AppliedRotation,
            Array.Empty<string>(),
            new Dictionary<string, object>
            {
                ["ProcessingSummary"] = pivData.ProcessingSummary,
                ["FaceCrop"] = pivData.FaceCrop,
                ["ComplianceValidation"] = pivData.ComplianceValidation,
            }
        );

        return Result.Success((pivData.PivImage, pivData.RoiSet, transformData));
    }

    private sealed record TransformationData(
        ImageDimensions OutputDimensions,
        float RotationApplied,
        IReadOnlyList<string> Warnings,
        IReadOnlyDictionary<string, object> AdditionalData
    );
}

/// <summary>
/// Internal service container for face processing operations
/// </summary>
internal sealed class ProcessingServices : IDisposable
{
    public IFaceDetector Detector { get; }
    public ILandmarkExtractor LandmarkExtractor { get; }
    public IJpeg2000Encoder Encoder { get; }

    public ProcessingServices(ILogger logger)
    {
        // Services use NullLogger for now - main processing logic uses the provided logger
        Detector = new RetinaFaceDetector(NullLogger<RetinaFaceDetector>.Instance);
        LandmarkExtractor = new OnnxLandmarkExtractor(NullLogger<OnnxLandmarkExtractor>.Instance);
        Encoder = new Jpeg2000EncoderService(NullLogger<Jpeg2000EncoderService>.Instance);
    }

    public void Dispose()
    {
        if (Detector is IDisposable disposableDetector)
            disposableDetector.Dispose();
        if (LandmarkExtractor is IDisposable disposableExtractor)
            disposableExtractor.Dispose();
        // Encoder is stateless, no disposal needed
    }
}
