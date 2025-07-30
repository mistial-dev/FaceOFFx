using AwesomeAssertions;
using FaceOFFx.Core.Domain.Common;
using NUnit.Framework;

namespace FaceOFFx.Core.Tests.Domain.Common;

/// <summary>
/// Contains unit tests for the Range class, validating its behavior and correctness across various scenarios.
/// </summary>
/// <remarks>
/// This class includes test cases to ensure that the creation, validation, and usage of ranges function as expected.
/// The tests include scenarios with numeric ranges, target values, boundary conditions, and utility methods.
/// </remarks>
[TestFixture]
public class RangeTests
{
    /// <summary>
    /// Tests the creation of a valid range using the <see cref="Range{T}.Create(T, T)"/> method.
    /// Ensures that the method returns a successful result, correctly initializes the minimum and maximum values,
    /// and leaves the optional target value unset.
    /// </summary>
    [Test]
    public void Create_WithValidMinMax_ShouldReturnSuccess()
    {
        var result = Range<int>.Create(10, 20);

        result.IsSuccess.Should().BeTrue();
        result.Value.Min.Should().Be(10);
        result.Value.Max.Should().Be(20);
        result.Value.Target.HasValue.Should().BeFalse();
    }

    /// <summary>
    /// Tests the creation of a range where the minimum value is greater than the maximum value,
    /// ensuring that the operation fails as expected.
    /// </summary>
    /// <remarks>
    /// This test verifies that attempting to create a range with an invalid configuration
    /// (minimum value greater than the maximum) results in a failure. It also confirms that the
    /// correct error message is returned.
    /// </remarks>
    /// <test>
    /// Given a minimum value of 20 and a maximum value of 10:
    /// - The result should indicate failure.
    /// - The error message should state that the minimum value cannot exceed the maximum value.
    /// </test>
    [Test]
    public void Create_WithMinGreaterThanMax_ShouldReturnFailure()
    {
        var result = Range<int>.Create(20, 10);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Min value 20 cannot be greater than max value 10");
    }

    /// <summary>
    /// Tests the creation of a range with equal minimum and maximum values.
    /// Ensures that the result is successful and the created range has the same value for both Min and Max.
    /// </summary>
    [Test]
    public void Create_WithEqualMinMax_ShouldReturnSuccess()
    {
        var result = Range<int>.Create(15, 15);

        result.IsSuccess.Should().BeTrue();
        result.Value.Min.Should().Be(15);
        result.Value.Max.Should().Be(15);
    }

    /// <summary>
    /// Validates the behavior of creating a <see cref="Range{T}"/> object with valid minimum, maximum, and target values.
    /// </summary>
    /// <remarks>
    /// Confirms that given a valid minimum, maximum, and target value, the <see cref="Range{T}"/> object is successfully created.
    /// Ensures that:
    /// - The result signifies success.
    /// - The minimum value is set correctly.
    /// - The maximum value is set correctly.
    /// - The target value exists and matches the specified input.
    /// </remarks>
    /// <exception cref="AssertionException">
    /// Thrown if the result is not successful, or if the properties of the created <see cref="Range{T}"/> object do not match the expected values.
    /// </exception>
    [Test]
    public void CreateWithTarget_WithValidValues_ShouldReturnSuccess()
    {
        var result = Range<float>.Create(0.0f, 10.0f, 7.5f);

        result.IsSuccess.Should().BeTrue();
        result.Value.Min.Should().Be(0.0f);
        result.Value.Max.Should().Be(10.0f);
        result.Value.Target.HasValue.Should().BeTrue();
        result.Value.Target.Value.Should().Be(7.5f);
    }

    /// <summary>
    /// Tests the creation of a range with a target value that is below the minimum boundary.
    /// Validates that the operation fails and provides an appropriate error message.
    /// </summary>
    /// <remarks>
    /// The purpose of this test is to ensure that the range creation logic correctly identifies
    /// and blocks invalid target values, specifically when the target value is less than the
    /// specified minimum value of the range.
    /// </remarks>
    /// <example>
    /// Expected outcome: The operation returns a failure result with an error message
    /// indicating that the target value must be within the specified range.
    /// </example>
    [Test]
    public void CreateWithTarget_WithTargetBelowMin_ShouldReturnFailure()
    {
        var result = Range<int>.Create(10, 20, 5);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Target value 5 must be within range [10, 20]");
    }

