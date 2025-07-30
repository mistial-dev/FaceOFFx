using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Common;

/// <summary>
/// Represents a two-dimensional point in image coordinate space.
/// </summary>
/// <remarks>
/// This immutable record represents a point using floating-point coordinates, which allows for
/// sub-pixel precision in image processing operations. The coordinate system follows the standard
/// image convention where (0,0) is the top-left corner, X increases to the right, and Y increases
/// downward.
/// </remarks>
/// <example>
/// <code>
/// // Create points
/// var point1 = new Point2D(100.5f, 200.3f);
/// var point2 = new Point2D(150.7f, 250.1f);
///
/// // Calculate distance
/// float distance = point1.DistanceTo(point2);
///
/// // Vector operations
/// var midpoint = (point1 + point2) * 0.5f;
/// var offset = point2 - point1;
///
/// // Use predefined origin
/// var origin = Point2D.Zero;
/// </code>
/// </example>
/// <param name="X">The X-coordinate of the point (horizontal position).</param>
/// <param name="Y">The Y-coordinate of the point (vertical position).</param>
[PublicAPI]
public record Point2D(float X, float Y)
{
    /// <summary>
    /// Gets a <see cref="Point2D"/> representing the origin (0, 0).
    /// </summary>
    /// <value>A point at coordinates (0, 0).</value>
    /// <remarks>
    /// This is commonly used as a default value or reference point in calculations.
    /// </remarks>
    public static Point2D Zero => new(0, 0);

    /// <summary>
    /// Calculates the Euclidean distance between this point and another point.
    /// </summary>
    /// <param name="other">The other point to calculate the distance to.</param>
    /// <returns>The Euclidean distance between the two points.</returns>
    /// <remarks>
    /// The distance is calculated using the Pythagorean theorem: √((x₂-x₁)² + (y₂-y₁)²).
    /// This is useful for determining proximity between facial landmarks or other features.
    /// </remarks>
    /// <example>
    /// <code>
    /// var eye1 = new Point2D(100, 150);
    /// var eye2 = new Point2D(200, 150);
    /// float eyeDistance = eye1.DistanceTo(eye2); // Returns 100.0
    /// </code>
    /// </example>
    public float DistanceTo(Point2D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Adds another point to this point, treating them as vectors.
    /// </summary>
    /// <param name="other">The point to add.</param>
    /// <returns>A new point representing the vector sum.</returns>
    /// <remarks>
    /// This operation is useful for translating points or combining offsets.
    /// </remarks>
    public Point2D Add(Point2D other) => new(X + other.X, Y + other.Y);

    /// <summary>
    /// Subtracts another point from this point, treating them as vectors.
    /// </summary>
    /// <param name="other">The point to subtract.</param>
    /// <returns>A new point representing the vector difference.</returns>
    /// <remarks>
    /// This operation is useful for calculating offsets or relative positions between points.
    /// </remarks>
    public Point2D Subtract(Point2D other) => new(X - other.X, Y - other.Y);

    /// <summary>
    /// Scales this point by a scalar factor.
    /// </summary>
    /// <param name="factor">The scaling factor to apply.</param>
    /// <returns>A new point with coordinates multiplied by the factor.</returns>
    /// <remarks>
    /// This operation is useful for resizing, zooming, or normalizing coordinates.
    /// A factor of 1.0 returns an identical point, while 2.0 doubles the coordinates.
    /// </remarks>
    public Point2D Scale(float factor) => new(X * factor, Y * factor);

    /// <summary>
    /// Adds two points together using the + operator.
    /// </summary>
    /// <param name="left">The first point.</param>
    /// <param name="right">The second point.</param>
    /// <returns>A new point representing the sum of the two points.</returns>
    public static Point2D operator +(Point2D left, Point2D right) => left.Add(right);

    /// <summary>
    /// Subtracts one point from another using the - operator.
    /// </summary>
    /// <param name="left">The point to subtract from.</param>
    /// <param name="right">The point to subtract.</param>
    /// <returns>A new point representing the difference.</returns>
    public static Point2D operator -(Point2D left, Point2D right) => left.Subtract(right);

    /// <summary>
    /// Multiplies a point by a scalar value using the * operator.
    /// </summary>
    /// <param name="point">The point to scale.</param>
    /// <param name="scalar">The scaling factor.</param>
    /// <returns>A new point with scaled coordinates.</returns>
    public static Point2D operator *(Point2D point, float scalar) => point.Scale(scalar);

    /// <summary>
    /// Multiplies a point by a scalar value using the * operator (commutative).
    /// </summary>
    /// <param name="scalar">The scaling factor.</param>
    /// <param name="point">The point to scale.</param>
    /// <returns>A new point with scaled coordinates.</returns>
    /// <remarks>
    /// This overload allows for natural mathematical expressions like "2 * point".
    /// </remarks>
    public static Point2D operator *(float scalar, Point2D point) => point.Scale(scalar);
}
