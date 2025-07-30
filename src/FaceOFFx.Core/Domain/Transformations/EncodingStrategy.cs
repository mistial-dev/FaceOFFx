using CSharpFunctionalExtensions;
using FaceOFFx.Core.Abstractions;
using FaceOFFx.Core.Domain.Detection;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Encoding strategy for JPEG 2000 compression
/// </summary>
[PublicAPI]
public abstract record EncodingStrategy
{
    /// <summary>
    /// Creates a fixed compression rate strategy
    /// </summary>
    /// <param name="rate">Compression rate in bits per pixel</param>
    /// <returns>Fixed rate encoding strategy</returns>
    public static EncodingStrategy FixedRate(float rate) => new FixedRateStrategy(rate);

    /// <summary>
    /// Creates a target file size strategy
    /// </summary>
    /// <param name="sizeBytes">Target file size in bytes</param>
    /// <returns>Target size encoding strategy</returns>
    public static EncodingStrategy TargetSize(int sizeBytes) => new TargetSizeStrategy(sizeBytes);

    /// <summary>
    /// Execute the encoding strategy
    /// </summary>
    /// <param name="image">Image to encode</param>
    /// <param name="roiSet">ROI regions for prioritized encoding</param>
    /// <param name="encoder">JPEG 2000 encoder</param>
    /// <param name="options">Processing options</param>
    /// <param name="logger">Logger for debug output</param>
    /// <returns>Encoding result with compressed data and metadata</returns>
    public abstract Result<EncodingResult> Execute(
        Image<Rgba32> image,
        FacialRoiSet roiSet,
        IJpeg2000Encoder encoder,
        ProcessingOptions options,
        ILogger? logger = null
    );
}

/// <summary>
/// Fixed compression rate strategy
/// </summary>
/// <param name="Rate">Compression rate in bits per pixel</param>
[PublicAPI]
public sealed record FixedRateStrategy(float Rate) : EncodingStrategy
{
    /// <summary>
    /// Execute fixed rate encoding
    /// </summary>
    public override Result<EncodingResult> Execute(
        Image<Rgba32> image,
        FacialRoiSet roiSet,
        IJpeg2000Encoder encoder,
        ProcessingOptions options,
        ILogger? logger = null
    )
    {
        return encoder
            .EncodeWithRoi(
                image,
                roiSet,
                Rate,
                options.RoiStartLevel,
                options.EnableRoi,
                options.AlignRoi
            )
            .Map(data => new EncodingResult(data, Rate, Maybe<int>.None));
    }
}

/// <summary>
/// Target file size strategy with stepped compression rates
/// </summary>
/// <param name="TargetBytes">Target file size in bytes</param>
[PublicAPI]
public sealed record TargetSizeStrategy(int TargetBytes) : EncodingStrategy
{
    /// <summary>
    /// Compression rates optimized for 420x560 PIV images based on JPEG 2000 quantization steps
    /// Based on empirical testing with CoreJ2K encoder discrete quantization levels
    /// Each rate represents the lowest value that achieves the target quantization level
    /// File sizes are approximate and may vary slightly between images
    /// </summary>
    private static readonly float[] CompressionSteps = new[]
    {
        0.35f,  // ~8,860 bytes - quantization level 1 (minimum quality)
        0.36f,  // ~11,830 bytes - quantization level 2 (good for PIV minimum 12KB target)
        0.46f,  // ~14,780 bytes - quantization level 3 (exceeds TWIC 14KB limit) 
        0.55f,  // ~17,700 bytes - quantization level 4 (intermediate)
        0.68f,  // ~20,650 bytes - quantization level 5 (good for PIV balanced 20KB target)
        0.75f,  // ~23,570 bytes - quantization level 6 (intermediate)
        0.85f,  // ~26,440 bytes - quantization level 7 (intermediate)
        0.96f,  // ~29,440 bytes - quantization level 8 (good for PIV high 30KB target)
        1.10f,  // ~32,370 bytes - quantization level 9
        1.20f,  // ~35,360 bytes - quantization level 10
        1.30f,  // ~38,300 bytes - quantization level 11
        1.40f,  // ~41,110 bytes - quantization level 12
        1.50f,  // ~44,000 bytes - quantization level 13
        1.60f,  // ~46,950 bytes - quantization level 14
        1.70f,  // ~50,060 bytes - quantization level 15
        1.80f,  // ~52,970 bytes - quantization level 16
        1.90f,  // ~55,930 bytes - quantization level 17
        2.00f,  // ~58,850 bytes - quantization level 18
        2.50f,  // ~73,060 bytes - quantization level 19 (maximum tested)
    };