    /// <summary>
    /// Tests the behavior when creating a range with a specified target value that is above the maximum value of the range.
    /// </summary>
    /// <remarks>
    /// This test verifies that the method returns a failure result when the target value exceeds the maximum boundary of the range.
    /// An appropriate error message is expected to be included in the failure.
    /// </remarks>
    [Test]
    public void CreateWithTarget_WithTargetAboveMax_ShouldReturnFailure()
    {
        var result = Range<int>.Create(10, 20, 25);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Target value 25 must be within range [10, 20]");
    }

    /// <summary>
    /// Validates whether the specified value falls within the range boundaries.
    /// </summary>
    /// <param name="value">The value to check against the range.</param>
    /// <param name="expected">The expected result indicating whether the value should be within the range.</param>
    [TestCase(15, true)]
    [TestCase(10, true)]
    [TestCase(20, true)]
    [TestCase(9, false)]
    [TestCase(21, false)]
    public void Contains_ShouldDetermineCorrectly(int value, bool expected)
    {
        var range = Range<int>.Create(10, 20).Value;

        range.Contains(value).Should().Be(expected);
    }

    /// <summary>
    /// Validates that the GetTargetOrMidpoint method returns the target value when a target is present within the range.
    /// </summary>
    /// <remarks>
    /// This method tests the behavior of the GetTargetOrMidpoint function in the presence of a target.
    /// Specifically, it ensures that the function does not calculate the midpoint and, instead, directly returns the defined target.
    /// </remarks>
    /// <example>
    /// The range is created with a defined target. The method is expected to yield the target value as the result.
    /// </example>
    [Test]
    public void GetTargetOrMidpoint_WithTarget_ShouldReturnTarget()
    {
        var range = Range<float>.Create(0.0f, 10.0f, 7.0f).Value;

        var result = range.GetTargetOrMidpoint((min, max) => (min + max) / 2f);

        result.Should().Be(7.0f);
    }

    /// <summary>
    /// Ensures the correct functionality of the <see cref="Range{T}.GetTargetOrMidpoint"/> method
    /// when no target value is provided. This test verifies that the method calculates
    /// the midpoint of the range using the given midpoint calculation logic.
    /// </summary>
    [Test]
    public void GetTargetOrMidpoint_WithoutTarget_ShouldCalculateMidpoint()
    {
        var range = Range<float>.Create(0.0f, 10.0f).Value;

        var result = range.GetTargetOrMidpoint((min, max) => (min + max) / 2f);

        result.Should().Be(5.0f);
    }

    /// <summary>
    /// Tests the CalculateDeviation method to ensure it computes deviations correctly against the given range.
    /// </summary>
    /// <param name="value">The value for which the deviation is to be calculated.</param>
    /// <param name="expectedDeviation">The expected deviation result for the given value.</param>
    [TestCase(15.0f, 0.0f)]
    [TestCase(10.0f, 0.0f)]
    [TestCase(20.0f, 0.0f)]
    [TestCase(25.0f, 0.25f)]
    [TestCase(5.0f, 0.5f)]
    [TestCase(30.0f, 0.5f)]
    [TestCase(0.0f, 1.0f)]
    public void CalculateDeviation_ShouldCalculateCorrectly(float value, float expectedDeviation)
    {
        var range = Range<float>.Create(10.0f, 20.0f).Value;

        var deviation = range.CalculateDeviation(value, x => x);

        deviation.Should().BeApproximately(expectedDeviation, 0.001f);
    }

