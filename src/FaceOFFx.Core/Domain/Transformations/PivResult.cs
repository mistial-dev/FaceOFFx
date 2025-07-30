using FaceOFFx.Core.Domain.Detection;
using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Represents the result of PIV (Personal Identity Verification) transformation processing,
/// containing the transformed image data and comprehensive metadata about the transformation.
/// </summary>
/// <remarks>
/// <para>
/// PivResult encapsulates all outputs from the PIV processing pipeline including:
/// - The final 420x560 pixel image data meeting FIPS 201-3 standards
/// - Transformation details (rotation, cropping, scaling)
/// - Source face information for traceability
/// - Processing metadata and warnings
/// - For JPEG 2000 format, includes ROI encoding
/// </para>
/// <para>
/// This record is immutable and uses required properties to ensure all necessary
/// information is provided when creating a result.
/// </para>
/// </remarks>
[PublicAPI]
public sealed record PivResult
{
    /// <summary>
    /// The PIV-compliant transformed image as encoded byte data.
    /// </summary>
    /// <value>
    /// A byte array containing the processed facial photograph encoded in the specified format,
    /// suitable for use in government ID cards and credentials.
    /// </value>
    /// <remarks>
    /// The image data meets all FIPS 201-3 requirements including proper face positioning,
    /// size (420x560), and orientation. For JPEG 2000 format, includes ROI encoding.
    /// </remarks>
    public required byte[] ImageData { get; init; }

    /// <summary>
    /// MIME type of the output image format.
    /// </summary>
    /// <value>
    /// Standard MIME type string such as "image/jpeg" or "image/png".
    /// </value>
    /// <remarks>
    /// PIV standards recommend JPEG format for optimal file size while maintaining
    /// quality. PNG may be used when lossless compression is required.
    /// </remarks>
    public required string MimeType { get; init; }

    /// <summary>
    /// Dimensions of the output image in pixels.
    /// </summary>
    /// <value>
    /// An <see cref="ImageDimensions"/> record containing width and height.
    /// For PIV compliance, this should always be 420x560 pixels.
    /// </value>
    public required ImageDimensions Dimensions { get; init; }

    /// <summary>
    /// The transformation parameters that were applied to produce the PIV-compliant image.
    /// </summary>
    /// <value>
    /// A <see cref="PivTransform"/> record containing rotation angle, crop region,
    /// scale factor, and other transformation details.
    /// </value>
    /// <remarks>
    /// This information is useful for understanding how the original image was
    /// modified and for potential reverse transformation if needed.
    /// </remarks>
    public required PivTransform AppliedTransform { get; init; }

    /// <summary>
    /// The original face detection result that was used as the basis for transformation.
    /// </summary>
    /// <value>
    /// A <see cref="DetectedFace"/> record containing the bounding box, confidence score,
    /// and other detection metadata from the source image.
    /// </value>
    /// <remarks>
    /// This provides traceability from the final PIV image back to the original
    /// face detection, useful for quality assurance and debugging.
    /// </remarks>
    public required DetectedFace SourceFace { get; init; }

    /// <summary>
    /// Gets a value indicating whether the result meets PIV compliance requirements.
    /// </summary>
    /// <value>
    /// <c>true</c> if the image meets all FIPS 201-3 requirements; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Compliance is determined by the applied transformation and includes checks for:
    /// - Correct dimensions (420x560)
    /// - Proper face positioning and size
    /// - Acceptable rotation correction (within ±5 degrees)
    /// </remarks>
    public bool IsPivCompliant => AppliedTransform.IsPivCompliant;

    /// <summary>
    /// Gets a human-readable summary of the transformations applied during processing.
    /// </summary>
    /// <value>
    /// A descriptive string explaining what operations were performed,
    /// such as "PIV transformation: rotated 2.3°, cropped to 85%, resized to 420x560".
    /// </value>
    public string ProcessingSummary { get; init; } = string.Empty;

    /// <summary>
    /// Gets a collection of warnings generated during processing.
    /// </summary>
    /// <value>
    /// A read-only list of warning messages. Empty if no warnings were generated.
    /// </value>
    /// <remarks>
    /// Warnings might include issues such as:
    /// - Low face detection confidence
    /// - Excessive rotation correction needed
    /// - Face positioning near edge of acceptable range
    /// - Image quality concerns
    /// </remarks>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets additional metadata about the processing operation.
    /// </summary>
    /// <value>
    /// A read-only dictionary containing key-value pairs of processing information.
    /// </value>
    /// <remarks>
    /// Common metadata includes:
    /// - "SourceDimensions": Original image dimensions
    /// - "FaceConfidence": Detection confidence score
    /// - "ProcessingOptions": Options used during processing
    /// - "ProcessingTime": Time taken to complete transformation
    /// </remarks>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } =
        new Dictionary<string, object>();

