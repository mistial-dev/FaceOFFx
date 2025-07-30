using JetBrains.Annotations;

namespace FaceOFFx.Core.Domain.Detection;

/// <summary>
/// Represents a detected face within an image, containing the face's location and detection confidence.
/// </summary>
/// <param name="BoundingBox">The bounding box that defines the face's location within the image.</param>
/// <param name="Confidence">The confidence score of the face detection, ranging from 0.0 to 1.0.</param>
/// <param name="Landmarks5">Optional 5-point facial landmarks from face detection (eyes, nose, mouth corners).</param>
/// <remarks>
/// This record is used as the primary output from face detection models. The confidence score
/// indicates the model's certainty that the detected region contains a face. Some detectors
/// also provide 5 key facial landmarks which can be used for basic alignment operations.
/// </remarks>
/// <example>
/// <code>
/// var faceBox = FaceBox.Create(100, 150, 200, 250).Value;
/// var detectedFace = new DetectedFace(faceBox, 0.95f);
///
/// if (detectedFace.IsValid)
/// {
///     Console.WriteLine($"Valid face detected with {detectedFace.Confidence:P0} confidence");
/// }
/// </code>
/// </example>
[PublicAPI]
public record DetectedFace(
    FaceBox BoundingBox,
    float Confidence,
    Maybe<FaceLandmarks5> Landmarks5 = default
)
{
    /// <summary>
    /// Determines whether this detected face is valid for further processing.
    /// </summary>
    /// <value>
    /// <c>true</c> if the confidence score exceeds 0.5 (50%) and the bounding box area
    /// is greater than 100 square pixels; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property implements basic quality filtering to exclude low-confidence detections
    /// and extremely small face regions that are likely to be false positives or unusable
    /// for downstream processing like landmark detection or recognition.
    /// </remarks>
    public bool IsValid => Confidence > 0.5f && BoundingBox.Area > 100;
}
