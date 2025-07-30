using FaceOFFx.Core.Domain.Common;

namespace FaceOFFx.Core.Domain.Detection;

/// <summary>
/// Represents a bounding box for a detected face using pixel coordinates.
/// </summary>
/// <remarks>
/// The FaceBox uses a coordinate system where (0,0) is at the top-left corner of the image,
/// with X increasing to the right and Y increasing downward. All measurements are in pixels.
/// This record is immutable and can only be created through the <see cref="Create"/> factory method
/// to ensure validation of dimensions.
/// </remarks>
/// <example>
/// <code>
/// // Create a face box at position (100, 150) with size 200x250
/// var result = FaceBox.Create(100, 150, 200, 250);
/// if (result.IsSuccess)
/// {
///     var box = result.Value;
///     Console.WriteLine($"Face center: ({box.Center.X}, {box.Center.Y})");
///     Console.WriteLine($"Face area: {box.Area} pixels");
/// }
/// </code>
/// </example>
public record FaceBox
{
    /// <summary>
    /// Gets the X-coordinate of the top-left corner of the bounding box.
    /// </summary>
    /// <value>The horizontal position in pixels from the left edge of the image.</value>
    public float X { get; }
    
    /// <summary>
    /// Gets the Y-coordinate of the top-left corner of the bounding box.
    /// </summary>
    /// <value>The vertical position in pixels from the top edge of the image.</value>
    public float Y { get; }
    
    /// <summary>
    /// Gets the width of the bounding box.
    /// </summary>
    /// <value>The horizontal size in pixels. Always positive.</value>
    public float Width { get; }
    
    /// <summary>
    /// Gets the height of the bounding box.
    /// </summary>
    /// <value>The vertical size in pixels. Always positive.</value>
    public float Height { get; }