    /// <summary>
    /// Execute target size encoding using stepped compression rates
    /// </summary>
    public override Result<EncodingResult> Execute(
        Image<Rgba32> image,
        FacialRoiSet roiSet,
        IJpeg2000Encoder encoder,
        ProcessingOptions options,
        ILogger? logger = null
    )
    {
        logger ??= NullLogger.Instance;
        var bestResult = Maybe<(byte[] Data, float Rate)>.None;
        var bestSize = 0; // Start with 0 to find the largest size that fits

        // Calculate estimated starting rate based on target size
        var estimatedIndex = EstimateStartingRateIndex(TargetBytes);
        
        // Start two steps above the estimated rate and work backwards
        var startingIndex = Math.Min(estimatedIndex + 2, CompressionSteps.Length - 1);
        
        logger.LogDebug("TargetSizeStrategy: Target={TargetBytes} bytes", TargetBytes);
        logger.LogDebug("Estimated rate index: {EstimatedIndex} (rate={EstimatedRate})", estimatedIndex, CompressionSteps[estimatedIndex]);
        logger.LogDebug("Starting index: {StartingIndex} (rate={StartingRate})", startingIndex, CompressionSteps[startingIndex]);
        logger.LogDebug("Will test rates backwards from index {StartingIndex} to 0", startingIndex);
        
        // Work backwards from starting index to ensure we start over target
        for (var i = startingIndex; i >= 0; i--)
        {
            var rate = CompressionSteps[i];
            logger.LogDebug("Trying rate {Rate} (index {Index})...", rate, i);
            
            var result = TryEncodeAtRate(rate, image, roiSet, encoder, options, logger);
            
            if (result.HasValue)
            {
                var (encodedData, size) = result.Value;
                logger.LogDebug("Rate {Rate} produced {Size} bytes", rate, size);
                
                // If we hit target closely (within 5%), use this rate immediately
                if (size <= TargetBytes && size > TargetBytes * 0.95)
                {
                    logger.LogDebug("Rate {Rate} is close enough to target ({Size} bytes), using immediately", rate, size);
                    return Result.Success(new EncodingResult(encodedData, rate, TargetBytes));
                }

                // Track best result that fits within target
                if (size <= TargetBytes && (bestResult.HasNoValue || size > bestSize))
                {
                    logger.LogDebug("Rate {Rate} fits under target, updating best result (was {OldSize}, now {NewSize})", rate, bestSize, size);
                    bestResult = (encodedData, rate);
                    bestSize = size;
                }
                else if (size <= TargetBytes)
                {
                    logger.LogDebug("Rate {Rate} fits under target but not better than current best ({BestSize})", rate, bestSize);
                }
                else
                {
                    logger.LogDebug("Rate {Rate} is over target ({Size} > {TargetBytes})", rate, size, TargetBytes);
                }
            }
            else
            {
                logger.LogDebug("Rate {Rate} encoding failed", rate);
            }
        }
        
        logger.LogDebug("Finished trying all rates. Best result: {BestResult}", bestResult.HasValue ? $"{bestSize} bytes" : "NONE");

        return bestResult.HasValue
            ? Result.Success(
                new EncodingResult(bestResult.Value.Data, bestResult.Value.Rate, TargetBytes)
            )
            : Result.Failure<EncodingResult>(
                $"Cannot compress image to {TargetBytes} bytes. Try a larger target size."
            );
    }

