using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Standards;

/// <summary>
/// PIV standard constants and specifications for FIPS 201-3 compliant image processing.
/// </summary>
[PublicAPI]
public static class PivConstants
{
    /// <summary>
    /// Width of the PIV image in pixels as specified by FIPS 201-3.
    /// Standard width for all PIV-compliant facial images.
    /// </summary>
    public const int Width = 420;

    /// <summary>
    /// Height of the PIV image in pixels as specified by FIPS 201-3.
    /// Standard height for all PIV-compliant facial images.
    /// </summary>
    public const int Height = 560;

    /// <summary>
    /// Default JPEG 2000 ROI quality parameters for PIV-compliant image encoding.
    /// Controls compression rates and ROI priority levels for facial region preservation.
    /// </summary>
    [PublicAPI]
    public static class RoiQuality
    {
        /// <summary>
        /// Default base compression rate in bits per pixel (bpp).
        /// Value of 0.7 bpp produces approximately 20KB JPEG 2000 files for 420x560 images.
        /// </summary>
        public const float DefaultBaseRate = 0.7f;

        /// <summary>
        /// Default ROI resolution level priority for smoothest quality transitions.
        /// Level 3 provides the most gradual transition between ROI and background regions.
        /// Valid range: 0-3, where 0 is most aggressive and 3 is smoothest.
        /// </summary>
        public const int DefaultStartLevel = 3;

        /// <summary>
        /// Aggressive ROI priority level for maximum facial quality preservation.
        /// Level 0 applies strongest compression difference between face and background.
        /// May result in visible quality boundaries.
        /// </summary>
        public const int AggressiveStartLevel = 0;

        /// <summary>
        /// Conservative ROI priority level for balanced quality distribution.
        /// Level 2 provides moderate facial enhancement with softer transitions.
        /// Recommended for high-quality archival images.
        /// </summary>
        public const int ConservativeStartLevel = 2;

        /// <summary>
        /// Gets the default ROI quality parameters optimized for PIV compliance.
        /// Returns: (baseRate: 0.7 bpp, startLevel: 3) for ~20KB files with smooth transitions.
        /// </summary>
        public static (float baseRate, int startLevel) Default =>
            (DefaultBaseRate, DefaultStartLevel);

        /// <summary>
        /// Gets balanced ROI quality parameters for larger file sizes.
        /// Returns: (baseRate: 1.5 bpp, startLevel: 1) for ~45KB files with moderate ROI enhancement.
        /// Suitable for applications requiring higher overall quality.
        /// </summary>
        public static (float baseRate, int startLevel) Balanced => (1.5f, 1);

        /// <summary>
        /// Gets high quality ROI parameters for archival or forensic use.
        /// Returns: (baseRate: 2.0 bpp, startLevel: 2) for ~60KB files with conservative ROI.
        /// Provides maximum detail preservation across the entire image.
        /// </summary>
        public static (float baseRate, int startLevel) HighQuality => (2.0f, 2);
    }
}
