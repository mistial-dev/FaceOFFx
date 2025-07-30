using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Processing options for face image transformation and encoding
/// </summary>
[PublicAPI]
public sealed record ProcessingOptions
{
    /// <summary>
    /// Gets the minimum confidence threshold for face detection
    /// </summary>
    /// <value>Confidence value from 0.0 to 1.0. Default is 0.8 (80% confidence).</value>
    public float MinFaceConfidence { get; init; } = 0.8f;

    /// <summary>
    /// Gets a value indicating whether to require exactly one face in the image
    /// </summary>
    /// <value>True to fail if multiple faces are detected; false to use the highest confidence face. Default is true.</value>
    public bool RequireSingleFace { get; init; } = true;

    /// <summary>
    /// Gets the maximum number of retry attempts for processing
    /// </summary>
    /// <value>Number of retry attempts. Default is 2.</value>
    public int MaxRetries { get; init; } = 2;

    /// <summary>
    /// Gets the timeout for processing operations
    /// </summary>
    /// <value>Maximum time allowed for processing. Default is 30 seconds.</value>
    public TimeSpan ProcessingTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets a value indicating whether to preserve EXIF metadata in the output image
    /// </summary>
    /// <value>True to preserve metadata; false to strip it. Default is false.</value>
    public bool PreserveMetadata { get; init; } = false;

    /// <summary>
    /// Gets the ROI resolution level priority setting
    /// </summary>
    /// <value>
    /// Controls which resolution levels get ROI priority.
    /// 0 = aggressive ROI priority, 1 = balanced, 2 = conservative, 3 = smoothest transitions.
    /// Default is 3 (smoothest transitions).
    /// </value>
    public int RoiStartLevel { get; init; } = 3;

    /// <summary>
    /// Gets a value indicating whether to enable ROI encoding
    /// </summary>
    /// <value>True to enable ROI encoding for enhanced facial quality. Default is true.</value>
    public bool EnableRoi { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to align ROI regions with compression blocks
    /// </summary>
    /// <value>True to align ROI regions; false for smoother quality transitions. Default is false.</value>
    public bool AlignRoi { get; init; } = false;

    /// <summary>
    /// Gets the maximum rotation angle allowed for face alignment
    /// </summary>
    /// <value>Maximum rotation in degrees. Default is 15.0 degrees (allows processing of moderately tilted input images).</value>
    public float MaxRotationDegrees { get; init; } = 15.0f;

    /// <summary>
    /// Gets the encoding strategy for JPEG 2000 compression
    /// </summary>
    /// <value>Strategy determining how the image is compressed. Default is 0.7 bpp fixed rate.</value>
    public EncodingStrategy Strategy { get; init; } = EncodingStrategy.FixedRate(0.7f);

    /// <summary>
    /// TWIC maximum: 14KB target for TWIC card compatibility
    /// </summary>
    /// <value>
    /// Processing options optimized for Transportation Worker Identification Credential (TWIC) cards.
    /// Targets 14KB maximum file size to fit within card storage constraints.
    /// </value>
    public static ProcessingOptions TwicMax =>
        new() { Strategy = EncodingStrategy.TargetSize(14000), RoiStartLevel = 3 };

    /// <summary>
    /// PIV minimum: 12KB target for cards with minimum available space
    /// </summary>
    /// <value>
    /// Processing options for PIV cards with minimal available space for facial images.
    /// Targets 12KB to ensure compatibility with any PIV card, even those with
    /// the minimum required space allocation for facial image storage.
    /// </value>
    public static ProcessingOptions PivMin =>
        new() { Strategy = EncodingStrategy.TargetSize(12000), RoiStartLevel = 3 };

    /// <summary>
    /// PIV balanced: 20KB target for optimal quality/size balance
    /// </summary>
    /// <value>
    /// Balanced processing options for Personal Identity Verification (PIV) cards.
    /// Targets 20KB file size with optimal balance of quality and compression.
    /// Recommended for most PIV applications.
    /// </value>
    public static ProcessingOptions PivBalanced =>
        new() { Strategy = EncodingStrategy.TargetSize(20000), RoiStartLevel = 3 };

    /// <summary>
    /// PIV high quality: 30KB target for enhanced PIV compatibility
    /// </summary>
    /// <value>
    /// High quality processing options for PIV cards where file size constraints allow.
    /// Targets 30KB file size with enhanced facial detail preservation.
    /// </value>
    public static ProcessingOptions PivHigh =>
        new() { Strategy = EncodingStrategy.TargetSize(30000), RoiStartLevel = 3 };

    /// <summary>
    /// PIV very high quality: 50KB target for premium quality
    /// </summary>
    /// <value>
    /// Very high quality processing options targeting 50KB file size.
    /// Provides excellent facial detail preservation while maintaining reasonable file sizes.
    /// Suitable for applications requiring superior image quality without archival requirements.
    /// </value>
    public static ProcessingOptions PivVeryHigh =>
        new() { Strategy = EncodingStrategy.TargetSize(50000), RoiStartLevel = 3 };

    /// <summary>
    /// Archival: 4.0 bpp fixed rate for long-term storage
    /// </summary>
    /// <value>
    /// Archival quality processing options for long-term storage and preservation.
    /// Uses fixed 4.0 bpp compression rate with metadata preservation and strict quality controls.
    /// </value>
    public static ProcessingOptions Archival =>
        new()
        {
            Strategy = EncodingStrategy.FixedRate(4.0f),
            RoiStartLevel = 3,
            PreserveMetadata = true,
            MinFaceConfidence = 0.95f,
        };

    /// <summary>
    /// Minimal file size: 0.5 bpp fixed rate for smallest storage footprint
    /// </summary>
    /// <value>
    /// Minimal file size preset with aggressive compression (0.5 bpp).
    /// Produces files around 15KB with reduced quality.
    /// Suitable when storage space is the primary concern.
    /// </value>
    public static ProcessingOptions Minimal =>
        new()
        {
            Strategy = EncodingStrategy.FixedRate(0.5f),
            RoiStartLevel = 3,
        };

    /// <summary>
    /// Fast processing: Fail-fast behavior with minimal retries
    /// </summary>
    /// <value>
    /// Fast processing preset with reduced retries and shorter timeout.
    /// Same quality as default settings but fails quickly on errors.
    /// Suitable for high-throughput batch processing where speed matters more than success rate.
    /// </value>
    public static ProcessingOptions Fast => PivBalanced with
    {
        MaxRetries = 1,
        ProcessingTimeout = TimeSpan.FromSeconds(10),
    };
}
