namespace FaceOFFx.Core.Domain.Common;

/// <summary>
/// Represents a confidence value between 0.0 and 1.0, typically used for machine learning predictions
/// and probability scores.
/// </summary>
/// <remarks>
/// This is a value object that enforces confidence values to be within the valid range of 0.0 to 1.0 (inclusive).
/// The value represents a ratio, where 0.0 means no confidence and 1.0 means complete confidence.
/// NaN and infinity values are explicitly rejected during creation.
/// </remarks>
/// <example>
/// <code>
/// // Create a confidence value
/// var result = Confidence.Create(0.95f);
/// if (result.IsSuccess)
/// {
///     var confidence = result.Value;
///     Console.WriteLine($"Confidence: {confidence}"); // Output: "Confidence: 95.0%"
/// }
/// 
/// // Use predefined values
/// var noConfidence = Confidence.Zero;
/// var fullConfidence = Confidence.One;
/// </code>
/// </example>
public sealed record Confidence : PercentageValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Confidence"/> class.
    /// </summary>
    /// <param name="value">The confidence value as a ratio between 0.0 and 1.0.</param>
    private Confidence(float value) : base(value) { }

    /// <summary>
    /// Creates a new <see cref="Confidence"/> instance with the specified value.
    /// </summary>
    /// <param name="value">The confidence value as a ratio between 0.0 and 1.0.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the <see cref="Confidence"/> instance if the value is valid,
    /// or a failure result with an error message if the value is invalid.
    /// </returns>
    /// <remarks>
    /// This method validates that the value is:
    /// - Not NaN (Not a Number)
    /// - Not positive or negative infinity
    /// - Between 0.0 and 1.0 (inclusive)
    /// </remarks>
    public static Result<Confidence> Create(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return Result.Failure<Confidence>($"Confidence must be a valid number, but was {value}");
        }

        return Create(value, v => ValidatePercentage(v, "Confidence"), v => new Confidence(v));
    }

    /// <summary>
    /// Gets a <see cref="Confidence"/> instance representing zero confidence (0.0).
    /// </summary>
    /// <value>A confidence value of 0.0, indicating no confidence.</value>
    public static Confidence Zero => new(0.0f);
    
    /// <summary>
    /// Gets a <see cref="Confidence"/> instance representing complete confidence (1.0).
    /// </summary>
    /// <value>A confidence value of 1.0, indicating full confidence.</value>
    public static Confidence One => new(1.0f);
}