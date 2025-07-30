using FaceOFFx.Core.Domain.Common;
using FaceOFFx.Core.Domain.Standards;
using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Detection;

/// <summary>
/// Represents the 68-point facial landmark model for comprehensive facial feature detection.
/// </summary>
/// <param name="Points">
/// A read-only list of exactly 68 landmark points following the standard 68-point annotation scheme.
/// </param>
/// <remarks>
/// <para>
/// The 68-point facial landmark model is a widely adopted standard in facial analysis,
/// providing detailed feature localization for face alignment, expression analysis, and
/// biometric applications like PIV (Personal Identity Verification) image processing.
/// </para>
/// <para>
/// The 68 points are organized as follows:
/// - Points 0-16: Jaw line (face contour from left to right)
/// - Points 17-21: Left eyebrow (from outside to inside)
/// - Points 22-26: Right eyebrow (from inside to outside)
/// - Points 27-35: Nose (bridge, tip, and nostrils)
/// - Points 36-41: Left eye (clockwise from left corner)
/// - Points 42-47: Right eye (clockwise from left corner)
/// - Points 48-67: Mouth (outer lips clockwise, then inner lips)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create landmarks from detection model output
/// var landmarkPoints = new List&lt;Point2D&gt;();
/// for (int i = 0; i &lt; 68; i++)
/// {
///     landmarkPoints.Add(new Point2D(x[i], y[i]));
/// }
///
/// var landmarks = new FaceLandmarks68(landmarkPoints);
/// if (landmarks.IsValid)
/// {
///     var eyeDistance = Math.Sqrt(
///         Math.Pow(landmarks.RightEyeCenter.X - landmarks.LeftEyeCenter.X, 2) +
///         Math.Pow(landmarks.RightEyeCenter.Y - landmarks.LeftEyeCenter.Y, 2)
///     );
///     Console.WriteLine($"Inter-eye distance: {eyeDistance:F2} pixels");
/// }
/// </code>
/// </example>
[PublicAPI]
public record FaceLandmarks68(IReadOnlyList<Point2D> Points)
{
    /// <summary>
    /// Gets the expected number of landmark points in this model.
    /// </summary>
    /// <value>Always returns 68.</value>
    public int Count => 68;

    /// <summary>
    /// Gets the center point of the left eye based on the eye contour landmarks.
    /// </summary>
    /// <value>
    /// The geometric center of points 36-41, which form the left eye contour.
    /// </value>
    /// <remarks>
    /// The left eye center is calculated as the average position of the six points
    /// that define the eye contour. This center point is crucial for face alignment
    /// operations, particularly for calculating the rotation angle needed for
    /// PIV-compliant images where eyes must be horizontally aligned.
    /// </remarks>
    public Point2D LeftEyeCenter => ComputeCenter(Points.Skip(36).Take(6).ToList()); // Points 36-41

    /// <summary>
    /// Gets the center point of the right eye based on the eye contour landmarks.
    /// </summary>
    /// <value>
    /// The geometric center of points 42-47, which form the right eye contour.
    /// </value>
    /// <remarks>
    /// The right eye center is calculated as the average position of the six points
    /// that define the eye contour. Together with the left eye center, this point
    /// enables accurate face rotation correction and inter-pupillary distance measurement.
    /// </remarks>
    public Point2D RightEyeCenter => ComputeCenter(Points.Skip(42).Take(6).ToList()); // Points 42-47

    /// <summary>
    /// Computes the geometric center of a collection of points.
    /// </summary>
    /// <param name="points">The points to average.</param>
    /// <returns>A point representing the average X and Y coordinates.</returns>
    private static Point2D ComputeCenter(IReadOnlyList<Point2D> points)
    {
        var x = points.Average(p => p.X);
        var y = points.Average(p => p.Y);
        return new Point2D(x, y);
    }

    /// <summary>
    /// Gets a value indicating whether this landmark set contains the expected number of points.
    /// </summary>
    /// <value>
    /// <c>true</c> if the Points collection contains exactly 68 landmarks; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This validation ensures that the landmark data is complete before use in downstream
    /// processing. Incomplete landmark sets may result from partial face occlusion, detection
    /// failures, or data corruption.
    /// </remarks>
    public bool IsValid => Points.Count == 68;

