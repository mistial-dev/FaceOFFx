namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Represents a scale factor for image transformation
/// </summary>
public sealed record ScaleFactor
{
    /// <summary>
    /// Gets the scale factor value (1.0 = no scaling)
    /// </summary>
    public float Value { get; }

    private ScaleFactor(float value) => Value = value;

    /// <summary>
    /// Creates a scale factor
    /// </summary>
    /// <param name="value">Scale factor value (must be positive)</param>
    /// <returns>Success with ScaleFactor if valid, otherwise failure</returns>
    /// <remarks>
    /// Values greater than 1.0 represent upscaling, less than 1.0 represent downscaling
    /// </remarks>
    public static Result<ScaleFactor> Create(float value)
    {
        if (value <= 0 || float.IsNaN(value) || float.IsInfinity(value))
        {
            return Result.Failure<ScaleFactor>("Scale factor must be a positive number");
        }

        return Result.Success(new ScaleFactor(value));
    }

    /// <summary>
    /// No scaling (1:1)
    /// </summary>
    public static ScaleFactor Identity => new(1f);

    /// <summary>
    /// Whether this scale factor increases image size
    /// </summary>
    public bool IsUpscaling => Value > 1.0f;

    /// <summary>
    /// Whether this scale factor decreases image size
    /// </summary>
    public bool IsDownscaling => Value < 1.0f;

    /// <summary>
    /// Whether this represents actual scaling
    /// </summary>
    public bool IsSignificant => Math.Abs(Value - 1f) > 0.01f;

    /// <summary>
    /// Calculates the resulting dimensions after scaling
    /// </summary>
    public ImageDimensions Apply(ImageDimensions source)
    {
        var newWidth = (int)Math.Round(source.Width * Value);
        var newHeight = (int)Math.Round(source.Height * Value);
        return new ImageDimensions(newWidth, newHeight);
    }

    /// <summary>
    /// Combines two scale factors
    /// </summary>
    public ScaleFactor Combine(ScaleFactor other) => new(Value * other.Value);

    /// <summary>
    /// Gets the inverse scale factor
    /// </summary>
    public ScaleFactor Inverse => new(1f / Value);
}
