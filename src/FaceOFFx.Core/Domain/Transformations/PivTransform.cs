using FaceOFFx.Core.Domain.Detection;
using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Represents the transformation parameters required to convert a source image
/// into a PIV-compliant photograph meeting FIPS 201-3 standards.
/// </summary>
/// <remarks>
/// <para>
/// A PIV transformation consists of three sequential operations:
/// 1. Rotation - Corrects head tilt by aligning eyes horizontally
/// 2. Cropping - Extracts face region with proper positioning
/// 3. Scaling - Resizes to exact 420x560 dimensions
/// </para>
/// <para>
/// The transformation ensures the resulting image meets all PIV requirements:
/// - Face fills approximately 70% of frame width
/// - Eyes positioned at 45% from top of frame
/// - Head rotation within ±5 degrees of vertical
/// - Maintains 3:4 aspect ratio throughout
/// </para>
/// </remarks>
[PublicAPI]
public sealed record PivTransform
{
    /// <summary>
    /// Gets the rotation angle in degrees to level the face.
    /// </summary>
    /// <value>
    /// Rotation angle where positive values rotate clockwise, negative counter-clockwise.
    /// Limited to ±5 degrees for PIV compliance.
    /// </value>
    /// <remarks>
    /// Rotation is calculated from the angle between the eyes to ensure
    /// they are horizontally aligned in the final image.
    /// </remarks>
    public float RotationDegrees { get; init; }

    /// <summary>
    /// Gets the crop region defining which portion of the image to extract.
    /// </summary>
    /// <value>
    /// A <see cref="CropRect"/> with normalized coordinates (0-1) relative to image dimensions.
    /// </value>
    /// <remarks>
    /// The crop region is calculated to:
    /// - Center the face horizontally
    /// - Position eyes at 45% from top
    /// - Maintain 3:4 aspect ratio
    /// - Include appropriate margins around the face
    /// </remarks>
    public required CropRect CropRegion { get; init; }

    /// <summary>
    /// Gets the scale factor applied during the resize operation.
    /// </summary>
    /// <value>
    /// Scale multiplier where 1.0 means no scaling, &lt;1.0 means downscale, &gt;1.0 means upscale.
    /// </value>
    /// <remarks>
    /// The scale factor is calculated to resize the cropped region to exactly
    /// 420x560 pixels while maintaining aspect ratio and image quality.
    /// </remarks>
    public float ScaleFactor { get; init; }

    /// <summary>
    /// Gets the final dimensions of the PIV-compliant image.
    /// </summary>
    /// <value>
    /// An <see cref="ImageDimensions"/> record that should always be 420x560 for PIV compliance.
    /// </value>
    /// <remarks>
    /// These dimensions are mandated by FIPS 201-3 for facial images on
    /// government credentials and ensure consistency across all PIV systems.
    /// </remarks>
    public required ImageDimensions TargetDimensions { get; init; }

    /// <summary>
    /// Gets a value indicating whether this transformation results in a PIV-compliant image.
    /// </summary>
    /// <value>
    /// <c>true</c> if all transformation parameters meet PIV requirements; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Compliance requires:
    /// - Target dimensions of 420x560
    /// - Rotation within ±5 degrees
    /// - Proper face positioning and sizing
    /// </remarks>
    public bool IsPivCompliant { get; init; }

    /// <summary>
    /// Creates an identity transformation that performs no modifications.
    /// </summary>
    /// <param name="sourceDimensions">The dimensions of the source image.</param>
    /// <returns>A <see cref="PivTransform"/> that leaves the image unchanged.</returns>
    /// <remarks>
    /// An identity transform has zero rotation, full image crop (no cropping),
    /// and 1.0 scale factor. Useful as a default or when no transformation is needed.
    /// Note that an identity transform is typically not PIV-compliant unless the
    /// source image already meets all requirements.
    /// </remarks>
    public static PivTransform Identity(ImageDimensions sourceDimensions) =>
        new()
        {
            RotationDegrees = 0f,
            CropRegion = CropRect.Full,
            ScaleFactor = 1f,
            TargetDimensions = sourceDimensions,
            IsPivCompliant = false,
        };

    /// <summary>
    /// Validates that this transformation contains reasonable and valid parameters.
    /// </summary>
    /// <returns>
    /// A <see cref="Result"/> indicating success or containing an error message
    /// describing which parameter is invalid.
    /// </returns>
    /// <remarks>
    /// Validation ensures:
    /// - Rotation angle is within ±45 degrees (PIV limit is ±5)
    /// - Scale factor is positive and reasonable (0 &lt; scale ≤ 10)
    /// - Target dimensions meet minimum PIV requirements (≥420x420)
    /// </remarks>
    public Result Validate()
    {
        if (Math.Abs(RotationDegrees) > 45f)
        {
            return Result.Failure("Rotation angle too large for PIV compliance");
        }

        if (ScaleFactor is <= 0f or > 10f)
        {
            return Result.Failure("Invalid scale factor");
        }

        if (TargetDimensions.Width < 420 || TargetDimensions.Height < 420)
        {
            return Result.Failure("Target dimensions too small for PIV compliance");
        }

        return Result.Success();
    }
}

