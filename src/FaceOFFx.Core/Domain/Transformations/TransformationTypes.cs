using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Represents a rotation correction to level the face for PIV compliance.
/// This correction ensures the face is properly aligned with the horizontal axis,
/// which is critical for meeting FIPS 201-3 facial image requirements.
/// </summary>
/// <remarks>
/// Rotation corrections are applied to ensure eyes are level and the face is upright.
/// The rotation angle is normalized to the range of -180 to 180 degrees.
/// Small rotations (less than 0.1 degrees) are considered insignificant.
/// </remarks>
[PublicAPI]
public sealed record RotationCorrection
{
    /// <summary>
    /// Gets the rotation angle in degrees to correct face orientation.
    /// Positive values indicate clockwise rotation, negative values indicate counter-clockwise.
    /// </summary>
    /// <value>The rotation angle in degrees, normalized to -180 to 180 range.</value>
    public float Degrees { get; }

    private RotationCorrection(float degrees) => Degrees = degrees;

    /// <summary>
    /// Creates a rotation correction with validation and normalization.
    /// </summary>
    /// <param name="degrees">Rotation angle in degrees. Will be normalized to -180 to 180 range.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the <see cref="RotationCorrection"/> if valid,
    /// otherwise a failure result with an error message.
    /// </returns>
    /// <example>
    /// <code>
    /// var rotationResult = RotationCorrection.Create(15.5f);
    /// if (rotationResult.IsSuccess)
    /// {
    ///     var rotation = rotationResult.Value;
    ///     Console.WriteLine($"Rotation: {rotation.Degrees}°");
    /// }
    /// </code>
    /// </example>
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

    /// <summary>
    /// Gets a rotation correction representing no rotation (0 degrees).
    /// </summary>
    /// <value>A <see cref="RotationCorrection"/> instance with 0 degrees rotation.</value>
    [PublicAPI]
    public static RotationCorrection None => new(0f);

    /// <summary>
    /// Gets a value indicating whether this rotation correction is significant enough to apply.
    /// </summary>
    /// <value>
    /// <c>true</c> if the absolute rotation angle is greater than 0.1 degrees; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Small rotations below 0.1 degrees are considered negligible and may be ignored
    /// to avoid unnecessary image processing that could introduce interpolation artifacts.
    /// </remarks>
    [PublicAPI]
    public bool IsSignificant => Math.Abs(Degrees) > 0.1f;
}

/// <summary>
/// Represents a scale correction to achieve target PIV facial measurements.
/// Scale corrections ensure the face meets the required size specifications
/// as defined in FIPS 201-3 (minimum 240 pixels face width).
/// </summary>
/// <remarks>
/// Scale factors are multiplicative: values greater than 1.0 enlarge the image,
/// values less than 1.0 reduce it. Scale factors are constrained between 0.1 and 10.0
/// to prevent extreme transformations that would degrade image quality.
/// </remarks>
[PublicAPI]
public sealed record ScaleCorrection
{
    /// <summary>
    /// Gets the scale factor to apply to the image.
    /// </summary>
    /// <value>
    /// The multiplicative scale factor. Values greater than 1.0 enlarge the image,
    /// values less than 1.0 reduce it. Always positive and within the range [0.1, 10.0].
    /// </value>
    [PublicAPI]
    public float Factor { get; }

    private ScaleCorrection(float factor) => Factor = factor;

    /// <summary>
    /// Creates a scale correction with validation.
    /// </summary>
    /// <param name="factor">
    /// The scale factor to apply. Must be a positive number between 0.1 and 10.0.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the <see cref="ScaleCorrection"/> if valid,
    /// otherwise a failure result with an error message.
    /// </returns>
    /// <example>
    /// <code>
    /// var scaleResult = ScaleCorrection.Create(1.5f); // Scale up by 50%
    /// if (scaleResult.IsSuccess)
    /// {
    ///     var scale = scaleResult.Value;
    ///     Console.WriteLine($"Scale factor: {scale.Factor}x");
    /// }
    /// </code>
    /// </example>
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

    /// <summary>
    /// Gets a scale correction representing no scaling (factor of 1.0).
    /// </summary>
    /// <value>A <see cref="ScaleCorrection"/> instance with a scale factor of 1.0.</value>
    [PublicAPI]
    public static ScaleCorrection None => new(1f);