    /// <summary>
    /// Creates an Appendix C.6 compliant ROI set for PIV images.
    /// </summary>
    /// <param name="imageWidth">The width of the PIV image (should be 420 for standard PIV).</param>
    /// <param name="imageHeight">The height of the PIV image (should be 560 for standard PIV).</param>
    /// <returns>A Result containing the FacialRoiSet with Appendix C.6 Inner Region or an error message.</returns>
    /// <remarks>
    /// Creates the Inner Region as specified in INCITS 385-2004 Appendix C.6.
    /// This method is for PIV-compliant images only and uses the standard rectangular region approach.
    /// </remarks>
    /// <example>
    /// <code>
    /// var roiResult = landmarks.CalculateRoiSet(420, 560); // Standard PIV dimensions
    /// if (roiResult.IsSuccess)
    /// {
    ///     var rois = roiResult.Value;
    ///     Console.WriteLine($"Inner ROI: {rois.InnerRegion.BoundingBox}");
    /// }
    /// </code>
    /// </example>
    public Result<FacialRoiSet> CalculateRoiSet(int imageWidth, int imageHeight)
    {
        return FacialRoiSet.CreateAppendixC6(imageWidth, imageHeight);
    }

    /// <summary>
    /// Calculates the PIV compliance lines (AA, BB, CC) from the 68-point landmarks.
    /// </summary>
    /// <returns>PIV compliance lines for face positioning validation</returns>
    /// <remarks>
    /// Line AA (Vertical Center): Passes through nose bridge and mouth center
    /// Line BB (Horizontal Eye): Passes through both eye centers
    /// Line CC (Head Width): Level line between the widest face contour points
    /// </remarks>
    public PivComplianceLines CalculatePivLines()
    {
        if (!IsValid)
        {
            throw new InvalidOperationException(
                "Cannot calculate PIV lines from invalid landmarks (must have exactly 68 points)"
            );
        }

        // Calculate nose center from nose bridge landmarks (27-30)
        var noseBridgePoints = Points.Skip(27).Take(4).ToList(); // Points 27-30: nose bridge
        var noseCenter = ComputeCenter(noseBridgePoints);

        // Calculate mouth center from key mouth landmarks
        // Use outer mouth corners (48, 54) and top/bottom centers (51, 57)
        var mouthKeyPoints = new[] { Points[48], Points[51], Points[54], Points[57] };
        var mouthCenter = ComputeCenter(mouthKeyPoints);

        // Eye centers are already available
        var leftEyeCenter = LeftEyeCenter;
        var rightEyeCenter = RightEyeCenter;

        // Find the actual widest points from the face contour (points 0-16)
        var faceContourPoints = Points.Take(17).ToList(); // Points 0-16: jaw/face contour

        // Find leftmost and rightmost points
        var leftmostPoint = faceContourPoints.OrderBy(p => p.X).First();
        var rightmostPoint = faceContourPoints.OrderByDescending(p => p.X).First();

        // Calculate the Y-position as the average of the widest points
        var levelY = (leftmostPoint.Y + rightmostPoint.Y) / 2.0f;

        // Create level ear points for aesthetic head width line
        var leftEarPoint = new Point2D(leftmostPoint.X, levelY);
        var rightEarPoint = new Point2D(rightmostPoint.X, levelY);

        // Calculate Line AA (Vertical Center Line) - average of nose and mouth X coordinates
        var lineAA_X = (noseCenter.X + mouthCenter.X) / 2.0f;

        // Calculate Line BB (Horizontal Eye Line) - average of eye Y coordinates
        var lineBB_Y = (leftEyeCenter.Y + rightEyeCenter.Y) / 2.0f;

        // Calculate Line CC (Head Width) - now using actual widest points
        var lineCC_Width = rightmostPoint.X - leftmostPoint.X;

        return new PivComplianceLines(
            lineAA_X,
            lineBB_Y,
            lineCC_Width,
            noseCenter,
            mouthCenter,
            leftEyeCenter,
            rightEyeCenter,
            leftEarPoint,
            rightEarPoint
        );
    }
}