    /// <summary>
    /// Validates that the <c>CalculateDeviation</c> method can properly handle very small boundary values,
    /// particularly values close to zero or epsilon, and ensures no unexpected behavior occurs in such cases.
    /// </summary>
    /// <remarks>
    /// This test is designed to confirm the robustness of the deviation calculation when the range boundaries
    /// are near zero or extremely small. It ensures the method accounts for precision and rounding issues in edge cases.
    /// </remarks>
    /// <exception cref="AssertionException">
    /// Thrown if the calculated deviation does not meet the expectations for values near the boundaries.
    /// </exception>
    /// <seealso cref="Range{T}.CalculateDeviation(T, Func{T, float})"/>
    [Test]
    public void CalculateDeviation_WithSmallBoundaryValues_ShouldHandleEpsilon()
    {
        var range = Range<float>.Create(0.0f, 0.0005f).Value;

        var deviation = range.CalculateDeviation(-0.001f, x => x);

        deviation.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Ensures the specified value is restricted within the bounds of a defined range.
    /// If the value is below the minimum, it returns the minimum; if above the maximum, it returns the maximum.
    /// </summary>
    /// <param name="value">The value to be clamped.</param>
    /// <param name="expected">The expected value after clamping to the range.</param>
    [TestCase(-10, 0)]
    [TestCase(50, 50)]
    [TestCase(150, 100)]
    [TestCase(0, 0)]
    [TestCase(100, 100)]
    public void Clamp_ShouldRestrictToRange(int value, int expected)
    {
        var range = Range<int>.Create(0, 100).Value;

        var clamped = range.Clamp(value);

        clamped.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that the string representation of a range, when no target is set,
    /// correctly formats the range using the format "[min, max]".
    /// </summary>
    /// <remarks>
    /// This test ensures that the default string representation of a range object
    /// accurately reflects its minimum and maximum values in a standard bracket notation.
    /// Expected result: a string formatted as "[min, max]".
    /// </remarks>
    [Test]
    public void ToString_WithoutTarget_ShouldFormatCorrectly()
    {
        var range = Range<int>.Create(10, 20).Value;

        var str = range.ToString();

        str.Should().Be("[10, 20]");
    }

    /// <summary>
    /// Verifies that the string representation of a <see cref="Range{T}"/> object
    /// includes the target value when a target is specified.
    /// </summary>
    /// <remarks>
    /// This test ensures that calling the <see cref="Range{T}.ToString"/> method
    /// with a range object that has a defined target value will include the target
    /// in the string output in the format "[min, max] (target: target)".
    /// </remarks>
    [Test]
    public void ToString_WithTarget_ShouldIncludeTarget()
    {
        var range = Range<float>.Create(0.0f, 1.0f, 0.75f).Value;

        var str = range.ToString();

        str.Should().Be("[0, 1] (target: 0.75)");
    }

    /// <summary>
    /// Tests the functionality of the <see cref="Range{T}"/> class with different
    /// types implementing <see cref="IComparable{T}"/>.
    /// </summary>
    /// <remarks>
    /// This method ensures that the Range class properly handles input of various
    /// comparable types, such as <see cref="DateTime"/>, including verifying
    /// successful creation of the range and correct behavior of range operations
    /// like <see cref="Range{T}.Contains"/>.
    /// </remarks>
    [Test]
    public void WorksWithDifferentComparableTypes()
    {
        var dateRange = Range<DateTime>.Create(
            new DateTime(2023, 1, 1),
            new DateTime(2023, 12, 31),
            new DateTime(2023, 6, 15)
        );

        dateRange.IsSuccess.Should().BeTrue();
        dateRange.Value.Contains(new DateTime(2023, 7, 1)).Should().BeTrue();
        dateRange.Value.Contains(new DateTime(2024, 1, 1)).Should().BeFalse();
    }

    /// <summary>
    /// Tests the functionality of creating a <c>Range</c> instance from a tuple of minimum and maximum values.
    /// </summary>
    /// <remarks>
    /// Validates that a tuple with a defined minimum and maximum is correctly converted to a <c>Range</c> object.
    /// Ensures the <c>Min</c> and <c>Max</c> properties of the resulting <c>Range</c> are properly assigned,
    /// and that the <c>Target</c> property is not set (i.e., remains <c>None</c>).
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the conversion does not produce a valid <c>Range</c> or if <c>Min</c> is greater than <c>Max</c>.
    /// </exception>
    [Test]
    public void RangeExtensions_ToRange_WithTuple_ShouldCreateRange()
    {
        var range = (0.0f, 10.0f).ToRange();

        range.Min.Should().Be(0.0f);
        range.Max.Should().Be(10.0f);
        range.Target.HasValue.Should().BeFalse();
    }

    /// <summary>
    /// Validates the creation of a <see cref="Range{T}"/> object from a tuple containing
    /// minimum, maximum, and target values using the <see cref="RangeExtensions.ToRange(ValueTuple{float, float, float})"/> extension method.
    /// Asserts that the resulting range is correctly constructed, and the target value is set and matches the input tuple target.
    /// </summary>
    /// <remarks>
    /// This test ensures that the extension method handles a valid tuple and creates a
    /// valid Range object where <c>Min</c>, <c>Max</c>, and <c>Target</c> properties
    /// are correctly populated.
    /// </remarks>
    [Test]
    public void RangeExtensions_ToRange_WithTargetTuple_ShouldCreateRange()
    {
        var range = (0.0f, 10.0f, 7.5f).ToRange();

        range.Min.Should().Be(0.0f);
        range.Max.Should().Be(10.0f);
        range.Target.HasValue.Should().BeTrue();
        range.Target.Value.Should().Be(7.5f);
    }

    /// <summary>
    /// Verifies that converting an invalid tuple to a range using the <c>ToRange</c> extension method
    /// correctly throws an <see cref="ArgumentException"/> due to an invalid range configuration.
    /// </summary>
    /// <remarks>
    /// This test ensures that when a tuple with the minimum value greater than the maximum value
    /// is passed to the <c>ToRange</c> method, an exception is thrown to indicate the invalid state.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the tuple represents an invalid range configuration,
    /// such as the minimum value being greater than the maximum value.
    /// </exception>
    [Test]
    public void RangeExtensions_ToRange_WithInvalidTuple_ShouldThrow()
    {
        var act = () => (10.0f, 0.0f).ToRange();

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Validates the behavior of the <c>GetMidpoint</c> extension method for ranges without a target value.
    /// Ensures that the midpoint is calculated correctly for a given range.
    /// </summary>
    /// <remarks>
    /// This method tests a scenario where no target value is provided in the range.
    /// The test will create a range using a tuple, calculate the midpoint, and assert the correctness of the result.
    /// </remarks>
    [Test]
    public void RangeExtensions_GetMidpoint_WithoutTarget_ShouldCalculate()
    {
        var range = (0.0f, 10.0f).ToRange();

        var midpoint = range.GetMidpoint();

        midpoint.Should().Be(5.0f);
    }

    /// <summary>
    /// Validates that the GetMidpoint method in the RangeExtensions class correctly returns the target value
    /// when a target is present in a range. This test ensures the functionality of the method when a valid
    /// target value exists within the specified range.
    /// </summary>
    [Test]
    public void RangeExtensions_GetMidpoint_WithTarget_ShouldReturnTarget()
    {
        var range = (0.0f, 10.0f, 8.0f).ToRange();

        var midpoint = range.GetMidpoint();

        midpoint.Should().Be(8.0f);
    }

    /// <summary>
    /// Validates that the equality methods for the <see cref="Range{T}"/> record work as expected.
    /// </summary>
    /// <remarks>
    /// This test ensures that two <see cref="Range{T}"/> objects with the same minimum and maximum bounds
    /// (and without a target value) are considered equal.
    /// It also verifies that differences in range bounds or the presence of a target value
    /// correctly affect equality comparisons.
    /// </remarks>
    [Test]
    public void Equality_ShouldWorkCorrectly()
    {
        var range1 = Range<int>.Create(10, 20).Value;
        var range2 = Range<int>.Create(10, 20).Value;
        var range3 = Range<int>.Create(10, 21).Value;
        var range4 = Range<int>.Create(10, 20, 15).Value;

        range1.Should().Be(range2);
        range1.Should().NotBe(range3);
        range1.Should().NotBe(range4);
    }
}
