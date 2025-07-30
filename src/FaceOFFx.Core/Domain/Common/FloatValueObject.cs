using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Common;

/// <summary>
/// Abstract base class for value objects that encapsulate a single float value with validation rules.
/// </summary>
/// <remarks>
/// This class provides a foundation for creating strongly-typed numeric value objects that enforce
/// business rules and constraints. It implements the Value Object pattern from Domain-Driven Design,
/// ensuring immutability and value equality semantics through the use of C# records.
/// </remarks>
/// <example>
/// <code>
/// public record Temperature : FloatValueObject
/// {
///     private Temperature(float value) : base(value) { }
///
///     public static Result&lt;Temperature&gt; Create(float value)
///     {
///         return Create(value,
///             v => ValidateRange(v, -273.15f, 1000f, "Temperature"),
///             v => new Temperature(v));
///     }
/// }
/// </code>
/// </example>
[PublicAPI]
public abstract record FloatValueObject
{
    /// <summary>
    /// Gets the encapsulated float value.
    /// </summary>
    /// <value>The underlying float value that this object represents.</value>
    public float Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FloatValueObject"/> class.
    /// </summary>
    /// <param name="value">The float value to encapsulate.</param>
    /// <remarks>
    /// This constructor is protected to enforce the use of factory methods in derived classes,
    /// ensuring proper validation before object creation.
    /// </remarks>
    protected FloatValueObject(float value) => Value = value;

    /// <summary>
    /// Factory method for creating validated instances of float value objects.
    /// </summary>
    /// <typeparam name="T">The type of value object to create, must derive from <see cref="FloatValueObject"/>.</typeparam>
    /// <param name="value">The float value to validate and wrap.</param>
    /// <param name="validate">A validation function that returns a <see cref="Result"/> indicating success or failure.</param>
    /// <param name="factory">A factory function that creates the value object instance when validation succeeds.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the created value object if validation succeeds,
    /// or a failure result with an error message if validation fails.
    /// </returns>
    /// <remarks>
    /// This method implements the template method pattern, allowing derived classes to specify
    /// their validation rules while reusing the creation logic.
    /// </remarks>
    protected static Result<T> Create<T>(
        float value,
        Func<float, Result> validate,
        Func<float, T> factory
    )
        where T : FloatValueObject
    {
        var validation = validate(value);
        return validation.IsSuccess
            ? Result.Success(factory(value))
            : Result.Failure<T>(validation.Error);
    }

    /// <summary>
    /// Validates that a value is non-negative (greater than or equal to zero).
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="valueName">The name of the value for error reporting.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success if the value is non-negative,
    /// or failure with a descriptive error message.
    /// </returns>
    /// <remarks>
    /// Use this validation for values that can be zero but not negative, such as distances,
    /// counts, or elapsed time.
    /// </remarks>
    protected static Result ValidateNonNegative(float value, string valueName)
    {
        return value >= 0
            ? Result.Success()
            : Result.Failure($"{valueName} must be non-negative, was {value}");
    }

    /// <summary>
    /// Validates that a value is strictly positive (greater than zero).
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="valueName">The name of the value for error reporting.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success if the value is positive,
    /// or failure with a descriptive error message.
    /// </returns>
    /// <remarks>
    /// Use this validation for values that must be greater than zero, such as ratios,
    /// scale factors, or divisors.
    /// </remarks>
    protected static Result ValidatePositive(float value, string valueName)
    {
        return value > 0
            ? Result.Success()
            : Result.Failure($"{valueName} must be positive, was {value}");
    }

    /// <summary>
    /// Validates that a value falls within a specified inclusive range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="valueName">The name of the value for error reporting.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success if the value is within the range,
    /// or failure with a descriptive error message.
    /// </returns>
    /// <remarks>
    /// Both the minimum and maximum values are inclusive. This validation is useful for
    /// values with natural bounds, such as percentages, angles, or normalized coordinates.
    /// </remarks>
    protected static Result ValidateRange(float value, float min, float max, string valueName)
    {
        return value >= min && value <= max
            ? Result.Success()
            : Result.Failure($"{valueName} must be between {min} and {max}, was {value}");
    }

    /// <summary>
    /// Provides implicit conversion from <see cref="FloatValueObject"/> to <see cref="float"/>.
    /// </summary>
    /// <param name="value">The value object to convert.</param>
    /// <returns>The underlying float value.</returns>
    /// <remarks>
    /// This operator allows value objects to be used seamlessly in mathematical operations
    /// and APIs that expect float values, improving usability while maintaining type safety
    /// during object creation.
    /// </remarks>
    public static implicit operator float(FloatValueObject value) => value.Value;

