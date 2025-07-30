using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Unified mapping between compression rates and file sizes for JPEG 2000 encoding
/// </summary>
/// <remarks>
/// Based on empirical testing with CoreJ2K encoder for 420x560 PIV images.
/// These mappings represent discrete quantization levels in the JPEG 2000 encoder.
/// </remarks>
[PublicAPI]
public static class CompressionMapping
{
    /// <summary>
    /// Compression rate to file size mappings based on JPEG 2000 quantization steps
    /// </summary>
    /// <remarks>
    /// Each entry represents a discrete quantization level in the CoreJ2K encoder.
    /// File sizes are approximate and may vary slightly between images.
    /// </remarks>
    public static readonly (float Rate, int ApproxSizeBytes, string Description)[] RateToSizeMap = new[]
    {
        (0.35f, 8_860, "Minimum quality (quantization level 1)"),
        (0.36f, 11_830, "PIV minimum 12KB target (quantization level 2)"),
        (0.46f, 14_780, "Low Quality (quantization level 3)"),
        (0.55f, 17_700, "Intermediate (quantization level 4)"),
        (0.68f, 20_650, "PIV balanced 20KB target (quantization level 5)"),
        (0.75f, 23_570, "Intermediate (quantization level 6)"),
        (0.85f, 26_440, "Intermediate (quantization level 7)"),
        (0.96f, 29_440, "PIV high 30KB target (quantization level 8)"),
        (1.10f, 32_370, "Higher quality (quantization level 9)"),
        (1.20f, 35_360, "Higher quality (quantization level 10)"),
        (1.30f, 38_300, "Higher quality (quantization level 11)"),
        (1.40f, 41_110, "Higher quality (quantization level 12)"),
        (1.50f, 44_000, "Higher quality (quantization level 13)"),
        (1.60f, 46_950, "Higher quality (quantization level 14)"),
        (1.70f, 50_060, "Higher quality (quantization level 15)"),
        (1.80f, 52_970, "Higher quality (quantization level 16)"),
        (1.90f, 55_930, "Higher quality (quantization level 17)"),
        (2.00f, 58_850, "Higher quality (quantization level 18)"),
        (2.50f, 73_060, "Maximum tested (quantization level 19)"),
    };

    /// <summary>
    /// Gets the compression rate for a target file size
    /// </summary>
    /// <param name="targetSizeBytes">Target file size in bytes</param>
    /// <returns>Recommended compression rate in bits per pixel</returns>
    public static float GetRateForTargetSize(int targetSizeBytes)
    {
        // Find the highest rate that produces a size <= target
        for (var i = RateToSizeMap.Length - 1; i >= 0; i--)
        {
            if (RateToSizeMap[i].ApproxSizeBytes <= targetSizeBytes)
            {
                return RateToSizeMap[i].Rate;
            }
        }

        // If target is smaller than minimum, return minimum rate
        return RateToSizeMap[0].Rate;
    }

    /// <summary>
    /// Gets the expected file size for a compression rate
    /// </summary>
    /// <param name="rate">Compression rate in bits per pixel</param>
    /// <returns>Expected file size in bytes</returns>
    public static int GetExpectedSizeForRate(float rate)
    {
        // Find exact match first
        foreach (var mapping in RateToSizeMap.Where(mapping => Math.Abs(mapping.Rate - rate) < 0.001f))
        {
            return mapping.ApproxSizeBytes;
        }

        // Otherwise, interpolate between nearest values
        for (var i = 0; i < RateToSizeMap.Length - 1; i++)
        {
            if (!(rate >= RateToSizeMap[i].Rate) || !(rate <= RateToSizeMap[i + 1].Rate))
            {
                continue;
            }

            var ratio = (rate - RateToSizeMap[i].Rate) /
                        (RateToSizeMap[i + 1].Rate - RateToSizeMap[i].Rate);
            var sizeRange = RateToSizeMap[i + 1].ApproxSizeBytes - RateToSizeMap[i].ApproxSizeBytes;
            return RateToSizeMap[i].ApproxSizeBytes + (int)(sizeRange * ratio);
        }

        // If rate is outside range, extrapolate
        if (rate < RateToSizeMap[0].Rate)
        {
            // Below minimum - use minimum size scaled down
            var ratio = rate / RateToSizeMap[0].Rate;
            return (int)(RateToSizeMap[0].ApproxSizeBytes * ratio);
        }
        else
        {
            // Above maximum - use maximum size scaled up
            var lastMapping = RateToSizeMap[^1];
            var ratio = rate / lastMapping.Rate;
            return (int)(lastMapping.ApproxSizeBytes * ratio);
        }
    }

    /// <summary>
    /// Gets the starting index for target size strategy
    /// </summary>
    /// <param name="targetSizeBytes">Target file size in bytes</param>
    /// <returns>Index in RateToSizeMap to start testing from</returns>
    public static int GetStartingIndexForTargetSize(int targetSizeBytes)
    {
        // Start 2 steps above the estimated rate to ensure we begin over target
        var estimatedRate = GetRateForTargetSize(targetSizeBytes);
        
        for (var i = 0; i < RateToSizeMap.Length; i++)
        {
            if (RateToSizeMap[i].Rate >= estimatedRate)
            {
                // Return index + 2, but cap at array length - 1
                return Math.Min(i + 2, RateToSizeMap.Length - 1);
            }
        }

        // Default to highest rate if target is very large
        return RateToSizeMap.Length - 1;
    }

    /// <summary>
    /// Gets all compression rates as an array
    /// </summary>
    /// <returns>Array of compression rates</returns>
    public static float[] GetAllRates()
    {
        return [.. RateToSizeMap.Select(static m => m.Rate)];
    }

    /// <summary>
    /// Gets the index of a compression rate in the rate map
    /// </summary>
    /// <param name="rate">Compression rate in bits per pixel</param>
    /// <returns>Index in RateToSizeMap, or closest index if exact match not found</returns>
    public static int GetIndexForRate(float rate)
    {
        // Find exact match first
        for (var i = 0; i < RateToSizeMap.Length; i++)
        {
            if (Math.Abs(RateToSizeMap[i].Rate - rate) < 0.001f)
            {
                return i;
            }
        }

        // Find closest rate if no exact match
        var closestIndex = 0;
        var closestDifference = Math.Abs(RateToSizeMap[0].Rate - rate);

        for (var i = 1; i < RateToSizeMap.Length; i++)
        {
            var difference = Math.Abs(RateToSizeMap[i].Rate - rate);
            if (difference < closestDifference)
            {
                closestIndex = i;
                closestDifference = difference;
            }
        }

        return closestIndex;
    }
}