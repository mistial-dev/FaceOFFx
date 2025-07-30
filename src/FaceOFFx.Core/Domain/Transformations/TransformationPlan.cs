namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Complete transformation plan with all operations
/// </summary>
public sealed record TransformationPlan
{
    /// <summary>
    /// Rotation to apply (if any)
    /// </summary>
    public required RotationAngle Rotation { get; init; }
    
    /// <summary>
    /// Scale factor to apply
    /// </summary>
    public required ScaleFactor Scale { get; init; }
    
    /// <summary>
    /// Crop region to apply
    /// </summary>
    public required CropRegion Crop { get; init; }
    
    /// <summary>
    /// Target dimensions for the final image
    /// </summary>
    public required ImageDimensions TargetDimensions { get; init; }
    
    /// <summary>
    /// Reason for these transformations
    /// </summary>
    public required string Reason { get; init; }
    
    /// <summary>
    /// Whether any transformations are needed
    /// </summary>
    public bool RequiresTransformation => 
        Rotation.IsSignificant || Scale.IsSignificant || Crop.IsSignificant;
    
    /// <summary>
    /// Creates a no-op transformation plan
    /// </summary>
    public static TransformationPlan NoTransformation(ImageDimensions currentDimensions) => new()
    {
        Rotation = RotationAngle.Zero,
        Scale = ScaleFactor.Identity,
        Crop = CropRegion.Full,
        TargetDimensions = currentDimensions,
        Reason = "No transformation required"
    };
    
    /// <summary>
    /// Validates the plan is internally consistent
    /// </summary>
    public Result Validate()
    {
        // Ensure target dimensions are achievable
        var (_, _, cropWidth, cropHeight) = Crop.ToPixels(100, 100);
        var scaledWidth = (int)(cropWidth * Scale.Value);
        var scaledHeight = (int)(cropHeight * Scale.Value);
        
        // Allow some tolerance for rounding
        var widthRatio = (float)scaledWidth / 100 * TargetDimensions.Width / scaledWidth;
        var heightRatio = (float)scaledHeight / 100 * TargetDimensions.Height / scaledHeight;
        
        if (Math.Abs(widthRatio - 1f) > 0.1f || Math.Abs(heightRatio - 1f) > 0.1f)
        {
            return Result.Failure(
                "Transformation plan is inconsistent: scaled crop dimensions don't match target");
        }

        return Result.Success();
    }
}