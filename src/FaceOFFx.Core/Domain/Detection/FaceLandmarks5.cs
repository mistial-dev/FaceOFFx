using FaceOFFx.Core.Domain.Common;
using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Detection;

/// <summary>
/// Represents the 5 key facial landmarks that can be extracted by face detectors.
/// </summary>
/// <param name="LeftEye">The position of the left eye center.</param>
/// <param name="RightEye">The position of the right eye center.</param>
/// <param name="Nose">The position of the nose tip.</param>
/// <param name="LeftMouth">The position of the left mouth corner.</param>
/// <param name="RightMouth">The position of the right mouth corner.</param>
/// <remarks>
/// These 5 landmarks are sufficient for basic face alignment operations like rotation
/// calculation, avoiding the need to run more expensive 68-point landmark detection
/// for initial face orientation assessment.
/// </remarks>
[PublicAPI]
public record FaceLandmarks5(
    Point2D LeftEye,
    Point2D RightEye,
    Point2D Nose,
    Point2D LeftMouth,
    Point2D RightMouth
)
{
    /// <summary>
    /// Calculates the rotation angle needed to level the eyes horizontally.
    /// </summary>
    /// <param name="maxRotationDegrees">Maximum allowed rotation in degrees. Default is 5.0 for PIV compliance.</param>
    /// <returns>Rotation angle in degrees. Positive values indicate clockwise rotation needed.</returns>
    public float CalculateEyeLevelRotation(float maxRotationDegrees = 5.0f)
    {
        var deltaY = RightEye.Y - LeftEye.Y;
        var deltaX = RightEye.X - LeftEye.X;

        // Calculate rotation to level eyes (negative to correct clockwise tilt)
        var rotationDegrees = -(float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);

        // Limit rotation to configured maximum
        return Math.Max(-maxRotationDegrees, Math.Min(maxRotationDegrees, rotationDegrees));
    }

    /// <summary>
    /// Gets the center point between the eyes.
    /// </summary>
    public Point2D EyeCenter => new((LeftEye.X + RightEye.X) / 2f, (LeftEye.Y + RightEye.Y) / 2f);

    /// <summary>
    /// Gets the inter-ocular distance (distance between eyes).
    /// </summary>
    public float InterOcularDistance =>
        (float)Math.Sqrt(Math.Pow(RightEye.X - LeftEye.X, 2) + Math.Pow(RightEye.Y - LeftEye.Y, 2));
}
