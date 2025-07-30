namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Represents a rotation angle in degrees
/// </summary>
public sealed record RotationAngle
{
    /// <summary>
    /// Gets the rotation angle in degrees (normalized to -180 to 180)
    /// </summary>
    public float Degrees { get; }
    
    /// <summary>
    /// Rotation in radians
    /// </summary>
    public float Radians => Degrees * (float)(Math.PI / 180.0);
    
    private RotationAngle(float degrees) => Degrees = degrees;
    
    /// <summary>
    /// Creates a rotation angle from degrees
    /// </summary>
    /// <param name="degrees">Rotation angle in degrees (will be normalized to -180 to 180)</param>
    /// <returns>Success with RotationAngle if valid, otherwise failure</returns>
    /// <remarks>
    /// Angles are automatically normalized to the range -180 to 180 degrees
    /// </remarks>
    public static Result<RotationAngle> Create(float degrees)
    {
        if (float.IsNaN(degrees) || float.IsInfinity(degrees))
        {
            return Result.Failure<RotationAngle>("Rotation angle must be a valid number");
        }

        // Normalize to -180 to 180 range
        var normalized = degrees % 360;
        if (normalized > 180)
        {
            normalized -= 360;
        }
        if (normalized < -180)
        {
            normalized += 360;
        }

        return Result.Success(new RotationAngle(normalized));
    }
    
    /// <summary>
    /// Creates rotation from radians
    /// </summary>
    public static Result<RotationAngle> FromRadians(float radians)
    {
        var degrees = radians * (float)(180.0 / Math.PI);
        return Create(degrees);
    }
    
    /// <summary>
    /// No rotation
    /// </summary>
    public static RotationAngle Zero => new(0f);
    
    /// <summary>
    /// Whether this represents actual rotation
    /// </summary>
    public bool IsSignificant => Math.Abs(Degrees) > 0.1f;
    
    /// <summary>
    /// Gets the opposite rotation
    /// </summary>
    public RotationAngle Inverse => new(-Degrees);
    
    /// <summary>
    /// Combines two rotations
    /// </summary>
    public Result<RotationAngle> Combine(RotationAngle other) => 
        Create(Degrees + other.Degrees);
}