    private FaceBox(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Creates a new FaceBox instance with validation.
    /// </summary>
    /// <param name="x">The X-coordinate of the top-left corner.</param>
    /// <param name="y">The Y-coordinate of the top-left corner.</param>
    /// <param name="width">The width of the box. Must be positive.</param>
    /// <param name="height">The height of the box. Must be positive.</param>
    /// <returns>
    /// A <see cref="Result{FaceBox}"/> containing the created FaceBox if validation succeeds,
    /// or a failure result with an error message if width or height are non-positive.
    /// </returns>
    /// <remarks>
    /// This factory method ensures that all FaceBox instances have valid dimensions.
    /// Negative or zero width/height values will result in a failure.
    /// </remarks>
    public static Result<FaceBox> Create(float x, float y, float width, float height)
    {
        if (width <= 0)
        {
            return Result.Failure<FaceBox>($"Width must be positive, but was {width}");
        }
        if (height <= 0)
        {
            return Result.Failure<FaceBox>($"Height must be positive, but was {height}");
        }

        return Result.Success(new FaceBox(x, y, width, height));
    }

    /// <summary>
    /// Gets the left edge X-coordinate of the bounding box.
    /// </summary>
    /// <value>Same as <see cref="X"/>.</value>
    public float Left => X;
    
    /// <summary>
    /// Gets the top edge Y-coordinate of the bounding box.
    /// </summary>
    /// <value>Same as <see cref="Y"/>.</value>
    public float Top => Y;
    
    /// <summary>
    /// Gets the right edge X-coordinate of the bounding box.
    /// </summary>
    /// <value>The X-coordinate of the right edge (X + Width).</value>
    public float Right => X + Width;
    
    /// <summary>
    /// Gets the bottom edge Y-coordinate of the bounding box.
    /// </summary>
    /// <value>The Y-coordinate of the bottom edge (Y + Height).</value>
    public float Bottom => Y + Height;
    
    /// <summary>
    /// Gets the center point of the bounding box.
    /// </summary>
    /// <value>A <see cref="Point2D"/> representing the geometric center of the box.</value>
    public Point2D Center => new(X + Width / 2, Y + Height / 2);
    
    /// <summary>
    /// Gets the area of the bounding box in square pixels.
    /// </summary>
    /// <value>The product of Width and Height.</value>
    public float Area => Width * Height;

    /// <summary>
    /// Determines whether the specified point lies within this bounding box.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns>
    /// <c>true</c> if the point is inside or on the edge of the bounding box; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The containment test is inclusive of the box edges.
    /// </remarks>
    public bool Contains(Point2D point) =>
        point.X >= Left && point.X <= Right &&
        point.Y >= Top && point.Y <= Bottom;

    /// <summary>
    /// Calculates the Intersection over Union (IoU) metric between this box and another.
    /// </summary>
    /// <param name="other">The other face box to compare with.</param>
    /// <returns>
    /// A value between 0.0 and 1.0 representing the IoU ratio.
    /// Returns 0.0 if the boxes don't overlap, and 1.0 if they are identical.
    /// </returns>
    /// <remarks>
    /// IoU is commonly used in object detection to measure the overlap between predicted
    /// and ground truth bounding boxes, or to determine if two detections refer to the
    /// same object. A typical threshold for considering boxes as the same object is 0.5.
    /// </remarks>
    /// <example>
    /// <code>
    /// var box1 = FaceBox.Create(0, 0, 100, 100).Value;
    /// var box2 = FaceBox.Create(50, 50, 100, 100).Value;
    /// float iou = box1.IntersectionOverUnion(box2);
    /// // iou will be 0.25 / 1.75 ≈ 0.143
    /// </code>
    /// </example>
    public float IntersectionOverUnion(FaceBox other)
    {
        var intersectionLeft = Math.Max(Left, other.Left);
        var intersectionTop = Math.Max(Top, other.Top);
        var intersectionRight = Math.Min(Right, other.Right);
        var intersectionBottom = Math.Min(Bottom, other.Bottom);

        if (intersectionRight < intersectionLeft || intersectionBottom < intersectionTop)
        {
            return 0;
        }

        var intersectionArea = (intersectionRight - intersectionLeft) * (intersectionBottom - intersectionTop);
        var unionArea = Area + other.Area - intersectionArea;

        return intersectionArea / unionArea;
    }

    /// <summary>
    /// Creates a new FaceBox scaled by the specified factor.
    /// </summary>
    /// <param name="factor">The scaling factor to apply to all dimensions and coordinates.</param>
    /// <returns>A new FaceBox with all values multiplied by the factor.</returns>
    /// <remarks>
    /// This method scales both the position and size of the box. For example, a factor of 2.0
    /// will double all coordinates and dimensions, effectively scaling the box relative to
    /// the origin (0,0). Use this when resizing images to maintain relative positions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var original = FaceBox.Create(100, 100, 50, 60).Value;
    /// var scaled = original.Scale(0.5f); // Results in box at (50, 50) with size 25x30
    /// </code>
    /// </example>
    public FaceBox Scale(float factor) => 
        new(X * factor, Y * factor, Width * factor, Height * factor);

    /// <summary>
    /// Creates a new FaceBox expanded by the specified number of pixels on all sides.
    /// </summary>
    /// <param name="pixels">The number of pixels to expand in each direction.</param>
    /// <returns>
    /// A new FaceBox that is larger by 2*pixels in both width and height,
    /// with the top-left corner moved by -pixels in both X and Y.
    /// </returns>
    /// <remarks>
    /// This method is useful for adding padding around a detected face before extraction
    /// or processing. Negative values will shrink the box. The resulting box may have
    /// negative coordinates or extend beyond image boundaries, so boundary checking
    /// should be performed when using the expanded box.
    /// </remarks>
    /// <example>
    /// <code>
    /// var face = FaceBox.Create(100, 100, 200, 200).Value;
    /// var padded = face.Expand(10); // Results in box at (90, 90) with size 220x220
    /// </code>
    /// </example>
    public FaceBox Expand(float pixels) =>
        new(X - pixels, Y - pixels, Width + 2 * pixels, Height + 2 * pixels);
}