/// <summary>
/// Represents a rectangular crop region using normalized coordinates relative to image dimensions.
/// </summary>
/// <remarks>
/// <para>
/// CropRect uses normalized coordinates (0.0 to 1.0) to define a region
/// independent of actual image dimensions. This allows the same crop
/// definition to work with images of different sizes.
/// </para>
/// <para>
/// Coordinate system:
/// - (0,0) = top-left corner of image
/// - (1,1) = bottom-right corner of image
/// - Width/Height of 1.0 = full image dimension
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Crop center 50% of image
/// var centerCrop = new CropRect
/// {
///     Left = 0.25f,
///     Top = 0.25f,
///     Width = 0.5f,
///     Height = 0.5f
/// };
/// </code>
/// </example>
[PublicAPI]
public sealed record CropRect
{
    /// <summary>
    /// Gets the normalized X coordinate of the left edge (0.0 = left edge, 1.0 = right edge).
    /// </summary>
    public float Left { get; init; }

    /// <summary>
    /// Gets the normalized Y coordinate of the top edge (0.0 = top edge, 1.0 = bottom edge).
    /// </summary>
    public float Top { get; init; }

    /// <summary>
    /// Gets the normalized width of the crop region (1.0 = full image width).
    /// </summary>
    public float Width { get; init; }

    /// <summary>
    /// Gets the normalized height of the crop region (1.0 = full image height).
    /// </summary>
    public float Height { get; init; }

    /// <summary>
    /// Gets the normalized X coordinate of the right edge.
    /// </summary>
    /// <value>The sum of <see cref="Left"/> and <see cref="Width"/>.</value>
    public float Right => Left + Width;

    /// <summary>
    /// Gets the normalized Y coordinate of the bottom edge.
    /// </summary>
    /// <value>The sum of <see cref="Top"/> and <see cref="Height"/>.</value>
    public float Bottom => Top + Height;

    /// <summary>
    /// Gets a crop rectangle representing the full image with no cropping.
    /// </summary>
    /// <value>A <see cref="CropRect"/> with coordinates (0,0) and size (1,1).</value>
    /// <remarks>
    /// Use this when no cropping is needed or as a default value.
    /// </remarks>
    public static CropRect Full =>
        new()
        {
            Left = 0f,
            Top = 0f,
            Width = 1f,
            Height = 1f,
        };

    /// <summary>
    /// Creates a crop rectangle from pixel coordinates by converting to normalized values.
    /// </summary>
    /// <param name="x">X coordinate of top-left corner in pixels.</param>
    /// <param name="y">Y coordinate of top-left corner in pixels.</param>
    /// <param name="width">Width of crop region in pixels.</param>
    /// <param name="height">Height of crop region in pixels.</param>
    /// <param name="imageWidth">Total width of the image in pixels.</param>
    /// <param name="imageHeight">Total height of the image in pixels.</param>
    /// <returns>A new <see cref="CropRect"/> with normalized coordinates.</returns>
    /// <remarks>
    /// This method converts absolute pixel coordinates to normalized coordinates
    /// by dividing by the image dimensions. Useful when working with specific
    /// pixel regions from image analysis or face detection.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert a 100x100 pixel region at (50,50) in a 800x600 image
    /// var crop = CropRect.FromPixels(50, 50, 100, 100, 800, 600);
    /// // Results in: Left=0.0625, Top=0.0833, Width=0.125, Height=0.1667
    /// </code>
    /// </example>
    public static CropRect FromPixels(
        int x,
        int y,
        int width,
        int height,
        int imageWidth,
        int imageHeight
    ) =>
        new()
        {
            Left = (float)x / imageWidth,
            Top = (float)y / imageHeight,
            Width = (float)width / imageWidth,
            Height = (float)height / imageHeight,
        };

    /// <summary>
    /// Converts this normalized crop rectangle to a <see cref="FaceBox"/> with pixel coordinates.
    /// </summary>
    /// <param name="imageWidth">Target image width in pixels.</param>
    /// <param name="imageHeight">Target image height in pixels.</param>
    /// <returns>A <see cref="FaceBox"/> with absolute pixel coordinates.</returns>
    /// <remarks>
    /// This method performs the inverse of <see cref="FromPixels"/>, converting
    /// normalized coordinates back to pixel coordinates for a specific image size.
    /// </remarks>
    /// <example>
    /// <code>
    /// var crop = new CropRect { Left = 0.25f, Top = 0.25f, Width = 0.5f, Height = 0.5f };
    /// var faceBox = crop.ToFaceBox(800, 600);
    /// // Results in: X=200, Y=150, Width=400, Height=300
    /// </code>
    /// </example>
    public FaceBox ToFaceBox(int imageWidth, int imageHeight) =>
        FaceBox
            .Create(Left * imageWidth, Top * imageHeight, Width * imageWidth, Height * imageHeight)
            .Value;
}
