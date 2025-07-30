using FaceOFFx.Core.Domain.Common;
using AwesomeAssertions;
using NUnit.Framework;

namespace FaceOFFx.Core.Tests.Domain.Common;

/// <summary>
/// Tests for the Confidence value object
/// </summary>
[TestFixture]
public class ConfidenceTests
{
    /// <summary>
    /// Test that valid confidence values can be created successfully
    /// </summary>
    [TestCase(0.0f)]
    [TestCase(0.5f)]
    [TestCase(1.0f)]
    public void Create_WithValidValue_ShouldSucceed(float value)
    {
        var result = Confidence.Create(value);
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    /// <summary>
    /// Test that invalid confidence values fail validation
    /// </summary>
    [TestCase(-0.1f)]
    [TestCase(1.1f)]
    public void Create_WithInvalidValue_ShouldFail(float value)
    {
        var result = Confidence.Create(value);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("between 0 and 1");
    }

    /// <summary>
    /// Test that non-finite values (NaN, Infinity) fail validation
    /// </summary>
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Create_WithNonFiniteValue_ShouldFail(float value)
    {
        var result = Confidence.Create(value);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("must be a valid number");
    }

    /// <summary>
    /// Test implicit conversion from Confidence to float
    /// </summary>
    [Test]
    public void ImplicitConversion_ToFloat_ShouldWork()
    {
        var confidence = Confidence.Create(0.75f).Value;

        float value = confidence;
        value.Should().Be(0.75f);
    }

    /// <summary>
    /// Test that ToString formats confidence as percentage
    /// </summary>
    [Test]
    public void ToString_ShouldFormatAsPercentage()
    {
        var confidence = Confidence.Create(0.856f).Value;

        var result = confidence.ToString();
        result.Should().Contain("85.6");
    }
}
