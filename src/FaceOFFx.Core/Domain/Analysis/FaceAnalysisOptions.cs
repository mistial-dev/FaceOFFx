namespace FaceOFFx.Core.Domain.Analysis;

/// <summary>
/// Options for configuring face analysis behavior
/// </summary>
public record FaceAnalysisOptions
{
    /// <summary>
    /// Gets or sets whether to detect eye states (open/closed)
    /// </summary>
    public bool DetectEyeState { get; init; } = false;

    /// <summary>
    /// Gets or sets whether to perform liveness/anti-spoofing detection
    /// </summary>
    public bool DetectLiveness { get; init; } = false;

    /// <summary>
    /// Gets the default analysis options with all features disabled
    /// </summary>
    public static FaceAnalysisOptions Default => new();

    /// <summary>
    /// Gets basic analysis options (same as default, all features disabled)
    /// </summary>
    public static FaceAnalysisOptions BasicAnalysis => new();

    /// <summary>
    /// Gets full analysis options with all available features enabled
    /// </summary>
    public static FaceAnalysisOptions FullAnalysis =>
        new() { DetectEyeState = true, DetectLiveness = true };
}