    /// <summary>
    /// Estimates the starting compression rate index based on target file size
    /// </summary>
    /// <param name="targetBytes">Target file size in bytes</param>
    /// <returns>Index in CompressionSteps array to start testing from</returns>
    private static int EstimateStartingRateIndex(int targetBytes)
    {
        // Direct mapping for all target sizes based on verified data
        var estimatedRate = targetBytes switch
        {
            <= 9000 => 0.35f,   // ~8,850 bytes -> 0.35 bpp
            <= 12000 => 0.40f,  // ~11,825 bytes -> 0.40 bpp (PIV min)
            <= 15000 => 0.46f,  // ~14,775 bytes -> 0.46 bpp (TWIC max)
            <= 18000 => 0.55f,  // ~17,700 bytes -> 0.55 bpp
            <= 21000 => 0.70f,  // ~20,635 bytes -> 0.70 bpp (PIV balanced)
            <= 24000 => 0.75f,  // ~23,570 bytes -> 0.75 bpp (extrapolated)
            <= 27000 => 0.85f,  // ~26,445 bytes -> 0.85 bpp
            <= 30000 => 1.00f,  // ~29,430 bytes -> 1.00 bpp (PIV high)
            <= 45000 => 1.50f,  // Larger files
            <= 60000 => 2.00f,
            <= 90000 => 3.00f,
            <= 120000 => 4.00f,
            <= 150000 => 5.00f,
            _ => 6.00f           // Very large files
        };

        // Find the closest rate in our steps array
        for (var i = 0; i < CompressionSteps.Length; i++)
        {
            if (CompressionSteps[i] >= estimatedRate)
                return i;
        }

        // Default to highest rate if target is very large
        return CompressionSteps.Length - 1;
    }

    /// <summary>
    /// Attempts to encode at a specific compression rate
    /// </summary>
    /// <param name="rate">Compression rate to try</param>
    /// <param name="image">Image to encode</param>
    /// <param name="roiSet">ROI regions</param>
    /// <param name="encoder">JPEG 2000 encoder</param>
    /// <param name="options">Processing options</param>
    /// <param name="logger">Logger for debug output</param>
    /// <returns>Encoded data and size if successful</returns>
    private static Maybe<(byte[] Data, int Size)> TryEncodeAtRate(
        float rate,
        Image<Rgba32> image,
        FacialRoiSet roiSet,
        IJpeg2000Encoder encoder,
        ProcessingOptions options,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        logger.LogDebug("TryEncodeAtRate: Calling encoder.EncodeWithRoi for rate {Rate}", rate);
        
        // Clone the image before encoding to prevent disposal issues
        // The JPEG 2000 encoder may dispose the image after encoding
        using var imageClone = image.Clone();
        logger.LogDebug("TryEncodeAtRate: Created image clone for rate {Rate}", rate);
        
        var result = encoder.EncodeWithRoi(
            imageClone,
            roiSet,
            rate,
            options.RoiStartLevel,
            options.EnableRoi,
            options.AlignRoi
        );

        if (result.IsFailure)
        {
            logger.LogDebug("TryEncodeAtRate: Encoder returned failure for rate {Rate}: {Error}", rate, result.Error);
            return Maybe<(byte[], int)>.None;
        }
        
        var encodedData = result.Value;
        if (encodedData == null)
        {
            logger.LogDebug("TryEncodeAtRate: Encoder returned null data for rate {Rate}", rate);
            return Maybe<(byte[], int)>.None;
        }
        logger.LogDebug("TryEncodeAtRate: Success for rate {Rate}, data length: {Length}", rate, encodedData.Length);
        return Maybe<(byte[], int)>.From((encodedData, encodedData.Length));
    }
}

/// <summary>
/// Result of encoding operation
/// </summary>
/// <param name="Data">Encoded image data</param>
/// <param name="ActualRate">Compression rate used</param>
/// <param name="TargetSize">Target size if using target size strategy</param>
[PublicAPI]
public sealed record EncodingResult(byte[] Data, float ActualRate, Maybe<int> TargetSize);
