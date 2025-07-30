namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Base type for all transformation-related errors
/// </summary>
public abstract record TransformationError(string Message)
{
    /// <summary>
    /// Invalid geometry data prevents transformation calculation
    /// </summary>
    public sealed record InvalidGeometry(string Details) 
        : TransformationError($"Invalid geometry for transformation: {Details}");
    
    /// <summary>
    /// Transformation parameter exceeds acceptable bounds
    /// </summary>
    public sealed record TransformationOutOfBounds(string Parameter, float Value, string Bounds) 
        : TransformationError($"{Parameter} value {Value} is outside acceptable bounds: {Bounds}");
    
    /// <summary>
    /// Image dimensions insufficient for PIV compliance
    /// </summary>
    public sealed record InsufficientImageSize(int Width, int Height, int MinWidth, int MinHeight) 
        : TransformationError($"Image size {Width}x{Height} is below minimum required {MinWidth}x{MinHeight}");
    
    /// <summary>
    /// Face position makes PIV compliance impossible
    /// </summary>
    public sealed record FacePositionNotCorrectable(string Reason) 
        : TransformationError($"Face position cannot be corrected: {Reason}");
    
    /// <summary>
    /// Quality loss would exceed acceptable threshold
    /// </summary>
    public sealed record ExcessiveQualityLoss(float EstimatedLoss, float MaxAcceptable) 
        : TransformationError($"Estimated quality loss {EstimatedLoss:P0} exceeds maximum acceptable {MaxAcceptable:P0}");
    
    /// <summary>
    /// Missing required data for transformation
    /// </summary>
    public sealed record MissingRequiredData(string DataType) 
        : TransformationError($"Missing required data for transformation: {DataType}");
    
    /// <summary>
    /// Transformation would result in invalid output
    /// </summary>
    public sealed record InvalidTransformationResult(string Details) 
        : TransformationError($"Transformation would produce invalid result: {Details}");
}