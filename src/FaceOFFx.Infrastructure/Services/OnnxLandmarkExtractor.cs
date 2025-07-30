using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FaceOFFx.Infrastructure.Services;

/// <summary>
/// ONNX-based 68-point facial landmark extractor using PFLD model
/// </summary>
[PublicAPI]
public sealed class OnnxLandmarkExtractor : ILandmarkExtractor, IDisposable
{
    private readonly ILogger<OnnxLandmarkExtractor> _logger;
    private readonly InferenceSession _session;
    private readonly string _inputName;
    private const int ModelInputSize = 112;
    private const int NumLandmarks = 68;
    private bool _disposed;

    /// <summary>
    /// 68-point facial landmark extraction using Onnx
    /// </summary>
    /// <param name="logger"></param>
    public OnnxLandmarkExtractor(ILogger<OnnxLandmarkExtractor> logger)
    {
        _logger = logger;

        try
        {
            var sessionOptions = new SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            };

            // Load model from embedded resources
            var modelBytes = Models.ModelRegistry.GetModel(Models.ModelRegistry.FaceLandmarks68);
            _session = new InferenceSession(modelBytes, sessionOptions);

            // Get input/output names from the model
            _inputName = _session.InputMetadata.Keys.First();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize OnnxLandmarkExtractor");
            throw;
        }
    }

    /// <summary>
    /// Extracts 68 facial landmark points from a face region
    /// </summary>
    public async Task<Result<FaceLandmarks68>> ExtractLandmarksAsync(
        Image<Rgba32> image,
        FaceBox faceBox,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Step 1: Crop face region with some padding
            var paddingRatio = 0.1f; // 10% padding
            var padding = (int)(Math.Max(faceBox.Width, faceBox.Height) * paddingRatio);

            var cropX = Math.Max(0, (int)faceBox.X - padding);
            var cropY = Math.Max(0, (int)faceBox.Y - padding);
            var cropWidth = Math.Min(image.Width - cropX, (int)faceBox.Width + 2 * padding);
            var cropHeight = Math.Min(image.Height - cropY, (int)faceBox.Height + 2 * padding);

            using var faceImage = image.Clone(ctx =>
                ctx.Crop(new Rectangle(cropX, cropY, cropWidth, cropHeight))
            );

            // Step 2: Resize to model input size (112x112) with padding to preserve aspect ratio
            faceImage.Mutate(static ctx =>
                ctx.Resize(
                    new ResizeOptions
                    {
                        Size = new Size(ModelInputSize, ModelInputSize),
                        Mode = ResizeMode.Pad,
                        PadColor = Color.Gray,
                    }
                )
            );

            // Step 3: Convert to tensor and normalize to [0, 1]
            var tensor = ImageToTensor(faceImage);

            // Step 4: Run inference
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(_inputName, tensor),
            };

            using var results = await Task.Run(() => _session.Run(inputs), cancellationToken)
                .ConfigureAwait(false);
            var output = results.First().AsEnumerable<float>().ToArray();

            // Step 5: Convert output to landmarks
            if (output.Length != NumLandmarks * 2)
            {
                return Result.Failure<FaceLandmarks68>(
                    $"Invalid model output: expected {NumLandmarks * 2} values, got {output.Length}"
                );
            }

            // Calculate padding used when resizing with Mode=Pad
            var aspectRatio = (float)cropWidth / cropHeight;
            var targetAspectRatio = 1.0f; // 112x112 is square
            float padX = 0,
                padY = 0;
            float scaleFactor;

            if (aspectRatio > targetAspectRatio)
            {
                // Image is wider - padding on top/bottom
                scaleFactor = (float)ModelInputSize / cropWidth;
                var scaledHeight = cropHeight * scaleFactor;
                padY = (ModelInputSize - scaledHeight) / 2f;
            }
            else
            {
                // Image is taller - padding on left/right
                scaleFactor = (float)ModelInputSize / cropHeight;
                var scaledWidth = cropWidth * scaleFactor;
                padX = (ModelInputSize - scaledWidth) / 2f;
            }

            var landmarks = new List<Point2D>();
            for (var i = 0; i < NumLandmarks; i++)
            {
                // Model outputs normalized coordinates [0, 1] in 112x112 space
                var modelX = output[i * 2] * ModelInputSize;
                var modelY = output[i * 2 + 1] * ModelInputSize;

                // Remove padding offset
                var unpaddedX = modelX - padX;
                var unpaddedY = modelY - padY;

                // Scale back to crop region coordinates
                var cropRegionX = unpaddedX / scaleFactor;
                var cropRegionY = unpaddedY / scaleFactor;

                // Convert to original image coordinates
                var imageX = cropX + cropRegionX;
                var imageY = cropY + cropRegionY;

                landmarks.Add(new Point2D(imageX, imageY));
            }

            _logger.LogDebug("Successfully extracted {Count} landmarks", landmarks.Count);
            return Result.Success(new FaceLandmarks68(landmarks));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Landmark extraction failed");
            return Result.Failure<FaceLandmarks68>($"Landmark extraction failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Converts an image to a tensor normalized to [0, 1]
    /// </summary>
    private DenseTensor<float> ImageToTensor(Image<Rgba32> image)
    {
        var tensor = new DenseTensor<float>([1, 3, ModelInputSize, ModelInputSize]);

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < ModelInputSize; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < ModelInputSize; x++)
                {
                    var pixel = row[x];

                    // Normalize to [0, 1] and set in CHW format
                    tensor[0, 0, y, x] = pixel.R / 255f; // Red channel
                    tensor[0, 1, y, x] = pixel.G / 255f; // Green channel
                    tensor[0, 2, y, x] = pixel.B / 255f; // Blue channel
                }
            }
        });

        return tensor;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _session.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disposal");
        }
        finally
        {
            _disposed = true;
        }
    }
}
