namespace FaceOFFx.Core.Domain.Recognition;

/// <summary>
/// Represents a face embedding vector for facial recognition operations.
/// </summary>
/// <remarks>
/// Face embeddings are high-dimensional vector representations of facial features
/// extracted by deep learning models. These vectors capture the unique characteristics
/// of a face in a way that allows for mathematical comparison between different faces.
/// The embeddings are normalized to unit length to enable cosine similarity comparisons.
/// </remarks>
/// <example>
/// <code>
/// // Create a face embedding from a model output
/// float[] modelOutput = faceRecognitionModel.Forward(faceImage);
/// var embeddingResult = FaceEmbedding.Create(modelOutput);
/// 
/// if (embeddingResult.IsSuccess)
/// {
///     var embedding = embeddingResult.Value;
///     Console.WriteLine($"Created {embedding.Dimensions}D embedding");
/// }
/// </code>
/// </example>
public record FaceEmbedding
{
    private readonly float[] _vector;

    /// <summary>
    /// Gets the normalized embedding vector components.
    /// </summary>
    /// <value>
    /// A read-only list of floating-point values representing the face embedding.
    /// All values are normalized such that the vector has unit length.
    /// </value>
    public IReadOnlyList<float> Vector => _vector;
    
    /// <summary>
    /// Gets the number of dimensions in the embedding vector.
    /// </summary>
    /// <value>
    /// The dimensionality of the embedding (typically 128, 256, or 512).
    /// </value>
    public int Dimensions => _vector.Length;

    private FaceEmbedding(float[] vector)
    {
        _vector = vector;
    }

    /// <summary>
    /// Creates a new face embedding from a raw feature vector.
    /// </summary>
    /// <param name="vector">
    /// The raw feature vector extracted from a face recognition model.
    /// Must be 128, 256, or 512 dimensions.
    /// </param>
    /// <returns>
    /// A Result containing the normalized FaceEmbedding if successful,
    /// or a failure with an error message if validation fails.
    /// </returns>
    /// <remarks>
    /// The input vector is automatically normalized to unit length to ensure
    /// consistent cosine similarity calculations. Only standard embedding
    /// dimensions (128, 256, 512) are accepted as these correspond to
    /// common face recognition model architectures.
    /// </remarks>
    /// <example>
    /// <code>
    /// float[] features = new float[128];
    /// // ... populate features from model ...
    /// 
    /// var result = FaceEmbedding.Create(features);
    /// result.Match(
    ///     success: embedding => Console.WriteLine("Embedding created successfully"),
    ///     failure: error => Console.WriteLine($"Error: {error}")
    /// );
    /// </code>
    /// </example>
    public static Result<FaceEmbedding> Create(float[] vector)
    {
        if (vector == null || vector.Length == 0)
        {
            return Result.Failure<FaceEmbedding>("Embedding vector cannot be null or empty");
        }

        if (vector.Length != 128 && vector.Length != 256 && vector.Length != 512)
        {
            return Result.Failure<FaceEmbedding>($"Embedding vector must be 128, 256, or 512 dimensions, but was {vector.Length}");
        }

        // Normalize the vector
        var magnitude = MathF.Sqrt(vector.Sum(v => v * v));
        if (magnitude < float.Epsilon)
        {
            return Result.Failure<FaceEmbedding>("Cannot create embedding from zero vector");
        }

        var normalized = vector.Select(v => v / magnitude).ToArray();
        return Result.Success(new FaceEmbedding(normalized));
    }

    /// <summary>
    /// Computes the cosine similarity between this embedding and another face embedding.
    /// </summary>
    /// <param name="other">The face embedding to compare against.</param>
    /// <returns>
    /// A value between -1.0 and 1.0 representing the cosine similarity.
    /// Values closer to 1.0 indicate higher similarity (same person),
    /// while values closer to -1.0 indicate dissimilarity.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the embeddings have different dimensions.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Cosine similarity is the preferred metric for face recognition as it measures
    /// the angle between two vectors, making it invariant to vector magnitude.
    /// Since the embeddings are pre-normalized, the cosine similarity is simply
    /// the dot product of the two vectors.
    /// </para>
    /// <para>
    /// Typical similarity thresholds for face verification:
    /// - &gt; 0.99: Identical or near-identical images
    /// - &gt; 0.95: Very high confidence same person
    /// - &gt; 0.85: High confidence same person
    /// - &gt; 0.70: Medium confidence same person
    /// - &gt; 0.50: Low confidence same person
    /// - &lt; 0.50: Different persons
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var embedding1 = FaceEmbedding.Create(face1Features).Value;
    /// var embedding2 = FaceEmbedding.Create(face2Features).Value;
    /// 
    /// float similarity = embedding1.CosineSimilarity(embedding2);
    /// if (similarity > 0.85f)
    /// {
    ///     Console.WriteLine($"Same person with {similarity:P0} confidence");
    /// }
    /// </code>
    /// </example>
    public float CosineSimilarity(FaceEmbedding other)
    {
        if (Dimensions != other.Dimensions)
        {
            throw new ArgumentException($"Cannot compare embeddings of different dimensions: {Dimensions} vs {other.Dimensions}");
        }

        var dotProduct = 0f;
        for (int i = 0; i < Dimensions; i++)
        {
            dotProduct += _vector[i] * other._vector[i];
        }

        // Since vectors are normalized, dot product equals cosine similarity
        return dotProduct;
    }

    /// <summary>
    /// Computes the Euclidean distance between this embedding and another face embedding.
    /// </summary>
    /// <param name="other">The face embedding to compare against.</param>
    /// <returns>
    /// The Euclidean distance between the two embeddings.
    /// Lower values indicate higher similarity.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the embeddings have different dimensions.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Euclidean distance measures the straight-line distance between two points
    /// in the embedding space. While cosine similarity is generally preferred for
    /// face recognition, Euclidean distance can be useful in certain scenarios.
    /// </para>
    /// <para>
    /// For normalized vectors, the Euclidean distance d relates to cosine similarity s as:
    /// d = sqrt(2 - 2s)
    /// </para>
    /// <para>
    /// Typical distance thresholds (for normalized embeddings):
    /// - &lt; 0.2: Very similar faces
    /// - &lt; 0.6: Similar faces
    /// - &lt; 1.0: Possibly same person
    /// - &gt; 1.0: Different persons
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var embedding1 = FaceEmbedding.Create(face1Features).Value;
    /// var embedding2 = FaceEmbedding.Create(face2Features).Value;
    /// 
    /// float distance = embedding1.EuclideanDistance(embedding2);
/// if (distance &lt; 0.6f)
    /// {
    ///     Console.WriteLine($"Faces are similar (distance: {distance:F3})");
    /// }
    /// </code>
    /// </example>
    public float EuclideanDistance(FaceEmbedding other)
    {
        if (Dimensions != other.Dimensions)
        {
            throw new ArgumentException($"Cannot compare embeddings of different dimensions: {Dimensions} vs {other.Dimensions}");
        }

        var sum = 0f;
        for (int i = 0; i < Dimensions; i++)
        {
            var diff = _vector[i] - other._vector[i];
            sum += diff * diff;
        }

        return MathF.Sqrt(sum);
    }

    /// <summary>
    /// Returns a string representation of the face embedding.
    /// </summary>
    /// <returns>
    /// A string indicating the embedding type and dimensionality.
    /// </returns>
    public override string ToString() => $"FaceEmbedding[{Dimensions}D]";
}
