using FaceOFFx.Core.Domain.Common;
using AwesomeAssertions;
using NUnit.Framework;

namespace FaceOFFx.Core.Tests.Domain.Common;

/// <summary>
/// Provides unit tests for the Point2D class, ensuring proper functionality
/// and correctness of core mathematical operations and properties.
/// </summary>
[TestFixture]
public class Point2DTests
{
    /// <summary>
    /// Verifies that the constructor of the <see cref="Point2D"/> class properly initializes the
    /// X and Y coordinates with the provided values.
    /// </summary>
    [Test]
    public void Constructor_ShouldInitializeCorrectly()
    {
        var point = new Point2D(10.5f, 20.3f);

        point.X.Should().Be(10.5f);
        point.Y.Should().Be(20.3f);
    }

    /// <summary>
    /// Validates that the static property <see cref="Point2D.Zero"/> correctly returns an instance of <see cref="Point2D"/>
    /// initialized at the origin with coordinates (0, 0).
    /// </summary>
    [Test]
    public void Zero_ShouldReturnOrigin()
    {
        var zero = Point2D.Zero;

        zero.X.Should().Be(0);
        zero.Y.Should().Be(0);
    }

    /// <summary>
    /// Validates that the <see cref="Point2D.DistanceTo(Point2D)"/> method calculates the correct distance between two points.
    /// </summary>
    /// <param name="x1">The X coordinate of the first point.</param>
    /// <param name="y1">The Y coordinate of the first point.</param>
    /// <param name="x2">The X coordinate of the second point.</param>
    /// <param name="y2">The Y coordinate of the second point.</param>
    /// <param name="expectedDistance">The expected distance between the two points.</param>
    [TestCase(0, 0, 3, 4, 5)]
    [TestCase(1, 1, 4, 5, 5)]
    [TestCase(10, 10, 10, 10, 0)]
    [TestCase(-5, -5, 5, 5, 14.142136f)]
    public void DistanceTo_ShouldCalculateCorrectly(float x1, float y1, float x2, float y2, float expectedDistance)
    {
        var point1 = new Point2D(x1, y1);
        var point2 = new Point2D(x2, y2);

        var distance = point1.DistanceTo(point2);

        distance.Should().BeApproximately(expectedDistance, 0.001f);
    }

    /// <summary>
    /// Verifies that the <see cref="Point2D.DistanceTo"/> method returns zero
    /// when calculating the distance from a point to itself.
    /// </summary>
    [Test]
    public void DistanceTo_WithSamePoint_ShouldReturnZero()
    {
        var point = new Point2D(42.5f, 73.2f);

        point.DistanceTo(point).Should().Be(0);
    }

    /// <summary>
    /// Verifies that the Add method correctly sums the X and Y coordinates of two Point2D instances
    /// and returns a new Point2D instance with the resulting coordinates.
    /// </summary>
    [Test]
    public void Add_ShouldReturnCorrectSum()
    {
        var point1 = new Point2D(10, 20);
        var point2 = new Point2D(5, 15);

        var result = point1.Add(point2);

        result.X.Should().Be(15);
        result.Y.Should().Be(35);
    }

    /// <summary>
    /// Validates that the Subtract method correctly calculates the difference
    /// between two <see cref="Point2D"/> instances.
    /// </summary>
    /// <remarks>
    /// This unit test ensures that when one <see cref="Point2D"/> instance
    /// is subtracted from another, the resulting <see cref="Point2D"/> has
    /// coordinates that are the arithmetic differences of the respective
    /// coordinates of the two points.
    /// </remarks>
    [Test]
    public void Subtract_ShouldReturnCorrectDifference()
    {
        var point1 = new Point2D(10, 20);
        var point2 = new Point2D(5, 15);

        var result = point1.Subtract(point2);

        result.X.Should().Be(5);
        result.Y.Should().Be(5);
    }

    /// <summary>
    /// Tests the <see cref="Point2D.Scale"/> method to verify that coordinates are scaled correctly
    /// based on the provided scaling factor.
    /// </summary>
    /// <param name="factor">The scaling factor to multiply the coordinates of the point.</param>
    [TestCase(2.0f)]
    [TestCase(0.5f)]
    [TestCase(-1.0f)]
    [TestCase(0.0f)]
    public void Scale_ShouldMultiplyCoordinates(float factor)
    {
        var point = new Point2D(10, 20);

        var result = point.Scale(factor);

        result.X.Should().Be(10 * factor);
        result.Y.Should().Be(20 * factor);
    }

    /// <summary>
    /// Verifies that the addition operator (+) for the <see cref="Point2D"/> type
    /// correctly calculates the sum of two <see cref="Point2D"/> instances.
    /// </summary>
    [Test]
    public void AddOperator_ShouldWorkCorrectly()
    {
        var point1 = new Point2D(10, 20);
        var point2 = new Point2D(5, 15);

        var result = point1 + point2;

        result.X.Should().Be(15);
        result.Y.Should().Be(35);
    }

    /// <summary>
    /// Validates the subtraction operator for the <see cref="Point2D"/> type.
    /// Ensures that the resulting point has the correct difference in both
    /// X and Y coordinate values when subtracting one point from another.
    /// </summary>
    [Test]
    public void SubtractOperator_ShouldWorkCorrectly()
    {
        var point1 = new Point2D(10, 20);
        var point2 = new Point2D(5, 15);

        var result = point1 - point2;

        result.X.Should().Be(5);
        result.Y.Should().Be(5);
    }

