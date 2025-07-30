namespace FaceOFFx.Core.Domain.Common;

/// <summary>
/// Represents a unique identifier for a detected face within the FaceOFFx system.
/// </summary>
/// <remarks>
/// This is an immutable value object that wraps a <see cref="Guid"/> to provide a strongly-typed
/// identifier for faces. It ensures type safety when passing face identifiers throughout the system
/// and prevents accidental misuse of raw GUIDs.
/// </remarks>
/// <example>
/// <code>
/// // Create a new face ID
/// var faceId = FaceId.New();
/// 
/// // Create from existing GUID
/// var guid = Guid.NewGuid();
/// var faceIdFromGuid = FaceId.From(guid);
/// 
/// // Display shortened version
/// Console.WriteLine(faceId); // Output: "a1b2c3d4" (first 8 characters)
/// </code>
/// </example>
public record FaceId
{
    /// <summary>
    /// Gets the underlying GUID value of this face identifier.
    /// </summary>
    /// <value>The GUID that uniquely identifies this face.</value>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FaceId"/> class with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value to use as the face identifier.</param>
    private FaceId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="FaceId"/> with a randomly generated unique identifier.
    /// </summary>
    /// <returns>A new <see cref="FaceId"/> instance with a unique GUID.</returns>
    /// <remarks>
    /// This method is the preferred way to create new face identifiers when detecting
    /// faces for the first time.
    /// </remarks>
    public static FaceId New() => new(Guid.NewGuid());
    
    /// <summary>
    /// Creates a <see cref="FaceId"/> from an existing GUID value.
    /// </summary>
    /// <param name="value">The GUID value to wrap as a face identifier.</param>
    /// <returns>A new <see cref="FaceId"/> instance with the specified GUID.</returns>
    /// <remarks>
    /// Use this method when reconstructing face identifiers from stored values or
    /// when you need to create a face identifier from a known GUID.
    /// </remarks>
    public static FaceId From(Guid value) => new(value);

    /// <summary>
    /// Returns a shortened string representation of the face identifier for display purposes.
    /// </summary>
    /// <returns>
    /// The first 8 characters of the GUID in lowercase hexadecimal format without hyphens.
    /// </returns>
    /// <remarks>
    /// This shortened format is useful for logging and display purposes where the full GUID
    /// would be too verbose. The 8-character format provides sufficient uniqueness for most
    /// display scenarios while remaining human-readable.
    /// </remarks>
    public override string ToString() => Value.ToString("N")[..8];
}