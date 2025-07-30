using JetBrains.Annotations;

namespace FaceOFFx.Models;

/// <summary>
/// Registry of ONNX models used in PIV processing pipeline.
/// </summary>
public static class ModelRegistry
{
    /// <summary>
    /// RetinaFace model for face detection.
    /// </summary>
    public const string FaceDetector = "FaceOFFx.Models.Resources.FaceDetector.onnx";

    /// <summary>
    /// PFLD model for 68-point facial landmark detection.
    /// </summary>
    public const string FaceLandmarks68 = "FaceOFFx.Models.Resources.landmarks_68_pfld.onnx";

    /// <summary>
    /// Gets a model as a byte array from embedded resources
    /// </summary>
    /// [PublicAPI]
    public static byte[] GetModel(string modelName)
    {
        var assembly = typeof(ModelRegistry).Assembly;
        using var stream =
            assembly.GetManifestResourceStream(modelName)
            ?? throw new InvalidOperationException(
                $"Model {modelName} not found in embedded resources"
            );

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Checks if a model exists in the registry
    /// </summary>
    [PublicAPI]
    public static bool ModelExists(string modelName)
    {
        var assembly = typeof(ModelRegistry).Assembly;
        return assembly.GetManifestResourceNames().Contains(modelName);
    }
}
