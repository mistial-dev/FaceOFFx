namespace FaceOFFx.Core.Domain.Common;

/// <summary>
/// Represents an immutable range of values with an optional target value within that range.
/// </summary>
/// <typeparam name="T">The type of values in the range. Must implement <see cref="IComparable{T}"/>.</typeparam>
/// <remarks>
/// This generic value object encapsulates a minimum and maximum value, with an optional target value
/// that must fall within the range. It provides methods for range validation, containment checking,
/// and deviation calculations. The class is sealed to ensure immutability and prevent inheritance.
/// </remarks>
/// <example>
/// <code>
/// // Create a simple range
/// var ageRange = Range&lt;int&gt;.Create(18, 65).Value;
/// 
/// // Create a range with target
/// var tempRange = Range&lt;float&gt;.Create(20.0f, 25.0f, 22.5f).Value;
/// 
/// // Check containment
/// bool isValid = ageRange.Contains(25); // true
/// 
/// // Calculate deviation
/// float deviation = tempRange.CalculateDeviation(26.0f, x => x);
/// 
/// // Use extension methods for float ranges
/// var floatRange = (0.0f, 1.0f).ToRange();
/// </code>
/// </example>
public sealed record Range<T> where T : IComparable<T>
{
    /// <summary>
    /// Gets the minimum value of the range (inclusive).
    /// </summary>
    /// <value>The lower bound of the range.</value>
    public T Min { get; }
    
    /// <summary>
    /// Gets the maximum value of the range (inclusive).
    /// </summary>
    /// <value>The upper bound of the range.</value>
    public T Max { get; }
    
    /// <summary>
    /// Gets the optional target value within the range.
    /// </summary>
    /// <value>
    /// A <see cref="Maybe{T}"/> containing the target value if specified,
    /// or <see cref="Maybe{T}.None"/> if no target was provided.
    /// </value>
    /// <remarks>
    /// The target value represents an ideal or preferred value within the range.
    /// When present, it must satisfy: Min ≤ Target ≤ Max.
    /// </remarks>
    public Maybe<T> Target { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Range{T}"/> class.
    /// </summary>
    /// <param name="min">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    /// <param name="target">The optional target value within the range.</param>
    private Range(T min, T max, Maybe<T> target)
    {
        Min = min;
        Max = max;
        Target = target;
    }
    
    /// <summary>
    /// Creates a new range with specified minimum and maximum values.
    /// </summary>
    /// <param name="min">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the created range if valid,
    /// or a failure result if min is greater than max.
    /// </returns>
    /// <remarks>
    /// Both min and max values are inclusive. The method validates that min ≤ max
    /// before creating the range.
    /// </remarks>
    public static Result<Range<T>> Create(T min, T max)
    {
        if (min.CompareTo(max) > 0)
        {
            return Result.Failure<Range<T>>($"Min value {min} cannot be greater than max value {max}");
        }

        return Result.Success(new Range<T>(min, max, Maybe<T>.None));
    }
    
    /// <summary>
    /// Creates a new range with specified minimum, maximum, and target values.
    /// </summary>
    /// <param name="min">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    /// <param name="target">The target value, which must be within the range.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the created range if all values are valid,
    /// or a failure result if validation fails.
    /// </returns>
    /// <remarks>
    /// This method validates that:
    /// - min ≤ max
    /// - min ≤ target ≤ max
    /// </remarks>
    public static Result<Range<T>> Create(T min, T max, T target)
    {
        if (min.CompareTo(max) > 0)
        {
            return Result.Failure<Range<T>>($"Min value {min} cannot be greater than max value {max}");
        }

        if (target.CompareTo(min) < 0 || target.CompareTo(max) > 0)
        {
            return Result.Failure<Range<T>>($"Target value {target} must be within range [{min}, {max}]");
        }

        return Result.Success(new Range<T>(min, max, Maybe<T>.From(target)));
    }
    
    /// <summary>
    /// Determines whether a value falls within this range (inclusive).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <c>true</c> if the value is greater than or equal to <see cref="Min"/>
    /// and less than or equal to <see cref="Max"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var range = Range&lt;int&gt;.Create(1, 10).Value;
    /// Console.WriteLine(range.Contains(5));  // true
    /// Console.WriteLine(range.Contains(15)); // false
    /// Console.WriteLine(range.Contains(1));  // true (inclusive)
    /// </code>
    /// </example>
    public bool Contains(T value) => 
        value.CompareTo(Min) >= 0 && value.CompareTo(Max) <= 0;
    
    /// <summary>
    /// Gets the target value if specified, or calculates a midpoint using the provided function.
    /// </summary>
    /// <param name="midpointCalculator">A function that calculates the midpoint between min and max.</param>
    /// <returns>
    /// The target value if specified, otherwise the result of the midpoint calculation.
    /// </returns>
    /// <remarks>
    /// This method is useful when you need a representative value from the range.
    /// The midpoint calculator allows for type-specific implementations of "middle" value.
    /// </remarks>
    /// <example>
    /// <code>
    /// var range = Range&lt;float&gt;.Create(0.0f, 10.0f).Value;
    /// float middle = range.GetTargetOrMidpoint((min, max) => (min + max) / 2f); // 5.0f
    /// </code>
    /// </example>
    public T GetTargetOrMidpoint(Func<T, T, T> midpointCalculator)
    {
        return Target.GetValueOrDefault(() => midpointCalculator(Min, Max));
    }
    
    /// <summary>
    /// Calculates the normalized deviation of a value from this range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="toFloat">A function to convert the generic type to float for calculations.</param>
    /// <returns>
    /// A normalized deviation where:
    /// - 0.0 indicates the value is within the range
    /// - Positive values indicate how far outside the range (as a ratio) the value is
    /// </returns>
    /// <remarks>
    /// The deviation is normalized by dividing the distance from the range by the boundary value.
    /// This provides a scale-independent measure of how far outside the range a value falls.
    /// A small epsilon (0.001) is used to prevent division by zero.
    /// </remarks>
    /// <example>
    /// <code>
    /// var range = Range&lt;float&gt;.Create(10.0f, 20.0f).Value;
    /// float dev1 = range.CalculateDeviation(15.0f, x => x);  // 0.0 (within range)
    /// float dev2 = range.CalculateDeviation(25.0f, x => x);  // 0.25 (25% beyond max)
    /// float dev3 = range.CalculateDeviation(5.0f, x => x);   // 0.5 (50% below min)
    /// </code>
    /// </example>
    public float CalculateDeviation(T value, Func<T, float> toFloat)
    {
        if (Contains(value))
        {
            return 0f;
        }

        var floatValue = toFloat(value);
        var floatMin = toFloat(Min);
        var floatMax = toFloat(Max);
        
        if (floatValue < floatMin)
        {
            return (floatMin - floatValue) / Math.Max(floatMin, 0.001f);
        }
        else
        {
            return (floatValue - floatMax) / Math.Max(floatMax, 0.001f);
        }
    }
    
    /// <summary>
    /// Restricts a value to be within the bounds of this range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>
    /// The input value if it's within the range; otherwise, the nearest range boundary
    /// (Min if value is too low, Max if value is too high).
    /// </returns>
    /// <example>
    /// <code>
    /// var range = Range&lt;int&gt;.Create(0, 100).Value;
    /// Console.WriteLine(range.Clamp(-10));  // 0
    /// Console.WriteLine(range.Clamp(50));   // 50
    /// Console.WriteLine(range.Clamp(150));  // 100
    /// </code>
    /// </example>
    public T Clamp(T value)
    {
        if (value.CompareTo(Min) < 0)
        {
            return Min;
        }
        if (value.CompareTo(Max) > 0)
        {
            return Max;
        }
        return value;
    }
    
    /// <summary>
    /// Returns a string representation of the range.
    /// </summary>
    /// <returns>
    /// A string in the format "[min, max]" or "[min, max] (target: value)" if a target is specified.
    /// </returns>
    public override string ToString() => 
        Target.HasValue 
            ? $"[{Min}, {Max}] (target: {Target.Value})"
            : $"[{Min}, {Max}]";
}

/// <summary>
/// Provides extension methods for convenient creation and manipulation of <see cref="Range{T}"/> instances.
/// </summary>
/// <remarks>
/// These extensions are particularly useful for float ranges, providing syntactic sugar
/// for common operations and tuple-based initialization.
/// </remarks>
public static class RangeExtensions
{
    /// <summary>
    /// Creates a <see cref="Range{T}"/> of float from a tuple of min and max values.
    /// </summary>
    /// <param name="tuple">A tuple containing the minimum and maximum values.</param>
    /// <returns>A new float range with the specified bounds.</returns>
    /// <exception cref="ArgumentException">Thrown if min is greater than max.</exception>
    /// <example>
    /// <code>
    /// var range = (0.0f, 1.0f).ToRange();
    /// var tempRange = (18.5f, 25.0f).ToRange();
    /// </code>
    /// </example>
    public static Range<float> ToRange(this (float min, float max) tuple)
    {
        var result = Range<float>.Create(tuple.min, tuple.max);
        return result.IsSuccess ? result.Value : throw new ArgumentException(result.Error);
    }
    