    /// <summary>
    /// Returns a string representation of the value object.
    /// </summary>
    /// <returns>The float value formatted with 2 decimal places.</returns>
    public override string ToString() => Value.ToString("F2");
}

/// <summary>
/// Abstract base class for value objects representing percentage values stored as ratios (0.0 to 1.0).
/// </summary>
/// <remarks>
/// This class extends <see cref="FloatValueObject"/> to provide specialized behavior for percentage values.
/// Values are stored internally as ratios (0.0 to 1.0) but can be accessed as percentages (0 to 100)
/// through the <see cref="Percentage"/> property. This approach maintains precision while providing
/// intuitive access patterns.
/// </remarks>
/// <example>
/// <code>
/// public record SuccessRate : PercentageValue
/// {
///     private SuccessRate(float value) : base(value) { }
///
///     public static Result&lt;SuccessRate&gt; Create(float value)
///     {
///         return Create(value,
///             v => ValidatePercentage(v, "Success rate"),
///             v => new SuccessRate(v));
///     }
/// }
///
/// // Usage
/// var rate = SuccessRate.Create(0.85f).Value;
/// Console.WriteLine($"Success: {rate}"); // Output: "Success: 85.0%"
/// Console.WriteLine($"Ratio: {rate.Value}"); // Output: "Ratio: 0.85"
/// </code>
/// </example>
[PublicAPI]
public abstract record PercentageValue : FloatValueObject
{
    /// <summary>
    /// Gets the value as a percentage (0-100).
    /// </summary>
    /// <value>
    /// The percentage representation of the internal ratio value.
    /// For example, an internal value of 0.75 returns 75.0.
    /// </value>
    public float Percentage => Value * 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="PercentageValue"/> class.
    /// </summary>
    /// <param name="value">The percentage value as a ratio between 0.0 and 1.0.</param>
    protected PercentageValue(float value)
        : base(value) { }

    /// <summary>
    /// Validates that a value is a valid percentage ratio (0.0 to 1.0 inclusive).
    /// </summary>
    /// <param name="value">The value to validate as a ratio.</param>
    /// <param name="valueName">The name of the value for error reporting.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success if the value is between 0.0 and 1.0,
    /// or failure with a descriptive error message.
    /// </returns>
    /// <remarks>
    /// This method expects the value to be passed as a ratio (0.0 to 1.0), not as a
    /// percentage (0 to 100). For example, 75% should be passed as 0.75.
    /// </remarks>
    protected static Result ValidatePercentage(float value, string valueName)
    {
        return ValidateRange(value, 0f, 1f, valueName);
    }

    /// <summary>
    /// Returns a string representation of the percentage value.
    /// </summary>
    /// <returns>The value formatted as a percentage with one decimal place (e.g., "85.5%").</returns>
    public override string ToString() => $"{Percentage:F1}%";
}

/// <summary>
/// Abstract base class for value objects representing ratio values.
/// </summary>
/// <remarks>
/// This class extends <see cref="FloatValueObject"/> to provide specialized behavior for ratio values
/// that must be positive. Ratios are commonly used to represent relationships between quantities,
/// such as aspect ratios, scale factors, or proportions.
/// </remarks>
/// <example>
/// <code>
/// public record AspectRatio : RatioValue
/// {
///     private AspectRatio(float value) : base(value) { }
///
///     public static Result&lt;AspectRatio&gt; Create(float width, float height)
///     {
///         var ratio = width / height;
///         return Create(ratio,
///             v => ValidateRatio(v, "Aspect ratio"),
///             v => new AspectRatio(v));
///     }
/// }
///
/// // Usage
/// var ratio = AspectRatio.Create(16, 9).Value;
/// Console.WriteLine(ratio); // Output: "1.78:1"
/// </code>
/// </example>
[PublicAPI]
public abstract record RatioValue : FloatValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RatioValue"/> class.
    /// </summary>
    /// <param name="value">The ratio value, which must be positive.</param>
    protected RatioValue(float value)
        : base(value) { }

    /// <summary>
    /// Validates that a value is a valid ratio (strictly positive).
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="valueName">The name of the value for error reporting.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success if the value is positive,
    /// or failure with a descriptive error message.
    /// </returns>
    /// <remarks>
    /// Ratios must be positive values greater than zero. Zero and negative values
    /// are not valid for ratio representations.
    /// </remarks>
    protected static Result ValidateRatio(float value, string valueName)
    {
        return ValidatePositive(value, valueName);
    }

    /// <summary>
    /// Returns a string representation of the ratio value.
    /// </summary>
    /// <returns>The ratio formatted in "X:1" notation with 2 decimal places (e.g., "1.78:1").</returns>
    public override string ToString() => $"{Value:F2}:1";
}
