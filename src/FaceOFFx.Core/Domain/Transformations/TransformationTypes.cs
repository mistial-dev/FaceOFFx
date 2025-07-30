namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Represents a rotation correction to level the face
/// </summary>
public sealed record RotationCorrection
{
    /// <summary>
    /// Gets the rotation angle in degrees to correct face orientation
    /// </summary>
    public float Degrees { get; }
    
    private RotationCorrection(float degrees) => Degrees = degrees;
    
    /// <summary>
    /// Creates a rotation correction
    /// </summary>
    /// <param name="degrees">Rotation angle in degrees (normalized to -180 to 180)</param>
    /// <returns>Success with RotationCorrection if valid, otherwise failure</returns>
    public static Result<RotationCorrection> Create(float degrees)
    {
        if (float.IsNaN(degrees) || float.IsInfinity(degrees))
        {
            return Result.Failure<RotationCorrection>("Rotation degrees must be a valid number");
        }

        // Normalize to -180 to 180 range
        var normalized = degrees % 360;
        if (normalized > 180)
        {
            normalized -= 360;
        }
        if (normalized < -180)
        {
            normalized += 360;
        }

        return Result.Success(new RotationCorrection(normalized));
    }
    
    public static RotationCorrection None => new(0f);
    
    public bool IsSignificant => Math.Abs(Degrees) > 0.1f;
}

/// <summary>
/// Represents a scale correction to achieve target measurements
/// </summary>
public sealed record ScaleCorrection
{
    public float Factor { get; }
    
    private ScaleCorrection(float factor) => Factor = factor;
    
    public static Result<ScaleCorrection> Create(float factor)
    {
        if (factor <= 0 || float.IsNaN(factor) || float.IsInfinity(factor))
        {
            return Result.Failure<ScaleCorrection>("Scale factor must be a positive number");
        }

        if (factor is < 0.1f or > 10f)
        {
            return Result.Failure<ScaleCorrection>("Scale factor must be between 0.1 and 10");
        }

        return Result.Success(new ScaleCorrection(factor));
    }
    
    public static ScaleCorrection None => new(1f);
    
    public bool IsSignificant => Math.Abs(Factor - 1f) > 0.01f;
}

/// <summary>
/// Represents a crop specification with normalized coordinates (0-1)
/// </summary>
public sealed record CropSpecification
{
    public float Left { get; }
    public float Top { get; }
    public float Width { get; }
    public float Height { get; }
    
    private CropSpecification(float left, float top, float width, float height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }
    
    public static Result<CropSpecification> Create(float left, float top, float width, float height)
    {
        if (left is < 0 or >= 1)
        {
            return Result.Failure<CropSpecification>("Left must be between 0 and 1");
        }
        if (top is < 0 or >= 1)
        {
            return Result.Failure<CropSpecification>("Top must be between 0 and 1");
        }
        if (width is <= 0 or > 1)
        {
            return Result.Failure<CropSpecification>("Width must be between 0 and 1");
        }
        if (height is <= 0 or > 1)
        {
            return Result.Failure<CropSpecification>("Height must be between 0 and 1");
        }
        if (left + width > 1)
        {
            return Result.Failure<CropSpecification>("Crop extends beyond right edge");
        }
        if (top + height > 1)
        {
            return Result.Failure<CropSpecification>("Crop extends beyond bottom edge");
        }

        return Result.Success(new CropSpecification(left, top, width, height));
    }
    
    public static CropSpecification Full => new(0f, 0f, 1f, 1f);
    
    public bool IsSignificant => Left > 0.001f || Top > 0.001f || 
                                 Width < 0.999f || Height < 0.999f;
}

/// <summary>
/// Complete PIV transformation plan with all corrections
/// </summary>
public sealed record PivTransformationPlan
{
    public required RotationCorrection Rotation { get; init; }
    public required ScaleCorrection Scale { get; init; }
    public required CropSpecification Crop { get; init; }
    public required TransformationReason Reason { get; init; }
    public required QualityImpactEstimate QualityImpact { get; init; }
    