    /// <summary>
    /// Verifies that the multiplication operator (*) works correctly
    /// when a <see cref="Point2D"/> object appears as the first operand
    /// and a scalar value is the second operand.
    /// </summary>
    /// <remarks>
    /// Ensures that the resulting coordinates of the <see cref="Point2D"/>
    /// object are correctly scaled by the scalar value.
    /// </remarks>
    [Test]
    public void MultiplyOperator_PointFirst_ShouldWorkCorrectly()
    {
        var point = new Point2D(10, 20);

        var result = point * 2.5f;

        result.X.Should().Be(25);
        result.Y.Should().Be(50);
    }

    /// <summary>
    /// Verifies that the multiplication operator correctly scales a <see cref="Point2D"/> instance
    /// when the scalar value is provided first in the expression.
    /// </summary>
    /// <remarks>
    /// This test ensures that the overloaded multiplication operator, where a scalar value appears
    /// before the <see cref="Point2D"/>, behaves identically to when the order is reversed. It
    /// validates that each coordinate of the <see cref="Point2D"/> is multiplied by the scalar
    /// factor, producing the expected result.
    /// </remarks>
    [Test]
    public void MultiplyOperator_ScalarFirst_ShouldWorkCorrectly()
    {
        var point = new Point2D(10, 20);

        var result = 2.5f * point;

        result.X.Should().Be(25);
        result.Y.Should().Be(50);
    }

    /// <summary>
    /// Tests that multiple chained operations (addition, subtraction, and multiplication)
    /// on instances of the <see cref="Point2D"/> class are correctly calculated.
    /// </summary>
    /// <remarks>
    /// This method verifies that the expected results are obtained when performing a
    /// series of operations on <see cref="Point2D"/> instances, such as addition and
    /// subtraction of points followed by scalar multiplication. It ensures proper compatibility
    /// of operator overloading and verifies the final computed values against expected outcomes.
    /// </remarks>
    [Test]
    public void ChainedOperations_ShouldWorkCorrectly()
    {
        var point1 = new Point2D(10, 10);
        var point2 = new Point2D(5, 5);
        var point3 = new Point2D(3, 3);

        var result = (point1 + point2 - point3) * 2;

        result.X.Should().Be(24);
        result.Y.Should().Be(24);
    }

    /// <summary>
    /// Verifies that the equality logic for the <see cref="Point2D"/> object operates as expected.
    /// Tests include checking equality for points with identical coordinates
    /// as well as inequality for points with different coordinates.
    /// </summary>
    [Test]
    public void Equality_ShouldWorkCorrectly()
    {
        var point1 = new Point2D(10.5f, 20.3f);
        var point2 = new Point2D(10.5f, 20.3f);
        var point3 = new Point2D(10.5f, 20.4f);

        point1.Should().Be(point2);
        point1.Should().NotBe(point3);
    }

    /// <summary>
    /// Tests the handling of extreme values (minimum, maximum, NaN, infinity) when initializing a <see cref="Point2D"/>.
    /// Validates that the X and Y coordinates of the point match the provided extreme values.
    /// </summary>
    /// <param name="x">The X-coordinate of the point, which can include extreme values (e.g., float.MinValue, float.MaxValue, float.NaN, float.PositiveInfinity, float.NegativeInfinity).</param>
    /// <param name="y">The Y-coordinate of the point, which can include extreme values (e.g., float.MinValue, float.MaxValue, float.NaN, float.PositiveInfinity, float.NegativeInfinity).</param>
    [TestCase(float.MinValue, float.MinValue)]
    [TestCase(float.MaxValue, float.MaxValue)]
    [TestCase(float.NaN, float.NaN)]
    [TestCase(float.PositiveInfinity, float.NegativeInfinity)]
    public void ExtremeValues_ShouldBeHandled(float x, float y)
    {
        var point = new Point2D(x, y);

        point.X.Should().Be(x);
        point.Y.Should().Be(y);
    }

    /// <summary>
    /// Tests whether the overridden ToString method of the <see cref="Point2D"/> record
    /// provides a useful string representation of the object's data.
    /// </summary>
    /// <remarks>
    /// Verifies that the string output of a <see cref="Point2D"/> instance contains
    /// the X and Y coordinate values.
    /// </remarks>
    [Test]
    public void ToString_ShouldProvideUsefulRepresentation()
    {
        var point = new Point2D(10.5f, 20.3f);

        var str = point.ToString();

        str.Should().Contain("10.5");
        str.Should().Contain("20.3");
    }

    /// <summary>
    /// Verifies the accuracy of vector-related calculations involving Point2D instances.
    /// </summary>
    /// <remarks>
    /// This test evaluates the correctness of operations such as addition, scalar multiplication,
    /// and averaging calculations performed on Point2D objects. The test ensures that computed
    /// results align with expected values using a predefined example.
    /// </remarks>
    /// <seealso cref="Point2D"/>
    [Test]
    public void VectorCalculations_ShouldBeAccurate()
    {
        var a = new Point2D(3, 4);
        var b = new Point2D(6, 8);

        var midpoint = (a + b) * 0.5f;

        midpoint.X.Should().Be(4.5f);
        midpoint.Y.Should().Be(6f);
    }

    /// <summary>
    /// Verifies that operations performed on a <see cref="Point2D"/> instance do not mutate the original instance,
    /// ensuring immutability.
    /// </summary>
    [Test]
    public void Immutability_OperationsShouldNotModifyOriginal()
    {
        var original = new Point2D(10, 20);
        var other = new Point2D(5, 5);

        var _ = original + other;
        var __ = original - other;
        var ___ = original * 2;

        original.X.Should().Be(10);
        original.Y.Should().Be(20);
    }
}
