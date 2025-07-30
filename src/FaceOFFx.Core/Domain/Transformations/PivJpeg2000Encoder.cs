using CSharpFunctionalExtensions;

namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Encodes PIV images to JPEG 2000 format with ROI support.
/// This is a domain service that coordinates with infrastructure.
/// </summary>
public static class PivJpeg2000Encoder
{
    /// <summary>
    /// Encodes a PIV landmark result to JPEG 2000 with ROI.
    /// </summary>
    public static async Task<Result<byte[]>> EncodeAsync(
        PivLandmarkResult result,
        float baseRate = 1.2f,
        int roiStartLevel = 1
    )
    {
        // Use the clean PIV image without any visualization
        var pivImage = result.PivImage;
        var roiSet = result.RoiSet;

        // The actual encoding will be done by infrastructure service
        // This is just a domain-level coordinator
        return await Task.FromResult(
            Result.Success(Array.Empty<byte>()) // Placeholder - will integrate with infrastructure
        );
    }

    /// <summary>
    /// Creates a JPEG 2000 filename from the original filename.
    /// </summary>
    public static string GenerateOutputFilename(string originalPath)
    {
        var directory = Path.GetDirectoryName(originalPath) ?? ".";
        var nameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
        return Path.Combine(directory, $"{nameWithoutExt}_piv.jp2");
    }
}
