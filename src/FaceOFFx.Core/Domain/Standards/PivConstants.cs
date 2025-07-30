namespace FaceOFFx.Core.Domain.Standards;

/// <summary>
/// PIV standard constants and specifications.
/// </summary>
public static class PivConstants
{
    /// <summary>
    /// PIV image dimensions.
    /// </summary>
    public const int Width = 420;
    /// <summary>
    /// Height of the PIV image in pixels.
    /// </summary>
    public const int Height = 560;
    
    /// <summary>
    /// Default JPEG 2000 ROI quality parameters.
    /// </summary>
    public static class RoiQuality
    {
        /// <summary>
        /// Base compression rate (bits per pixel) - tuned for ~20KB level 3.
        /// </summary>
        public const float DefaultBaseRate = 0.7f;        // Base compression rate (bits per pixel) - tuned for ~20KB level 3
        /// <summary>
        /// Default ROI resolution level priority (conservative).
        /// </summary>
        public const int DefaultStartLevel = 3;           // ROI resolution level priority (conservative)
        /// <summary>
        /// Aggressive ROI priority.
        /// </summary>
        public const int AggressiveStartLevel = 0;        // Aggressive ROI priority 
        /// <summary>
        /// Conservative ROI priority.
        /// </summary>
        public const int ConservativeStartLevel = 2;      // Conservative ROI priority
        
        /// <summary>
        /// Gets the default ROI quality parameters (base rate and start level).
        /// </summary>
        public static (float baseRate, int startLevel) Default => (DefaultBaseRate, DefaultStartLevel);
        /// <summary>
        /// Gets balanced ROI quality parameters (medium base rate and start level).
        /// </summary>
        public static (float baseRate, int startLevel) Balanced => (1.5f, 1);
        /// <summary>
        /// Gets high quality ROI parameters (higher base rate and start level).
        /// </summary>
        public static (float baseRate, int startLevel) HighQuality => (2.0f, 2);
    }
    
    /// <summary>
    /// ROI margin percentages.
    /// </summary>
    public static class RoiMargins
    {
        public const float PeriocularMargin = 0.01f;      // 1% all around
        public const float OrofacialHorizontal = 0.12f;   // 12% horizontal (for ears)
        public const float OrofacialVertical = 0.05f;     // 5% vertical
    }
}