    /// <summary>
    /// Gets a value indicating whether this scale correction is significant enough to apply.
    /// </summary>
    /// <value>
    /// <c>true</c> if the scale factor differs from 1.0 by more than 0.01; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Scale factors very close to 1.0 (within 1%) are considered negligible and may be ignored
    /// to avoid unnecessary image resampling that could introduce interpolation artifacts.
    /// </remarks>
    [PublicAPI]
    public bool IsSignificant => Math.Abs(Factor - 1f) > 0.01f;
}

/// <summary>
/// Represents a crop specification with normalized coordinates for image cropping operations.
/// All coordinates and dimensions are normalized to the range [0, 1] relative to the source image.
/// </summary>
/// <remarks>
/// This record uses normalized coordinates where (0, 0) represents the top-left corner
/// and (1, 1) represents the bottom-right corner of the source image. This allows
/// crop specifications to be resolution-independent and easily applied to images of any size.
/// </remarks>
[PublicAPI]
public sealed record CropSpecification
{
    /// <summary>
    /// Gets the normalized X-coordinate of the left edge of the crop region.
    /// </summary>
    /// <value>A value between 0 and 1, where 0 is the left edge of the image.</value>
    [PublicAPI]
    public float Left { get; }

    /// <summary>
    /// Gets the normalized Y-coordinate of the top edge of the crop region.
    /// </summary>
    /// <value>A value between 0 and 1, where 0 is the top edge of the image.</value>
    [PublicAPI]
    public float Top { get; }

    /// <summary>
    /// Gets the normalized width of the crop region.
    /// </summary>
    /// <value>A value between 0 and 1, where 1 represents the full width of the image.</value>
    [PublicAPI]
    public float Width { get; }

    /// <summary>
    /// Gets the normalized height of the crop region.
    /// </summary>
    /// <value>A value between 0 and 1, where 1 represents the full height of the image.</value>
    [PublicAPI]
    public float Height { get; }

    private CropSpecification(float left, float top, float width, float height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Creates a crop specification with validation to ensure all values are within valid ranges.
    /// </summary>
    /// <param name="left">The normalized X-coordinate of the left edge (0-1).</param>
    /// <param name="top">The normalized Y-coordinate of the top edge (0-1).</param>
    /// <param name="width">The normalized width of the crop region (0-1).</param>
    /// <param name="height">The normalized height of the crop region (0-1).</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the <see cref="CropSpecification"/> if valid,
    /// otherwise a failure result with an error message.
    /// </returns>
    /// <remarks>
    /// The method validates that:
    /// - All coordinates are within [0, 1] range
    /// - Width and height are positive
    /// - The crop region doesn't extend beyond the image boundaries
    /// </remarks>
    /// <example>
    /// <code>
    /// // Crop the center 50% of the image
    /// var cropResult = CropSpecification.Create(0.25f, 0.25f, 0.5f, 0.5f);
    /// if (cropResult.IsSuccess)
    /// {
    ///     var crop = cropResult.Value;
    ///     // Apply crop to image...
    /// }
    /// </code>
    /// </example>
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

    /// <summary>
    /// Gets a crop specification representing the full image (no cropping).
    /// </summary>
    /// <value>
    /// A <see cref="CropSpecification"/> with coordinates (0, 0) and dimensions (1, 1),
    /// representing the entire image.
    /// </value>
    [PublicAPI]
    public static CropSpecification Full => new(0f, 0f, 1f, 1f);

    /// <summary>
    /// Gets a value indicating whether this crop specification represents a significant crop operation.
    /// </summary>
    /// <value>
    /// <c>true</c> if any edge is cropped by more than 0.1% or dimensions are less than 99.9% of the original;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Very small crops (less than 0.1% on any edge) are considered negligible and may be ignored
    /// to avoid unnecessary processing overhead.
    /// </remarks>
    [PublicAPI]
    public bool IsSignificant => Left > 0.001f || Top > 0.001f || Width < 0.999f || Height < 0.999f;
}

/// <summary>
/// Represents a complete transformation plan for achieving PIV compliance.
/// This plan encapsulates all necessary corrections (rotation, scale, crop) along with
/// the reasons for transformation and estimated quality impact.
/// </summary>
/// <remarks>
/// The transformation plan is the result of analyzing an image against PIV requirements
/// and determining what corrections are needed. The plan includes metadata about why
/// transformations are necessary and their expected impact on image quality.
/// </remarks>
[PublicAPI]
public sealed record PivTransformationPlan
{
    /// <summary>
    /// Gets the rotation correction to apply for leveling the face.
    /// </summary>
    /// <value>The <see cref="RotationCorrection"/> to apply, or <see cref="RotationCorrection.None"/> if no rotation is needed.</value>
    [PublicAPI]
    public required RotationCorrection Rotation { get; init; }