    public static PivTransformationPlan NoCorrection => new()
    {
        Rotation = RotationCorrection.None,
        Scale = ScaleCorrection.None,
        Crop = CropSpecification.Full,
        Reason = TransformationReason.NoViolations,
        QualityImpact = QualityImpactEstimate.None
    };
    
    public bool RequiresTransformation => 
        Rotation.IsSignificant || Scale.IsSignificant || Crop.IsSignificant;
}

/// <summary>
/// Reason for applying transformations
/// </summary>
public sealed record TransformationReason
{
    public string Description { get; }
    public IReadOnlyList<string> ViolationsAddressed { get; }
    
    private TransformationReason(string description, IReadOnlyList<string> violations)
    {
        Description = description;
        ViolationsAddressed = violations;
    }
    
    public static TransformationReason ForViolations(IReadOnlyList<string> violations)
        => new($"Correcting {violations.Count} PIV violations", violations);
        
    public static TransformationReason NoViolations 
        => new("No corrections needed", Array.Empty<string>());
        
    public static TransformationReason OptimizationOnly
        => new("Optimizing image for better PIV compliance", Array.Empty<string>());
}

/// <summary>
/// Estimated quality impact of transformations
/// </summary>
public sealed record QualityImpactEstimate
{
    public float EstimatedQualityLoss { get; }
    public bool IsLossless { get; }
    public string Explanation { get; }
    
    private QualityImpactEstimate(float qualityLoss, bool isLossless, string explanation)
    {
        EstimatedQualityLoss = qualityLoss;
        IsLossless = isLossless;
        Explanation = explanation;
    }
    
    public static QualityImpactEstimate None 
        => new(0f, true, "No transformations applied");
        
    public static QualityImpactEstimate ForTransformations(
        RotationCorrection rotation,
        ScaleCorrection scale,
        CropSpecification crop)
    {
        var qualityLoss = 0f;
        var explanations = new List<string>();
        
        // Rotation causes interpolation artifacts
        if (rotation.IsSignificant)
        {
            var rotationLoss = Math.Min(Math.Abs(rotation.Degrees) / 180f * 0.15f, 0.15f);
            qualityLoss += rotationLoss;
            explanations.Add($"Rotation by {rotation.Degrees:F1}°");
        }
        
        // Scaling impacts quality based on factor
        if (scale.IsSignificant)
        {
            var scaleLoss = scale.Factor > 1f 
                ? 0f // Upscaling doesn't lose quality (but doesn't add either)
                : (1f - scale.Factor) * 0.2f; // Downscaling loses up to 20%
            qualityLoss += scaleLoss;
            explanations.Add($"Scaling by {scale.Factor:F2}x");
        }
        
        // Cropping is lossless for retained pixels
        if (crop.IsSignificant)
        {
            explanations.Add("Cropping applied");
        }
        
        var isLossless = qualityLoss < 0.001f;
        var explanation = explanations.Any() 
            ? string.Join(", ", explanations)
            : "No significant transformations";
            
        return new(qualityLoss, isLossless, explanation);
    }
}

/// <summary>
/// Dimensions of an image
/// </summary>
public sealed record ImageDimensions(int Width, int Height)
{
    public float AspectRatio => (float)Width / Height;
    
    public static Result<ImageDimensions> Create(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return Result.Failure<ImageDimensions>("Image dimensions must be positive");
        }

        return Result.Success(new ImageDimensions(width, height));
    }
}

/// <summary>
/// PIV aspect ratio specification
/// </summary>
public sealed record PivAspectRatio
{
    public float Value { get; }
    public float WidthToHeightRatio => Value;
    
    private PivAspectRatio(float value) => Value = value;
    
    public static Result<PivAspectRatio> Create(float value)
    {
        if (value <= 0 || float.IsNaN(value) || float.IsInfinity(value))
        {
            return Result.Failure<PivAspectRatio>("Aspect ratio must be a positive number");
        }

        if (value is < 0.5f or > 2f)
        {
            return Result.Failure<PivAspectRatio>("Aspect ratio must be between 0.5 and 2.0");
        }

        return Result.Success(new PivAspectRatio(value));
    }
    
    public static PivAspectRatio Standard => new(0.75f); // 3:4 ratio
}