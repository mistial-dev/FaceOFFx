using FaceOFFx.Core.Domain.Common;

namespace FaceOFFx.Core.Domain.Recognition;

/// <summary>
/// Represents the result of comparing two faces for recognition purposes.
/// </summary>
/// <remarks>
/// FaceMatch encapsulates the comparison result between a query face and a candidate face,
/// including the similarity score and a categorized match type based on confidence thresholds.
/// This record is immutable and provides a clear API for determining if two faces belong
/// to the same person.
/// </remarks>
/// <example>
/// <code>
/// // Create a face match result
/// var match = FaceMatch.Create(queryFaceId, candidateFaceId, 0.87f);
///
/// if (match.IsHighConfidenceMatch)
/// {
///     Console.WriteLine($"Found match with {match.Similarity:P0} confidence");
///     Console.WriteLine($"Match type: {match.Type}");
/// }
/// </code>
/// </example>
public record FaceMatch
{
    /// <summary>
    /// Gets the identifier of the query face being searched for.
    /// </summary>
    /// <value>
    /// The unique identifier of the face that initiated the matching request.
    /// </value>
    public FaceId QueryFaceId { get; }

    /// <summary>
    /// Gets the identifier of the matched candidate face.
    /// </summary>
    /// <value>
    /// The unique identifier of the face that was compared against the query.
    /// </value>
    public FaceId MatchedFaceId { get; }

    /// <summary>
    /// Gets the similarity score between the two faces.
    /// </summary>
    /// <value>
    /// A value typically between 0.0 and 1.0, where higher values indicate
    /// greater similarity. For cosine similarity, 1.0 represents identical embeddings.
    /// </value>
    public float Similarity { get; }

    /// <summary>
    /// Gets the categorized match type based on the similarity score.
    /// </summary>
    /// <value>
    /// A MatchType enum value indicating the confidence level of the match.
    /// </value>
    public MatchType Type { get; }

    private FaceMatch(FaceId queryFaceId, FaceId matchedFaceId, float similarity, MatchType type)
    {
        QueryFaceId = queryFaceId;
        MatchedFaceId = matchedFaceId;
        Similarity = similarity;
        Type = type;
    }

    /// <summary>
    /// Creates a new face match result with automatic match type categorization.
    /// </summary>
    /// <param name="queryFaceId">The identifier of the query face.</param>
    /// <param name="matchedFaceId">The identifier of the candidate face being compared.</param>
    /// <param name="similarity">
    /// The similarity score between the faces, typically from cosine similarity (0.0 to 1.0).
    /// </param>
    /// <returns>
    /// A new FaceMatch instance with the match type automatically determined based on
    /// standard similarity thresholds.
    /// </returns>
    /// <remarks>
    /// The match type is automatically categorized based on these thresholds:
    /// - Identical: similarity ≥ 0.99 (same or nearly identical image)
    /// - VeryHigh: similarity ≥ 0.95 (very high confidence same person)
    /// - High: similarity ≥ 0.85 (high confidence same person)
    /// - Medium: similarity ≥ 0.70 (medium confidence same person)
    /// - Low: similarity ≥ 0.50 (low confidence, possibly same person)
    /// - NoMatch: similarity &lt; 0.50 (different persons)
    /// </remarks>
    /// <example>
    /// <code>
    /// var queryId = FaceId.Generate();
    /// var candidateId = FaceId.Generate();
    /// float similarity = queryEmbedding.CosineSimilarity(candidateEmbedding);
    ///
    /// var match = FaceMatch.Create(queryId, candidateId, similarity);
    /// Console.WriteLine($"Match confidence: {match.Type}");
    /// </code>
    /// </example>
    public static FaceMatch Create(FaceId queryFaceId, FaceId matchedFaceId, float similarity)
    {
        var type = similarity switch
        {
            >= 0.99f => MatchType.Identical,
            >= 0.95f => MatchType.VeryHigh,
            >= 0.85f => MatchType.High,
            >= 0.70f => MatchType.Medium,
            >= 0.50f => MatchType.Low,
            _ => MatchType.NoMatch,
        };

        return new FaceMatch(queryFaceId, matchedFaceId, similarity, type);
    }

    /// <summary>
    /// Gets a value indicating whether this result represents a match.
    /// </summary>
    /// <value>
    /// <c>true</c> if the match type is anything other than NoMatch; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property provides a simple boolean check for any level of match,
    /// including low confidence matches. For production systems, consider using
    /// IsHighConfidenceMatch for more reliable results.
    /// </remarks>
    public bool IsMatch => Type != MatchType.NoMatch;

    /// <summary>
    /// Gets a value indicating whether this is a high confidence match suitable for secure applications.
    /// </summary>
    /// <value>
    /// <c>true</c> if the match type is VeryHigh or Identical; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// High confidence matches have similarity scores of 0.95 or higher, making them
    /// suitable for security-critical applications like access control or identity verification.
    /// </remarks>
    public bool IsHighConfidenceMatch => Type is MatchType.VeryHigh or MatchType.Identical;
}