    /// <summary>
    /// Gets the scale correction to apply for achieving target face dimensions.
    /// </summary>
    /// <value>The <see cref="ScaleCorrection"/> to apply, or <see cref="ScaleCorrection.None"/> if no scaling is needed.</value>
    [PublicAPI]
    public required ScaleCorrection Scale { get; init; }

    /// <summary>
    /// Gets the crop specification for centering and framing the face.
    /// </summary>
    /// <value>The <see cref="CropSpecification"/> to apply, or <see cref="CropSpecification.Full"/> if no cropping is needed.</value>
    [PublicAPI]
    public required CropSpecification Crop { get; init; }

    /// <summary>
    /// Gets the reason why these transformations are being applied.
    /// </summary>
    /// <value>A <see cref="TransformationReason"/> describing the violations addressed or optimization goals.</value>
    [PublicAPI]
    public required TransformationReason Reason { get; init; }

    /// <summary>
    /// Gets the estimated quality impact of applying these transformations.
    /// </summary>
    /// <value>A <see cref="QualityImpactEstimate"/> describing potential quality loss and explanations.</value>
    [PublicAPI]
    public required QualityImpactEstimate QualityImpact { get; init; }

    /// <summary>
    /// Gets a transformation plan representing no corrections needed.
    /// </summary>
    /// <value>
    /// A <see cref="PivTransformationPlan"/> with no rotation, scaling, or cropping,
    /// indicating the image already meets PIV requirements.
    /// </value>
    [PublicAPI]
    public static PivTransformationPlan NoCorrection =>
        new()
        {
            Rotation = RotationCorrection.None,
            Scale = ScaleCorrection.None,
            Crop = CropSpecification.Full,
            Reason = TransformationReason.NoViolations,
            QualityImpact = QualityImpactEstimate.None,
        };

    /// <summary>
    /// Gets a value indicating whether this plan requires any transformations to be applied.
    /// </summary>
    /// <value>
    /// <c>true</c> if any of the rotation, scale, or crop corrections are significant;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property is useful for determining whether image processing is necessary
    /// or if the original image can be used as-is.
    /// </remarks>
    [PublicAPI]
    public bool RequiresTransformation =>
        Rotation.IsSignificant || Scale.IsSignificant || Crop.IsSignificant;
}

/// <summary>
/// Describes the reason for applying transformations to achieve PIV compliance.
/// This record provides context about why transformations are necessary and what
/// specific violations they address.
/// </summary>
/// <remarks>
/// Transformation reasons help document the decision-making process and provide
/// transparency about why an image is being modified. This is particularly important
/// for audit trails and quality assurance in PIV processing systems.
/// </remarks>
[PublicAPI]
public sealed record TransformationReason
{
    /// <summary>
    /// Gets a human-readable description of why transformations are being applied.
    /// </summary>
    /// <value>A descriptive string explaining the transformation rationale.</value>
    [PublicAPI]
    public string Description { get; }

    /// <summary>
    /// Gets the list of specific PIV violations that these transformations address.
    /// </summary>
    /// <value>
    /// A read-only list of violation descriptions. Empty if transformations are
    /// for optimization only or if no violations exist.
    /// </value>
    [PublicAPI]
    public IReadOnlyList<string> ViolationsAddressed { get; }

    private TransformationReason(string description, IReadOnlyList<string> violations)
    {
        Description = description;
        ViolationsAddressed = violations;
    }

    /// <summary>
    /// Creates a transformation reason for addressing specific PIV violations.
    /// </summary>
    /// <param name="violations">The list of PIV violations that need to be corrected.</param>
    /// <returns>A <see cref="TransformationReason"/> describing the violations to be addressed.</returns>
    /// <example>
    /// <code>
    /// var violations = new[] { "Face rotation exceeds 5 degrees", "Face width below 240 pixels" };
    /// var reason = TransformationReason.ForViolations(violations);
    /// Console.WriteLine(reason.Description); // "Correcting 2 PIV violations"
    /// </code>
    /// </example>
    [PublicAPI]
    public static TransformationReason ForViolations(IReadOnlyList<string> violations) =>
        new($"Correcting {violations.Count} PIV violations", violations);

