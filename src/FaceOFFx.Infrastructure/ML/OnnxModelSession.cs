namespace FaceOFFx.Infrastructure.ML;

/// <summary>
/// Wrapper for ONNX inference sessions with resource management
/// </summary>
public sealed class OnnxModelSession : IDisposable
{
    private readonly InferenceSession _session;
    private readonly string _modelName;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnnxModelSession"/> class.
    /// </summary>
    /// <param name="modelData">The ONNX model data as a byte array.</param>
    /// <param name="modelName">The name of the model for identification.</param>
    /// <param name="options">Optional session configuration options.</param>
    public OnnxModelSession(byte[] modelData, string modelName, SessionOptions? options = null)
    {
        _modelName = modelName;
        _session =
            options != null
                ? new InferenceSession(modelData, options)
                : new InferenceSession(modelData);
    }

    /// <summary>
    /// Gets the underlying ONNX inference session.
    /// </summary>
    public InferenceSession Session => _session;

    /// <summary>
    /// Gets the name of the loaded model.
    /// </summary>
    public string ModelName => _modelName;

    /// <summary>
    /// Gets the names of the model's input tensors.
    /// </summary>
    public IReadOnlyList<string> InputNames => _session.InputNames;

    /// <summary>
    /// Gets the names of the model's output tensors.
    /// </summary>
    public IReadOnlyList<string> OutputNames => _session.OutputNames;

    /// <summary>
    /// Disposes the ONNX inference session and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _session?.Dispose();
        _disposed = true;
    }
}