/// <summary>
/// Categorizes face match confidence levels based on similarity scores.
/// </summary>
/// <remarks>
/// These categories help applications make decisions based on the required
/// security level and use case. Higher match types indicate greater confidence
/// that two faces belong to the same person.
/// </remarks>
public enum MatchType
{
    /// <summary>
    /// No match detected (similarity &lt; 0.50).
    /// The faces belong to different persons.
    /// </summary>
    NoMatch,

    /// <summary>
    /// Low confidence match (0.50 ≤ similarity &lt; 0.70).
    /// Possibly the same person, but additional verification recommended.
    /// </summary>
    Low,

    /// <summary>
    /// Medium confidence match (0.70 ≤ similarity &lt; 0.85).
    /// Likely the same person, suitable for non-critical applications.
    /// </summary>
    Medium,

    /// <summary>
    /// High confidence match (0.85 ≤ similarity &lt; 0.95).
    /// Same person with high confidence, suitable for most applications.
    /// </summary>
    High,

    /// <summary>
    /// Very high confidence match (0.95 ≤ similarity &lt; 0.99).
    /// Same person with very high confidence, suitable for secure applications.
    /// </summary>
    VeryHigh,

    /// <summary>
    /// Identical or near-identical match (similarity ≥ 0.99).
    /// Same image or extremely similar images of the same person.
    /// </summary>
    Identical,
}

/// <summary>
/// Configuration options for face matching operations.
/// </summary>
/// <remarks>
/// FaceMatchingOptions allows customization of the matching process including
/// similarity thresholds, distance metrics, and result limits. Pre-configured
/// options are available for common security scenarios.
/// </remarks>
/// <example>
/// <code>
/// // Use high security settings for access control
/// var options = FaceMatchingOptions.HighSecurity;
/// var matches = faceRecognizer.FindMatches(queryFace, candidates, options);
///
/// // Custom configuration
/// var customOptions = new FaceMatchingOptions
/// {
///     MinimumSimilarity = 0.8f,
///     Metric = DistanceMetric.CosineSimilarity,
///     MaxResults = 5
/// };
/// </code>
/// </example>
public record FaceMatchingOptions
{
    /// <summary>
    /// Gets or initializes the minimum similarity score to consider a match.
    /// </summary>
    /// <value>
    /// A value between 0.0 and 1.0. Default is 0.7 (medium confidence).
    /// </value>
    /// <remarks>
    /// This threshold filters out matches below the specified similarity score.
    /// Adjust based on your security requirements and false positive tolerance.
    /// </remarks>
    public float MinimumSimilarity { get; init; } = 0.7f;

    /// <summary>
    /// Gets or initializes the distance metric to use for face comparison.
    /// </summary>
    /// <value>
    /// The metric for computing similarity. Default is CosineSimilarity.
    /// </value>
    /// <remarks>
    /// Cosine similarity is recommended for most face recognition scenarios
    /// as it's invariant to embedding magnitude and well-suited for high-dimensional spaces.
    /// </remarks>
    public DistanceMetric Metric { get; init; } = DistanceMetric.CosineSimilarity;

    /// <summary>
    /// Gets or initializes the maximum number of matches to return.
    /// </summary>
    /// <value>
    /// The maximum number of results. Default is 10.
    /// </value>
    /// <remarks>
    /// Limits the number of returned matches, sorted by similarity score in descending order.
    /// Set to 1 for face verification (1:1) scenarios.
    /// </remarks>
    public int MaxResults { get; init; } = 10;

    /// <summary>
    /// Gets default matching options suitable for general use.
    /// </summary>
    /// <value>
    /// Options with 0.7 minimum similarity, cosine similarity metric, and 10 max results.
    /// </value>
    public static FaceMatchingOptions Default => new();

    /// <summary>
    /// Gets high security matching options for access control and verification.
    /// </summary>
    /// <value>
    /// Options with 0.85 minimum similarity for reduced false positives.
    /// </value>
    /// <remarks>
    /// Use these settings when false positives must be minimized, such as
    /// building access control or financial transaction verification.
    /// </remarks>
    public static FaceMatchingOptions HighSecurity => new() { MinimumSimilarity = 0.85f };

    /// <summary>
    /// Gets low security matching options for social or convenience applications.
    /// </summary>
    /// <value>
    /// Options with 0.5 minimum similarity for increased match recall.
    /// </value>
    /// <remarks>
    /// Use these settings when false negatives are more problematic than false positives,
    /// such as photo tagging or social media friend suggestions.
    /// </remarks>
    public static FaceMatchingOptions LowSecurity => new() { MinimumSimilarity = 0.5f };
}

/// <summary>
/// Specifies the distance metric for comparing face embeddings.
/// </summary>
public enum DistanceMetric
{
    /// <summary>
    /// Cosine similarity metric (recommended).
    /// Measures the cosine of the angle between two vectors.
    /// </summary>
    /// <remarks>
    /// Values range from -1 to 1, where 1 indicates identical directions.
    /// Invariant to vector magnitude, making it ideal for face recognition.
    /// </remarks>
    CosineSimilarity,

    /// <summary>
    /// Euclidean distance metric.
    /// Measures the straight-line distance between two points.
    /// </summary>
    /// <remarks>
    /// Lower values indicate higher similarity. For normalized vectors,
    /// relates to cosine similarity as: d = sqrt(2 - 2 * cos_sim).
    /// </remarks>
    EuclideanDistance,
}
