using CSharpFunctionalExtensions;
using FaceOFFx.Core.Domain.Transformations;
using FaceOFFx.Infrastructure.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace FaceOFFx;

/// <summary>
/// Simplified facade for face processing with automatic service management
/// </summary>
[PublicAPI]
public static class SimpleFaceProcessor
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
    ) => await FaceProcessor.ProcessAsync(imageData, options, logger);

    /// <summary>
    /// Process image bytes for TWIC card compatibility (14KB maximum)
    /// </summary>
    /// <param name="imageData">Input image data</param>
    /// <param name="logger">Optional logger for processing information. Uses NullLogger if not provided.</param>
    /// <returns>Processing result optimized for TWIC requirements</returns>
    public static async Task<Result<ProcessingResult>> ProcessForTwicAsync(
        byte[] imageData,
        ILogger? logger = null
    ) => await FaceProcessor.ProcessForTwicAsync(imageData, logger);

    /// <summary>
    /// Process image bytes for PIV card compatibility (20KB target)
    /// </summary>
    /// <param name="imageData">Input image data</param>
    /// <param name="logger">Optional logger for processing information. Uses NullLogger if not provided.</param>
    /// <returns>Processing result optimized for PIV requirements</returns>
    public static async Task<Result<ProcessingResult>> ProcessForPivAsync(
        byte[] imageData,
        ILogger? logger = null
    ) => await FaceProcessor.ProcessForPivAsync(imageData, logger);

    /// <summary>
    /// Process image bytes with custom target file size
    /// </summary>
    /// <param name="imageData">Input image data</param>
    /// <param name="targetSizeBytes">Target file size in bytes</param>
    /// <param name="logger">Optional logger for processing information. Uses NullLogger if not provided.</param>
    /// <returns>Processing result targeting specified file size</returns>
    public static async Task<Result<ProcessingResult>> ProcessToSizeAsync(
        byte[] imageData,
        int targetSizeBytes,
        ILogger? logger = null
    ) => await FaceProcessor.ProcessToSizeAsync(imageData, targetSizeBytes, logger);

    /// <summary>
    /// Process image bytes with fixed compression rate
    /// </summary>
    /// <param name="imageData">Input image data</param>
    /// <param name="compressionRate">Compression rate in bits per pixel</param>
    /// <param name="logger">Optional logger for processing information. Uses NullLogger if not provided.</param>
    /// <returns>Processing result using specified compression rate</returns>
    public static async Task<Result<ProcessingResult>> ProcessWithRateAsync(
        byte[] imageData,
        float compressionRate,
        ILogger? logger = null
    ) => await FaceProcessor.ProcessWithRateAsync(imageData, compressionRate, logger);
}