    /// <summary>
    /// Creates a successful PIV result with all required information.
    /// </summary>
    /// <param name="imageData">The encoded PIV-compliant image data.</param>
    /// <param name="mimeType">MIME type of the output format (e.g., "image/jpeg").</param>
    /// <param name="dimensions">Dimensions of the output image.</param>
    /// <param name="appliedTransform">The transformation that was applied.</param>
    /// <param name="sourceFace">The original detected face used for transformation.</param>
    /// <param name="summary">Optional processing summary. Auto-generated if null.</param>
    /// <param name="warnings">Optional list of warnings. Empty list if null.</param>
    /// <param name="metadata">Optional processing metadata. Empty dictionary if null.</param>
    /// <returns>A new <see cref="PivResult"/> instance with the provided information.</returns>
    /// <example>
    /// <code>
    /// var result = PivResult.Success(
    ///     imageBytes,
    ///     "image/jpeg",
    ///     new ImageDimensions(420, 560),
    ///     transform,
    ///     detectedFace,
    ///     metadata: new Dictionary&lt;string, object&gt;
    ///     {
    ///         ["ProcessingTime"] = "150ms"
    ///     });
    /// </code>
    /// </example>
    public static PivResult Success(
        byte[] imageData,
        string mimeType,
        ImageDimensions dimensions,
        PivTransform appliedTransform,
        DetectedFace sourceFace,
        string? summary = null,
        IReadOnlyList<string>? warnings = null,
        IReadOnlyDictionary<string, object>? metadata = null
    )
    {
        return new PivResult
        {
            ImageData = imageData,
            MimeType = mimeType,
            Dimensions = dimensions,
            AppliedTransform = appliedTransform,
            SourceFace = sourceFace,
            ProcessingSummary = summary ?? GenerateDefaultSummary(appliedTransform),
            Warnings = warnings ?? Array.Empty<string>(),
            Metadata = metadata ?? new Dictionary<string, object>(),
        };
    }

    /// <summary>
    /// Generates a default processing summary based on the applied transformation.
    /// </summary>
    /// <param name="transform">The transformation to summarize.</param>
    /// <returns>A human-readable summary of the operations performed.</returns>
    private static string GenerateDefaultSummary(PivTransform transform)
    {
        var operations = new List<string>();

        if (Math.Abs(transform.RotationDegrees) > 0.1f)
        {
            operations.Add($"rotated {transform.RotationDegrees:F1}°");
        }

        if (Math.Abs(transform.ScaleFactor - 1.0f) > 0.01f)
        {
            if (transform.ScaleFactor > 1.0f)
            {
                operations.Add($"upscaled {transform.ScaleFactor:F2}x");
            }
            else
            {
                operations.Add($"downscaled {transform.ScaleFactor:F2}x");
            }
        }

        var cropArea = transform.CropRegion.Width * transform.CropRegion.Height;
        if (cropArea < 0.99f) // Less than 99% means significant cropping
        {
            operations.Add($"cropped to {cropArea:P0}");
        }

        operations.Add(
            $"resized to {transform.TargetDimensions.Width}x{transform.TargetDimensions.Height}"
        );

        return operations.Any()
            ? $"Applied transformations: {string.Join(", ", operations)}"
            : "No transformations needed - image already PIV compliant";
    }

    /// <summary>
    /// Validates that this PIV result is complete and meets all requirements.
    /// </summary>
    /// <returns>
    /// A <see cref="Result"/> indicating success or containing an error message
    /// describing what validation failed.
    /// </returns>
    /// <remarks>
    /// Validation checks include:
    /// - Image data is not null or empty
    /// - MIME type is specified
    /// - Dimensions meet PIV minimum requirements (420x420)
    /// - Applied transform is valid
    /// </remarks>
    public Result Validate()
    {
        if (ImageData == null || ImageData.Length == 0)
        {
            return Result.Failure("Result image data is null or empty");
        }

        if (string.IsNullOrEmpty(MimeType))
        {
            return Result.Failure("MIME type is required");
        }

        if (Dimensions.Width < 420 || Dimensions.Height < 420)
        {
            return Result.Failure("Result dimensions do not meet PIV minimum requirements");
        }

        return AppliedTransform.Validate();
    }
}

