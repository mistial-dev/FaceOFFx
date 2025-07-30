using FaceOFFx.Core.Domain.Common;

namespace FaceOFFx.Core.Domain.Analysis;

/// <summary>
/// Represents the state of eyes (open/closed)
/// </summary>
public record EyeStatePrediction
{
    /// <summary>
    /// Gets the detected state of the left eye
    /// </summary>
    public EyeState LeftEye { get; }
    
    /// <summary>
    /// Gets the detected state of the right eye
    /// </summary>
    public EyeState RightEye { get; }
    
    /// <summary>
    /// Gets the confidence score for the left eye detection
    /// </summary>
    public Confidence LeftEyeConfidence { get; }
    
    /// <summary>
    /// Gets the confidence score for the right eye detection
    /// </summary>
    public Confidence RightEyeConfidence { get; }

    private EyeStatePrediction(
        EyeState leftEye, 
        EyeState rightEye, 
        Confidence leftEyeConfidence, 
        Confidence rightEyeConfidence)
    {
        LeftEye = leftEye;
        RightEye = rightEye;
        LeftEyeConfidence = leftEyeConfidence;
        RightEyeConfidence = rightEyeConfidence;
    }

    /// <summary>
    /// Creates an eye state prediction from individual eye states and confidence scores
    /// </summary>
    /// <param name="leftEye">State of the left eye</param>
    /// <param name="leftConfidence">Confidence score for left eye (0-1)</param>
    /// <param name="rightEye">State of the right eye</param>
    /// <param name="rightConfidence">Confidence score for right eye (0-1)</param>
    /// <returns>Success with EyeStatePrediction if valid, otherwise failure</returns>
    public static Result<EyeStatePrediction> Create(
        EyeState leftEye,
        float leftConfidence,
        EyeState rightEye,
        float rightConfidence)
    {
        var leftConfResult = Confidence.Create(leftConfidence);
        var rightConfResult = Confidence.Create(rightConfidence);

        if (leftConfResult.IsFailure)
        {
            return Result.Failure<EyeStatePrediction>($"Left eye: {leftConfResult.Error}");
        }
        if (rightConfResult.IsFailure)
        {
            return Result.Failure<EyeStatePrediction>($"Right eye: {rightConfResult.Error}");
        }

        return Result.Success(new EyeStatePrediction(
            leftEye, rightEye, leftConfResult.Value, rightConfResult.Value));
    }

    /// <summary>
    /// Gets whether both eyes are detected as open
    /// </summary>
    public bool BothEyesOpen => LeftEye == EyeState.Open && RightEye == EyeState.Open;
    
    /// <summary>
    /// Gets whether both eyes are detected as closed
    /// </summary>
    public bool BothEyesClosed => LeftEye == EyeState.Closed && RightEye == EyeState.Closed;
    
    /// <summary>
    /// Gets whether one eye is open and the other is closed (winking)
    /// </summary>
    public bool IsWinking => LeftEye == EyeState.Open && RightEye == EyeState.Closed ||
                            LeftEye == EyeState.Closed && RightEye == EyeState.Open;

    /// <summary>
    /// Returns a string representation of the eye states
    /// </summary>
    /// <returns>String describing both eye states</returns>
    public override string ToString() => $"Left: {LeftEye}, Right: {RightEye}";
}

/// <summary>
/// Represents the possible states of an eye
/// </summary>
public enum EyeState
{
    /// <summary>
    /// Eye is detected as open
    /// </summary>
    Open,
    
    /// <summary>
    /// Eye is detected as closed
    /// </summary>
    Closed
}