    /// <summary>
    /// Creates a <see cref="Range{T}"/> of float from a tuple of min, max, and target values.
    /// </summary>
    /// <param name="tuple">A tuple containing the minimum, maximum, and target values.</param>
    /// <returns>A new float range with the specified bounds and target.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if min is greater than max, or if target is outside the range.
    /// </exception>
    /// <example>
    /// <code>
    /// var range = (0.0f, 10.0f, 7.5f).ToRange();
    /// var percentRange = (0.0f, 1.0f, 0.85f).ToRange();
    /// </code>
    /// </example>
    public static Range<float> ToRange(this (float min, float max, float target) tuple)
    {
        var result = Range<float>.Create(tuple.min, tuple.max, tuple.target);
        return result.IsSuccess ? result.Value : throw new ArgumentException(result.Error);
    }
    
    /// <summary>
    /// Calculates the midpoint of a float range.
    /// </summary>
    /// <param name="range">The range to calculate the midpoint for.</param>
    /// <returns>
    /// The target value if specified, otherwise the arithmetic mean of min and max.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that provides a default midpoint calculation
    /// for float ranges using simple averaging.
    /// </remarks>
    /// <example>
    /// <code>
    /// var range1 = (0.0f, 10.0f).ToRange();
    /// float mid1 = range1.GetMidpoint(); // 5.0f
    /// 
    /// var range2 = (0.0f, 10.0f, 7.0f).ToRange();
    /// float mid2 = range2.GetMidpoint(); // 7.0f (returns target)
    /// </code>
    /// </example>
    public static float GetMidpoint(this Range<float> range)
    {
        return range.GetTargetOrMidpoint((min, max) => (min + max) / 2f);
    }
}