using FaceOFFx.Core.Domain.Common;
using FaceOFFx.Core.Domain.Detection;
using AwesomeAssertions;
using NUnit.Framework;

namespace FaceOFFx.Core.Tests.Domain.Detection;

/// <summary>
/// Contains unit tests for the <see cref="FaceBox"/> class, verifying its functionality
/// and correctness in various scenarios.
/// </summary>
/// <remarks>
/// This test class includes various test cases to validate the creation, property calculations,
/// containment checks, scaling, area calculations, and other operations of <see cref="FaceBox"/>.
/// Additionally, boundary and edge cases are tested to ensure robustness.
/// </remarks>
[TestFixture]
public class FaceBoxTests
{
    /// <summary>
    /// Verifies that the <see cref="FaceBox.Create(float, float, float, float)"/> method
    /// successfully creates a valid <see cref="FaceBox"/> instance when provided with
    /// positive dimensions for width and height.
    /// </summary>
    /// <remarks>
    /// This test checks that the resulting <see cref="FaceBox"/> object has the expected
    /// values for its X, Y, Width, and Height properties, and that the operation
    /// is indicated as successful.
    /// </remarks>
    [Test]
    public void Create_WithValidDimensions_ShouldReturnSuccess()
    {
        var result = FaceBox.Create(10, 20, 100, 200);

        result.IsSuccess.Should().BeTrue();
        result.Value.X.Should().Be(10);
        result.Value.Y.Should().Be(20);
        result.Value.Width.Should().Be(100);
        result.Value.Height.Should().Be(200);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public void Create_WithNonPositiveWidth_ShouldReturnFailure(float width)
    {
        var result = FaceBox.Create(10, 20, width, 200);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Width must be positive");
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public void Create_WithNonPositiveHeight_ShouldReturnFailure(float height)
    {
        var result = FaceBox.Create(10, 20, 100, height);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Height must be positive");
    }

    [Test]
    public void Properties_ShouldCalculateCorrectly()
    {
        var box = FaceBox.Create(10, 20, 100, 200).Value;

        box.Left.Should().Be(10);
        box.Top.Should().Be(20);
        box.Right.Should().Be(110);
        box.Bottom.Should().Be(220);
        box.Area.Should().Be(20000);
    }

    [Test]
    public void Center_ShouldCalculateCorrectly()
    {
        var box = FaceBox.Create(10, 20, 100, 200).Value;

        box.Center.X.Should().Be(60);
        box.Center.Y.Should().Be(120);
    }

    [TestCase(60, 120, true)]
    [TestCase(10, 20, true)]
    [TestCase(110, 220, true)]
    [TestCase(9, 120, false)]
    [TestCase(111, 120, false)]
    [TestCase(60, 19, false)]
    [TestCase(60, 221, false)]
    public void Contains_ShouldDeterminePointContainmentCorrectly(float x, float y, bool expected)
    {
        var box = FaceBox.Create(10, 20, 100, 200).Value;
        var point = new Point2D(x, y);

        box.Contains(point).Should().Be(expected);
    }

    [Test]
    public void IntersectionOverUnion_WithIdenticalBoxes_ShouldReturn1()
    {
        var box1 = FaceBox.Create(10, 20, 100, 200).Value;
        var box2 = FaceBox.Create(10, 20, 100, 200).Value;

        box1.IntersectionOverUnion(box2).Should().Be(1.0f);
    }

    [Test]
    public void IntersectionOverUnion_WithNoOverlap_ShouldReturn0()
    {
        var box1 = FaceBox.Create(0, 0, 100, 100).Value;
        var box2 = FaceBox.Create(200, 200, 100, 100).Value;

        box1.IntersectionOverUnion(box2).Should().Be(0.0f);
    }

    [Test]
    public void IntersectionOverUnion_WithPartialOverlap_ShouldCalculateCorrectly()
    {
        var box1 = FaceBox.Create(0, 0, 100, 100).Value;
        var box2 = FaceBox.Create(50, 50, 100, 100).Value;

        var iou = box1.IntersectionOverUnion(box2);

        var intersectionArea = 50 * 50;
        var unionArea = (100 * 100) + (100 * 100) - intersectionArea;
        var expectedIou = (float)intersectionArea / unionArea;

        iou.Should().BeApproximately(expectedIou, 0.001f);
    }

    [TestCase(2.0f)]
    [TestCase(0.5f)]
    [TestCase(1.5f)]
    public void Scale_ShouldScaleAllDimensions(float factor)
    {
        var original = FaceBox.Create(10, 20, 100, 200).Value;

        var scaled = original.Scale(factor);

        scaled.X.Should().Be(10 * factor);
        scaled.Y.Should().Be(20 * factor);
        scaled.Width.Should().Be(100 * factor);
        scaled.Height.Should().Be(200 * factor);
    }

    [TestCase(10)]
    [TestCase(-5)]
    [TestCase(0)]
    public void Expand_ShouldExpandCorrectly(float pixels)
    {
        var original = FaceBox.Create(100, 100, 200, 200).Value;

        var expanded = original.Expand(pixels);

        expanded.X.Should().Be(100 - pixels);
        expanded.Y.Should().Be(100 - pixels);
        expanded.Width.Should().Be(200 + 2 * pixels);
        expanded.Height.Should().Be(200 + 2 * pixels);
    }

    [Test]
    public void Equality_ShouldWorkCorrectly()
    {
        var box1 = FaceBox.Create(10, 20, 100, 200).Value;
        var box2 = FaceBox.Create(10, 20, 100, 200).Value;
        var box3 = FaceBox.Create(10, 20, 100, 201).Value;

        box1.Should().Be(box2);
        box1.Should().NotBe(box3);
    }

    [Test]
    public void ToString_ShouldProvideUsefulRepresentation()
    {
        var box = FaceBox.Create(10, 20, 100, 200).Value;

        var str = box.ToString();

        str.Should().Contain("10");
        str.Should().Contain("20");
        str.Should().Contain("100");
        str.Should().Contain("200");
    }

    [TestCase(float.MinValue, 0, 100, 100)]
    [TestCase(float.MaxValue, 0, 100, 100)]
    [TestCase(0, float.MinValue, 100, 100)]
    [TestCase(0, float.MaxValue, 100, 100)]
    public void Create_WithExtremeCoordinates_ShouldHandleCorrectly(float x, float y, float width, float height)
    {
        var result = FaceBox.Create(x, y, width, height);

        result.IsSuccess.Should().BeTrue();
        result.Value.X.Should().Be(x);
        result.Value.Y.Should().Be(y);
    }

    [Test]
    public void IntersectionOverUnion_WithTouchingEdges_ShouldReturn0()
    {
        var box1 = FaceBox.Create(0, 0, 100, 100).Value;
        var box2 = FaceBox.Create(100, 0, 100, 100).Value;

        box1.IntersectionOverUnion(box2).Should().Be(0.0f);
    }

    [Test]
    public void Scale_WithNegativeFactor_ShouldInvertPositions()
    {
        var original = FaceBox.Create(10, 20, 100, 200).Value;

        var scaled = original.Scale(-1);

        scaled.X.Should().Be(-10);
        scaled.Y.Should().Be(-20);
        scaled.Width.Should().Be(-100);
        scaled.Height.Should().Be(-200);
    }
}
