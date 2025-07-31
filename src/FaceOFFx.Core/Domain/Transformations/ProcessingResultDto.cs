using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Result of face processing operation for public API
/// </summary>
/// <param name="ImageData">Processed image data in JPEG 2000 format</param>
/// <param name="Metadata">Processing metadata and statistics</param>
[PublicAPI]
public sealed record ProcessingResultDto(byte[] ImageData, ProcessingMetadataDto Metadata);

/// <summary>
/// Metadata about the processing operation for public API
/// </summary>
/// <param name="OutputDimensions">Final image dimensions</param>
/// <param name="RotationApplied">Rotation applied to correct face orientation (degrees)</param>
/// <param name="FaceConfidence">Confidence score of the detected face (0.0 to 1.0)</param>
/// <param name="FileSize">Final compressed file size in bytes</param>
/// <param name="ProcessingTime">Total time taken for processing</param>
[PublicAPI]
public sealed record ProcessingMetadataDto(
    ImageDimensions OutputDimensions,
    float RotationApplied,
    float FaceConfidence,
    int FileSize,
    TimeSpan ProcessingTime
)
{
    /// <summary>
    /// Gets the actual compression rate used during encoding
    /// </summary>
    /// <value>Compression rate in bits per pixel</value>
    public float CompressionRate { get; init; }

    /// <summary>
    /// Gets the target file size if using target size strategy
    /// </summary>
    /// <value>Target size in bytes, or null if using fixed rate strategy</value>
    public int? TargetSize { get; init; }

    /// <summary>
    /// Gets any warnings generated during processing
    /// </summary>
    /// <value>List of warning messages, empty if no warnings</value>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets additional processing context data
    /// </summary>
    /// <value>Dictionary of additional metadata for advanced scenarios</value>
    public IReadOnlyDictionary<string, object> AdditionalData { get; init; } =
        new Dictionary<string, object>();
};
