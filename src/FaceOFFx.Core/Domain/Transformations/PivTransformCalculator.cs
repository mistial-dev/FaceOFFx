using FaceOFFx.Core.Domain.Common;
using FaceOFFx.Core.Domain.Detection;

namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Pure functions for calculating PIV (Personal Identity Verification) transformation parameters.
/// These functions perform mathematical calculations without side effects, making them easily testable.
/// </summary>
public static class PivTransformCalculator
{
    /// <summary>
    /// Calculates the rotation angle needed to level the eyes horizontally.
    /// </summary>
    /// <param name="leftEye">Position of the left eye center.</param>
    /// <param name="rightEye">Position of the right eye center.</param>
    /// <param name="maxRotationDegrees">Maximum allowed rotation in degrees. Default is 5.0 for PIV compliance.</param>
    /// <returns>
    /// Rotation angle in degrees, limited to the specified maximum.
    /// Positive values indicate counter-clockwise rotation.
    /// </returns>
    /// <remarks>
    /// The calculation uses arctangent to determine the angle between the eyes,
    /// then applies the specified rotation limit.
    /// </remarks>
    public static float CalculateRotationFromEyes(Point2D leftEye, Point2D rightEye, float maxRotationDegrees = 5.0f)
    {
        var deltaY = rightEye.Y - leftEye.Y;
        var deltaX = rightEye.X - leftEye.X;

        // Negative rotation to level the eyes (if right eye is higher, rotate clockwise)
        var rotationDegrees = -(float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);

        // Limit rotation to configured maximum
        return Math.Max(-maxRotationDegrees, Math.Min(maxRotationDegrees, rotationDegrees));
    }

    /// <summary>
    /// Calculates the crop region for PIV-compliant face positioning.
    /// </summary>
    /// <param name="eyeCenter">Center point between the left and right eyes after rotation.</param>
    /// <param name="faceBox">Detected face bounding box.</param>
    /// <param name="sourceDimensions">Original image dimensions.</param>
    /// <returns>
    /// A <see cref="CropRect"/> with normalized coordinates (0-1) representing
    /// the crop region that will center the face with PIV-compliant proportions.
    /// </returns>
    /// <remarks>
    /// PIV standards require:
    /// - Face width should be approximately 70% of image width
    /// - Eyes should be positioned at 45% from the top of the image
    /// - Final aspect ratio must be 3:4 (width:height)
    /// </remarks>
    public static CropRect CalculatePivCrop(
        Point2D eyeCenter,
        FaceBox faceBox,
        ImageDimensions sourceDimensions
    )
    {
        // For PIV: face should fill about 60-70% of frame width
        const float targetFaceWidthRatio = 0.70f;
        var desiredImageWidth = faceBox.Width / targetFaceWidthRatio;
        var desiredImageHeight = desiredImageWidth * 4.0f / 3.0f; // Maintain 3:4 aspect ratio

        // Ensure crop dimensions don't exceed source image bounds
        if (desiredImageWidth > sourceDimensions.Width)
        {
            desiredImageWidth = sourceDimensions.Width;
            desiredImageHeight = desiredImageWidth * 4.0f / 3.0f;
        }
        if (desiredImageHeight > sourceDimensions.Height)
        {
            desiredImageHeight = sourceDimensions.Height;
            desiredImageWidth = desiredImageHeight * 3.0f / 4.0f;
        }

        // Center crop around the eye position, with eyes at 45% from top (PIV standard)
        var cropX = Math.Max(0, eyeCenter.X - desiredImageWidth / 2);
        var cropY = Math.Max(0, eyeCenter.Y - desiredImageHeight * 0.45f);

        // Final bounds check to ensure crop stays within image
        cropX = Math.Min(cropX, sourceDimensions.Width - desiredImageWidth);
        cropY = Math.Min(cropY, sourceDimensions.Height - desiredImageHeight);

        return new CropRect
        {
            Left = cropX / sourceDimensions.Width,
            Top = cropY / sourceDimensions.Height,
            Width = desiredImageWidth / sourceDimensions.Width,
            Height = desiredImageHeight / sourceDimensions.Height,
        };
    }

    /// <summary>
    /// Calculates the scale factor needed to fit the source image into PIV dimensions (420x560).
    /// </summary>
    /// <param name="sourceDimensions">Original image dimensions.</param>
    /// <returns>
    /// Scale factor (less than or equal to 1.0) that maintains aspect ratio
    /// while fitting the image into 420x560 PIV dimensions.
    /// </returns>
    /// <remarks>
    /// The scale factor is calculated as the minimum of the width and height ratios
    /// to ensure the entire image fits within PIV boundaries without distortion.
    /// </remarks>
    public static float CalculateScaleFactor(ImageDimensions sourceDimensions)
    {
        const int pivWidth = 420;
        const int pivHeight = 560;

        var scaleX = (float)pivWidth / sourceDimensions.Width;
        var scaleY = (float)pivHeight / sourceDimensions.Height;

        return Math.Min(scaleX, scaleY);
    }

    /// <summary>
    /// Calculates the new position of a point after rotation around the image center.
    /// </summary>
    /// <param name="point">Original point position.</param>
    /// <param name="rotationDegrees">Rotation angle in degrees.</param>
    /// <param name="imageDimensions">Image dimensions for calculating center point.</param>
    /// <returns>New position of the point after rotation.</returns>
    /// <remarks>
    /// This function applies 2D rotation transformation around the image center.
    /// Useful for calculating where facial features will be positioned after image rotation.
    /// </remarks>
    public static Point2D RotatePointAroundImageCenter(
        Point2D point,
        float rotationDegrees,
        ImageDimensions imageDimensions
    )
    {
        var imageCenterX = imageDimensions.Width / 2f;
        var imageCenterY = imageDimensions.Height / 2f;

        // Translate point to origin (image center)
        var translatedX = point.X - imageCenterX;
        var translatedY = point.Y - imageCenterY;

        // Apply rotation (convert degrees to radians)
        var angleRad = rotationDegrees * Math.PI / 180;
        var rotatedX = (float)(translatedX * Math.Cos(angleRad) - translatedY * Math.Sin(angleRad));
        var rotatedY = (float)(translatedX * Math.Sin(angleRad) + translatedY * Math.Cos(angleRad));

        // Translate back
        return new Point2D(rotatedX + imageCenterX, rotatedY + imageCenterY);
    }

    /// <summary>
    /// Calculates the center point between two eye positions.
    /// </summary>
    /// <param name="leftEye">Left eye center position.</param>
    /// <param name="rightEye">Right eye center position.</param>
    /// <returns>Center point between the two eyes.</returns>
    public static Point2D CalculateEyeCenter(Point2D leftEye, Point2D rightEye)
    {
        return new Point2D((leftEye.X + rightEye.X) / 2, (leftEye.Y + rightEye.Y) / 2);
    }
}
