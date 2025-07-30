using CSharpFunctionalExtensions;

namespace FaceOFFx.Core.Domain.Standards;

/// <summary>
/// Represents the validation results for PIV compliance according to INCITS 385-2004 Section 8.
/// </summary>
/// <param name="IsAAAligned">True if nose and mouth centers align with the vertical center line</param>
/// <param name="IsBBPositioned">True if eye line is positioned 50-70% from bottom edge</param>
/// <param name="IsCCRatioValid">True if head width meets minimum 7:4 ratio (image width : head width)</param>
/// <param name="IsFullyCompliant">True if all PIV requirements are met</param>
/// <param name="AADeviation">Deviation of face center line from image center in pixels</param>
/// <param name="BBFromBottom">Actual percentage of eye line from bottom edge (0.5-0.7 is compliant)</param>
/// <param name="CCRatio">Current image width : head width ratio (must be ≥ 1.75 for 7:4)</param>
/// <param name="HeadWidthPixels">Actual head width in pixels</param>
/// <param name="MinRequiredHeadWidth">Minimum required head width for current image size</param>
/// <param name="Issues">List of specific compliance issues found</param>
/// <param name="Recommendations">List of recommended corrections</param>
public record PivComplianceValidation(
    bool IsAAAligned,
    bool IsBBPositioned,
    bool IsCCRatioValid,
    bool IsFullyCompliant,
    float AADeviation,
    float BBFromBottom,
    float CCRatio,
    float HeadWidthPixels,
    float MinRequiredHeadWidth,
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> Recommendations)
{
    /// <summary>
    /// PIV compliance thresholds and constants.
    /// </summary>
    public static class Thresholds
    {
        /// <summary>Maximum allowed deviation from center line in pixels.</summary>
        public const float MaxCenterDeviation = 10.0f;
        
        /// <summary>Minimum percentage from bottom for eye line.</summary>
        public const float MinEyeFromBottom = 0.50f;
        
        /// <summary>Maximum percentage from bottom for eye line.</summary>
        public const float MaxEyeFromBottom = 0.70f;
        
        /// <summary>Optimal percentage from bottom for eye line.</summary>
        public const float OptimalEyeFromBottom = 0.60f;
        
        /// <summary>Minimum image width to head width ratio (7:4 = 1.75).</summary>
        public const float MinWidthRatio = 7.0f / 4.0f; // 1.75
        
        /// <summary>Optimal image width to head width ratio.</summary>
        public const float OptimalWidthRatio = 1.85f;
        
        /// <summary>Maximum allowed alignment deviation for nose/mouth centers.</summary>
        public const float MaxAlignmentDeviation = 8.0f;
    }
    
    /// <summary>
    /// Creates a PIV compliance validation from PIV lines and image dimensions.
    /// </summary>
    /// <param name="lines">The PIV compliance lines</param>
    /// <param name="imageWidth">Width of the image</param>
    /// <param name="imageHeight">Height of the image</param>
    /// <returns>Complete validation results</returns>
    public static PivComplianceValidation Validate(PivComplianceLines lines, int imageWidth, int imageHeight)
    {
        var issues = new List<string>();
        var recommendations = new List<string>();
        
        // Validate Line AA (Vertical Center Line)
        var aaDeviation = lines.GetCenterLineDeviation(imageWidth);
        var isAAAligned = Math.Abs(aaDeviation) <= Thresholds.MaxCenterDeviation && 
                          lines.AreNoseAndMouthAligned(Thresholds.MaxAlignmentDeviation);
        
        if (!isAAAligned)
        {
            if (Math.Abs(aaDeviation) > Thresholds.MaxCenterDeviation)
            {
                issues.Add($"Face center line deviates {aaDeviation:F1}px from image center (max: {Thresholds.MaxCenterDeviation}px)");
                recommendations.Add($"Adjust horizontal crop to center face (shift {-aaDeviation:F1}px)");
            }
            
            if (!lines.AreNoseAndMouthAligned(Thresholds.MaxAlignmentDeviation))
            {
                var noseMouthDev = Math.Abs(lines.NoseCenter.X - lines.MouthCenter.X);
                issues.Add($"Nose and mouth not aligned: {noseMouthDev:F1}px deviation (max: {Thresholds.MaxAlignmentDeviation}px)");
                recommendations.Add("Check facial landmark detection quality or face pose");
            }
        }
        
        // Validate Line BB (Horizontal Eye Line)
        var bbFromBottom = lines.GetEyeLinePercentageFromBottom(imageHeight);
        var isBBPositioned = bbFromBottom >= Thresholds.MinEyeFromBottom && 
                            bbFromBottom <= Thresholds.MaxEyeFromBottom;
                            
        if (!isBBPositioned)
        {
            if (bbFromBottom < Thresholds.MinEyeFromBottom)
            {
                issues.Add($"Eyes too low: {bbFromBottom:P1} from bottom (min: {Thresholds.MinEyeFromBottom:P1})");
                recommendations.Add($"Adjust vertical crop to raise eye position by {(Thresholds.OptimalEyeFromBottom - bbFromBottom) * imageHeight:F0}px");
            }
            else
            {
                issues.Add($"Eyes too high: {bbFromBottom:P1} from bottom (max: {Thresholds.MaxEyeFromBottom:P1})");
                recommendations.Add($"Adjust vertical crop to lower eye position by {(bbFromBottom - Thresholds.OptimalEyeFromBottom) * imageHeight:F0}px");
            }
        }
        
        // Validate Line CC (Head Width Ratio)
        var ccRatio = lines.GetImageToHeadWidthRatio(imageWidth);
        var minRequiredHeadWidth = imageWidth / Thresholds.MinWidthRatio;
        // Use proper floating point comparison with epsilon tolerance
        const float epsilon = 0.001f; // Small tolerance for floating point precision
        var isCCRatioValid = ccRatio >= (Thresholds.MinWidthRatio - epsilon) || 
                            Math.Abs(ccRatio - Thresholds.MinWidthRatio) < epsilon;
        
        if (!isCCRatioValid)
        {
            issues.Add($"Head width ratio {ccRatio:F2} below minimum {Thresholds.MinWidthRatio:F2} (7:4)");
            issues.Add($"Head width {lines.LineCC_Width:F0}px below minimum {minRequiredHeadWidth:F0}px");
            recommendations.Add($"Increase head size in crop or use higher resolution source image");
        }
        
        var isFullyCompliant = isAAAligned && isBBPositioned && isCCRatioValid;
        
        if (isFullyCompliant)
        {
            recommendations.Add("Image meets all PIV compliance requirements");
        }
        
        return new PivComplianceValidation(
            isAAAligned,
            isBBPositioned,
            isCCRatioValid,
            isFullyCompliant,
            aaDeviation,
            bbFromBottom,
            ccRatio,
            lines.LineCC_Width,
            minRequiredHeadWidth,
            issues,
            recommendations);
    }
    
    /// <summary>
    /// Gets a severity level for the compliance issues.
    /// </summary>
    public ComplianceSeverity Severity
    {
        get
        {
            if (IsFullyCompliant) return ComplianceSeverity.Compliant;
            if (Issues.Count >= 3) return ComplianceSeverity.Critical;
            if (!IsCCRatioValid) return ComplianceSeverity.High;
            if (!IsBBPositioned) return ComplianceSeverity.Medium;
            return ComplianceSeverity.Low;
        }
    }
    
    /// <summary>
    /// Gets a summary of compliance status for logging.
    /// </summary>
    public string Summary => IsFullyCompliant 
        ? "PIV compliant" 
        : $"PIV non-compliant: {Issues.Count} issues ({Severity} severity)";
        
    /// <summary>
    /// Gets detailed compliance report for debugging.
    /// </summary>
    public string DetailedReport
    {
        get
        {
            var report = new List<string>
            {
                $"PIV Compliance Report:",
                $"  Line AA (Center): {(IsAAAligned ? "✓" : "✗")} Deviation: {AADeviation:F1}px",
                $"  Line BB (Eyes): {(IsBBPositioned ? "✓" : "✗")} Position: {BBFromBottom:P1} from bottom",
                $"  Line CC (Width): {(IsCCRatioValid ? "✓" : "✗")} Ratio: {CCRatio:F2} (head: {HeadWidthPixels:F0}px)",
                $"  Overall: {(IsFullyCompliant ? "COMPLIANT" : $"NON-COMPLIANT ({Severity})")}"
            };
            
            if (Issues.Any())
            {
                report.Add("  Issues:");
                report.AddRange(Issues.Select(issue => $"    - {issue}"));
            }
            
            if (Recommendations.Any())
            {
                report.Add("  Recommendations:");
                report.AddRange(Recommendations.Select(rec => $"    - {rec}"));
            }
            
            return string.Join(Environment.NewLine, report);
        }
    }
}

/// <summary>
/// Severity levels for PIV compliance issues.
/// </summary>
public enum ComplianceSeverity
{
    /// <summary>Fully compliant with all requirements.</summary>
    Compliant,
    
    /// <summary>Minor deviations that don't affect core requirements.</summary>
    Low,
    
    /// <summary>Moderate issues that may affect recognition quality.</summary>
    Medium,
    
    /// <summary>Significant deviations from requirements.</summary>
    High,
    
    /// <summary>Critical failures that prevent PIV compliance.</summary>
    Critical
}