/// <summary>
/// Configuration options for PIV (Personal Identity Verification) image processing.
/// </summary>
/// <remarks>
/// <para>
/// These options control various aspects of the PIV processing pipeline including:
/// - Output format and quality settings
/// - Face detection thresholds
/// - Metadata preservation preferences
/// </para>
/// <para>
/// Three preset configurations are provided:
/// - <see cref="Default"/>: Balanced settings for most use cases
/// - <see cref="HighQuality"/>: Maximum quality for archival purposes
/// - <see cref="Fast"/>: Optimized for speed with acceptable quality
/// </para>
/// </remarks>
[PublicAPI]
public sealed record PivProcessingOptions
{
    /// <summary>
    /// Gets the JPEG 2000 base compression rate in bits per pixel.
    /// </summary>
    /// <value>
    /// Base compression rate affecting overall image quality.
    /// Default is 0.7 bits per pixel for ~20KB files.
    /// </value>
    public float BaseRate { get; init; } = 0.7f;

    /// <summary>
    /// Gets the ROI resolution level priority setting.
    /// </summary>
    /// <value>
    /// Controls which resolution levels get ROI priority.
    /// 0 = aggressive ROI priority, 1 = balanced, 2 = conservative, 3 = smoothest transitions.
    /// Default is 3 (smoothest transitions).
    /// </value>
    public int RoiStartLevel { get; init; } = 3;

    /// <summary>
    /// Gets a value indicating whether to preserve EXIF metadata in the output image.
    /// </summary>
    /// <value>
    /// <c>true</c> to preserve metadata; <c>false</c> to strip it. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// PIV standards generally recommend removing EXIF data for privacy
    /// and to reduce file size. Enable only if metadata is specifically required.
    /// </remarks>
    public bool PreserveExifMetadata { get; init; } = false;

    /// <summary>
    /// Gets the minimum confidence threshold for face detection.
    /// </summary>
    /// <value>
    /// Confidence value from 0.0 to 1.0. Default is 0.8 (80% confidence).
    /// </value>
    /// <remarks>
    /// Higher values reduce false positives but may miss valid faces.
    /// For PIV compliance, a high confidence ensures only clear, frontal
    /// faces are processed.
    /// </remarks>
    public float MinFaceConfidence { get; init; } = 0.8f;

    /// <summary>
    /// Gets a value indicating whether to require exactly one face in the image.
    /// </summary>
    /// <value>
    /// <c>true</c> to fail if multiple faces are detected; <c>false</c> to use the highest confidence face. Default is <c>true</c>.
    /// </value>
    /// <remarks>
    /// PIV standards require a single subject per photograph. When true,
    /// processing fails if multiple faces are detected to prevent ambiguity.
    /// </remarks>
    public bool RequireSingleFace { get; init; } = true;

    /// <summary>
    /// Gets the default PIV processing options with balanced settings.
    /// </summary>
    /// <value>
    /// Options with JPEG output at 95% quality, 80% face confidence threshold,
    /// and single face requirement.
    /// </value>
    /// <remarks>
    /// Suitable for most PIV processing scenarios with good quality/performance balance.
    /// </remarks>
    public static PivProcessingOptions Default => new();

    /// <summary>
    /// Gets high quality processing options optimized for archival and maximum fidelity.
    /// </summary>
    /// <value>
    /// Options with higher base rate (2.0 bpp), conservative ROI priority,
    /// metadata preservation enabled, and 90% face confidence threshold.
    /// </value>
    /// <remarks>
    /// Use these settings when image quality is paramount, such as for
    /// official credential production or long-term archival storage.
    /// Results in larger file sizes but ensures maximum biometric accuracy.
    /// </remarks>
    public static PivProcessingOptions HighQuality =>
        new()
        {
            BaseRate = 2.0f,
            RoiStartLevel = 2,
            PreserveExifMetadata = true,
            MinFaceConfidence = 0.9f,
        };

    /// <summary>
    /// Gets fast processing options optimized for speed with acceptable quality.
    /// </summary>
    /// <value>
    /// Options with lower base rate (0.8 bpp), aggressive ROI priority,
    /// and 70% face confidence threshold.
    /// </value>
    /// <remarks>
    /// Use these settings for high-volume processing or preview generation
    /// where speed is more important than maximum quality. Still maintains
    /// PIV compliance but with faster processing.
    /// </remarks>
    public static PivProcessingOptions Fast =>
        new()
        {
            BaseRate = 0.8f,
            RoiStartLevel = 0,
            MinFaceConfidence = 0.7f,
        };
}