    /// <summary>
    /// Gets a transformation reason indicating no violations need correction.
    /// </summary>
    /// <value>A <see cref="TransformationReason"/> indicating the image already meets requirements.</value>
    [PublicAPI]
    public static TransformationReason NoViolations =>
        new("No corrections needed", Array.Empty<string>());

    /// <summary>
    /// Gets a transformation reason for optional optimizations that improve PIV compliance.
    /// </summary>
    /// <value>
    /// A <see cref="TransformationReason"/> indicating transformations are for
    /// optimization rather than violation correction.
    /// </value>
    /// <remarks>
    /// Use this when applying transformations that improve image quality or
    /// positioning even though the image technically meets minimum requirements.
    /// </remarks>
    [PublicAPI]
    public static TransformationReason OptimizationOnly =>
        new("Optimizing image for better PIV compliance", Array.Empty<string>());
}

/// <summary>
/// Represents an estimate of the quality impact from applying image transformations.
/// This helps users understand the trade-offs between PIV compliance and image quality.
/// </summary>
/// <remarks>
/// Quality impact estimates are based on the types and magnitudes of transformations applied.
/// Rotation and scaling typically introduce interpolation artifacts, while cropping is lossless
/// for the retained portion of the image.
/// </remarks>
[PublicAPI]
public sealed record QualityImpactEstimate
{
    /// <summary>
    /// Gets the estimated percentage of quality loss from transformations.
    /// </summary>
    /// <value>
    /// A value between 0.0 and 1.0 representing the estimated quality loss,
    /// where 0.0 means no loss and 1.0 means complete loss.
    /// </value>
    [PublicAPI]
    public float EstimatedQualityLoss { get; }

    /// <summary>
    /// Gets a value indicating whether the transformations are lossless.
    /// </summary>
    /// <value>
    /// <c>true</c> if no quality loss is expected (e.g., only cropping);
    /// otherwise, <c>false</c>.
    /// </value>
    [PublicAPI]
    public bool IsLossless { get; }

    /// <summary>
    /// Gets a human-readable explanation of the quality impact.
    /// </summary>
    /// <value>A descriptive string explaining what transformations affect quality and how.</value>
    [PublicAPI]
    public string Explanation { get; }

    private QualityImpactEstimate(float qualityLoss, bool isLossless, string explanation)
    {
        EstimatedQualityLoss = qualityLoss;
        IsLossless = isLossless;
        Explanation = explanation;
    }

    /// <summary>
    /// Gets a quality impact estimate representing no transformations.
    /// </summary>
    /// <value>A <see cref="QualityImpactEstimate"/> with zero quality loss.</value>
    [PublicAPI]
    public static QualityImpactEstimate None => new(0f, true, "No transformations applied");

    /// <summary>
    /// Calculates the quality impact for a set of transformations.
    /// </summary>
    /// <param name="rotation">The rotation correction to apply.</param>
    /// <param name="scale">The scale correction to apply.</param>
    /// <param name="crop">The crop specification to apply.</param>
    /// <returns>
    /// A <see cref="QualityImpactEstimate"/> describing the cumulative quality impact
    /// of all transformations.
    /// </returns>
    /// <remarks>
    /// Quality loss estimation algorithm:
    /// - Rotation: Up to 15% loss based on angle (larger angles cause more interpolation)
    /// - Scaling down: Up to 20% loss based on factor (more downscaling loses more detail)
    /// - Scaling up: No quality loss (but no quality gain either)
    /// - Cropping: Lossless for retained pixels
    /// </remarks>
    /// <example>
    /// <code>
    /// var impact = QualityImpactEstimate.ForTransformations(
    ///     RotationCorrection.Create(5f).Value,
    ///     ScaleCorrection.Create(0.8f).Value,
    ///     CropSpecification.Full
    /// );
    /// Console.WriteLine($"Quality loss: {impact.EstimatedQualityLoss:P}");
    /// Console.WriteLine($"Explanation: {impact.Explanation}");
    /// </code>
    /// </example>
    [PublicAPI]
    public static QualityImpactEstimate ForTransformations(
        RotationCorrection rotation,
        ScaleCorrection scale,
        CropSpecification crop
    )
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
            var scaleLoss =
                scale.Factor > 1f
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
/// Represents the dimensions of an image in pixels.
/// </summary>
/// <param name="Width">The width of the image in pixels.</param>
/// <param name="Height">The height of the image in pixels.</param>
/// <remarks>
/// This record provides a type-safe way to represent image dimensions and
/// calculate derived properties like aspect ratio. All dimensions must be positive integers.
/// </remarks>
[PublicAPI]
public sealed record ImageDimensions(int Width, int Height)
{
    /// <summary>
    /// Gets the aspect ratio of the image (width divided by height).
    /// </summary>
    /// <value>
    /// The aspect ratio as a floating-point number. For example, a 4:3 image
    /// returns approximately 1.333, while a 3:4 image returns 0.75.
    /// </value>
    [PublicAPI]
    public float AspectRatio => (float)Width / Height;

    /// <summary>
    /// Creates image dimensions with validation.
    /// </summary>
    /// <param name="width">The width in pixels. Must be positive.</param>
    /// <param name="height">The height in pixels. Must be positive.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the <see cref="ImageDimensions"/> if valid,
    /// otherwise a failure result with an error message.
    /// </returns>
    /// <example>
    /// <code>
    /// var dimensionsResult = ImageDimensions.Create(420, 560);
    /// if (dimensionsResult.IsSuccess)
    /// {
    ///     var dims = dimensionsResult.Value;
    ///     Console.WriteLine($"Image: {dims.Width}x{dims.Height}, AR: {dims.AspectRatio:F2}");
    /// }
    /// </code>
    /// </example>
    [PublicAPI]
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
/// Represents an aspect ratio specification for PIV-compliant images.
/// PIV standards require a 3:4 aspect ratio (0.75) for facial images.
/// </summary>
/// <remarks>
/// The PIV aspect ratio is critical for compliance with FIPS 201-3 requirements.
/// While the standard ratio is 0.75 (3:4, or 420x560 pixels), this type allows
/// for validation of other ratios within reasonable bounds.
/// </remarks>
[PublicAPI]
public sealed record PivAspectRatio
{
    /// <summary>
    /// Gets the aspect ratio value as width divided by height.
    /// </summary>
    /// <value>The aspect ratio as a decimal. Standard PIV ratio is 0.75.</value>
    [PublicAPI]
    public float Value { get; }

    /// <summary>
    /// Gets the width-to-height ratio (same as Value, provided for clarity).
    /// </summary>
    /// <value>The aspect ratio expressed as width/height.</value>
    [PublicAPI]
    public float WidthToHeightRatio => Value;

    private PivAspectRatio(float value) => Value = value;

    /// <summary>
    /// Creates a PIV aspect ratio with validation.
    /// </summary>
    /// <param name="value">
    /// The aspect ratio value (width/height). Must be positive and between 0.5 and 2.0.
    /// Standard PIV value is 0.75.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the <see cref="PivAspectRatio"/> if valid,
    /// otherwise a failure result with an error message.
    /// </returns>
    /// <example>
    /// <code>
    /// // Create standard PIV aspect ratio
    /// var standardRatio = PivAspectRatio.Create(0.75f);
    ///
    /// // Or use the predefined standard
    /// var pivRatio = PivAspectRatio.Standard;
    ///
    /// Console.WriteLine($"PIV ratio: {pivRatio.Value} ({pivRatio.Value * 4}:{4})");
    /// </code>
    /// </example>
    [PublicAPI]
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

    /// <summary>
    /// Gets the standard PIV aspect ratio of 3:4 (0.75).
    /// </summary>
    /// <value>
    /// A <see cref="PivAspectRatio"/> with value 0.75, representing the
    /// FIPS 201-3 standard aspect ratio for PIV facial images (420x560 pixels).
    /// </value>
    /// <remarks>
    /// This is the required aspect ratio for all PIV-compliant facial images.
    /// The 3:4 ratio ensures proper facial coverage and framing as specified
    /// in government identity verification standards.
    /// </remarks>
    [PublicAPI]
    public static PivAspectRatio Standard => new(0.75f); // 3:4 ratio
}
