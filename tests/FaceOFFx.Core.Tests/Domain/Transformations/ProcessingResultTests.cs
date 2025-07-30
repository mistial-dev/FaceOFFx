using AwesomeAssertions;
using CSharpFunctionalExtensions;
using FaceOFFx.Core.Domain.Transformations;
using NUnit.Framework;

namespace FaceOFFx.Core.Tests.Domain.Transformations;

/// <summary>
/// Tests for ProcessingResult and ProcessingMetadata records
/// </summary>
[TestFixture]
public class ProcessingResultTests
{
    private byte[] _testImageData = null!;
    private ProcessingMetadata _testMetadata = null!;

    /// <inheritdoc/>
    [SetUp]
    public void SetUp()
    {
        _testImageData = new byte[] { 1, 2, 3, 4, 5 };
        _testMetadata = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            2.5f,
            0.95f,
            15000,
            TimeSpan.FromMilliseconds(1500)
        )
        {
            CompressionRate = 0.7f,
            TargetSize = 20000,
            Warnings = new[] { "Minor rotation applied" },
            AdditionalData = new Dictionary<string, object> { ["test"] = "value" },
        };
    }

    /// <summary>
    /// Tests ProcessingResult constructor with valid data
    /// </summary>
    [Test]
    public void ProcessingResult_Constructor_SetsPropertiesCorrectly()
    {
        var result = new ProcessingResult(_testImageData, _testMetadata);

        result.ImageData.Should().Equal(_testImageData);
        result.Metadata.Should().Be(_testMetadata);
    }

    /// <summary>
    /// Tests ProcessingMetadata constructor with all parameters
    /// </summary>
    [Test]
    public void ProcessingMetadata_Constructor_SetsPropertiesCorrectly()
    {
        var dimensions = new ImageDimensions(800, 600);
        var rotation = 1.5f;
        var confidence = 0.85f;
        var fileSize = 25000;
        var processingTime = TimeSpan.FromSeconds(2);

        var metadata = new ProcessingMetadata(
            dimensions,
            rotation,
            confidence,
            fileSize,
            processingTime
        );

        metadata.OutputDimensions.Should().Be(dimensions);
        metadata.RotationApplied.Should().Be(rotation);
        metadata.FaceConfidence.Should().Be(confidence);
        metadata.FileSize.Should().Be(fileSize);
        metadata.ProcessingTime.Should().Be(processingTime);
    }

    /// <summary>
    /// Tests ProcessingMetadata with optional properties
    /// </summary>
    [Test]
    public void ProcessingMetadata_WithOptionalProperties_SetsCorrectly()
    {
        var metadata = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            0f,
            0.9f,
            20000,
            TimeSpan.FromSeconds(1)
        )
        {
            CompressionRate = 1.2f,
            TargetSize = 25000,
            Warnings = new[] { "Warning 1", "Warning 2" },
            AdditionalData = new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = 42 },
        };

        metadata.CompressionRate.Should().Be(1.2f);
        metadata.TargetSize.HasValue.Should().BeTrue();
        metadata.TargetSize.Value.Should().Be(25000);
        metadata.Warnings.Should().HaveCount(2);
        metadata.Warnings.Should().Contain("Warning 1");
        metadata.Warnings.Should().Contain("Warning 2");
        metadata.AdditionalData.Should().ContainKey("key1");
        metadata.AdditionalData.Should().ContainKey("key2");
        metadata.AdditionalData["key1"].Should().Be("value1");
        metadata.AdditionalData["key2"].Should().Be(42);
    }

    /// <summary>
    /// Tests ProcessingMetadata defaults for optional properties
    /// </summary>
    [Test]
    public void ProcessingMetadata_Defaults_AreSetCorrectly()
    {
        var metadata = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            0f,
            0.8f,
            15000,
            TimeSpan.FromMilliseconds(500)
        );

        metadata.CompressionRate.Should().Be(0f); // Default value
        metadata.TargetSize.HasValue.Should().BeFalse();
        metadata.Warnings.Should().BeEmpty();
        metadata.AdditionalData.Should().BeEmpty();
    }

    /// <summary>
    /// Tests ProcessingMetadata with no target size
    /// </summary>
    [Test]
    public void ProcessingMetadata_WithNoTargetSize_HasCorrectMaybe()
    {
        var metadata = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            0f,
            0.8f,
            15000,
            TimeSpan.FromMilliseconds(500)
        )
        {
            CompressionRate = 2.0f,
            // TargetSize not set, should be None
        };

        metadata.TargetSize.HasValue.Should().BeFalse();
        metadata.TargetSize.HasNoValue.Should().BeTrue();
    }

    /// <summary>
    /// Tests ProcessingMetadata with target size set
    /// </summary>
    [Test]
    public void ProcessingMetadata_WithTargetSize_HasCorrectMaybe()
    {
        var targetSize = 30000;
        var metadata = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            0f,
            0.8f,
            25000,
            TimeSpan.FromSeconds(1)
        )
        {
            TargetSize = targetSize,
        };

        metadata.TargetSize.HasValue.Should().BeTrue();
        metadata.TargetSize.Value.Should().Be(targetSize);
    }

    /// <summary>
    /// Tests ProcessingResult record equality
    /// </summary>
    [Test]
    public void ProcessingResult_Equality_WorksCorrectly()
    {
        var result1 = new ProcessingResult(_testImageData, _testMetadata);
        var result2 = new ProcessingResult(_testImageData, _testMetadata);
        var result3 = new ProcessingResult(new byte[] { 6, 7, 8 }, _testMetadata);

        // Same data should be equal
        result1.Should().Be(result2);
        result1.GetHashCode().Should().Be(result2.GetHashCode());

        // Different data should not be equal
        result1.Should().NotBe(result3);
    }

    /// <summary>
    /// Tests ProcessingMetadata record equality
    /// </summary>
    [Test]
    public void ProcessingMetadata_Equality_WorksCorrectly()
    {
        var metadata1 = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            2.5f,
            0.95f,
            15000,
            TimeSpan.FromSeconds(1)
        )
        {
            CompressionRate = 0.7f,
            TargetSize = Maybe<int>.None,
            Warnings = Array.Empty<string>(),
            AdditionalData = new Dictionary<string, object>()
        };

        var metadata2 = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            2.5f,
            0.95f,
            15000,
            TimeSpan.FromSeconds(1)
        )
        {
            CompressionRate = 0.7f,
            TargetSize = Maybe<int>.None,
            Warnings = Array.Empty<string>(),
            AdditionalData = new Dictionary<string, object>()
        };

        var metadata3 = metadata1 with { CompressionRate = 1.0f };

        // Same values should be structurally equal
        metadata1.Should().BeEquivalentTo(metadata2);

        // For true equality, we need to use the same collection instances
        var sharedWarnings = Array.Empty<string>();
        var sharedAdditionalData = new Dictionary<string, object>();

        var metadata1a = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            2.5f,
            0.95f,
            15000,
            TimeSpan.FromSeconds(1)
        )
        {
            CompressionRate = 0.7f,
            TargetSize = Maybe<int>.None,
            Warnings = sharedWarnings,
            AdditionalData = sharedAdditionalData
        };

        var metadata2a = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            2.5f,
            0.95f,
            15000,
            TimeSpan.FromSeconds(1)
        )
        {
            CompressionRate = 0.7f,
            TargetSize = Maybe<int>.None,
            Warnings = sharedWarnings,
            AdditionalData = sharedAdditionalData
        };

        // With same collection instances, they should be equal
        metadata1a.Should().Be(metadata2a);
        metadata1a.GetHashCode().Should().Be(metadata2a.GetHashCode());

        // Different values should not be equal
        metadata1.Should().NotBe(metadata3);
    }

    /// <summary>
    /// Tests ProcessingMetadata with expression creates new instance
    /// </summary>
    [Test]
    public void ProcessingMetadata_WithExpression_CreatesNewInstance()
    {
        var originalMetadata = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            0f,
            0.8f,
            15000,
            TimeSpan.FromSeconds(1)
        )
        {
            CompressionRate = 0.7f,
            Warnings = new[] { "Original warning" },
        };

        var modifiedMetadata = originalMetadata with
        {
            CompressionRate = 1.5f,
            Warnings = new[] { "New warning" },
        };

        // Original should be unchanged
        originalMetadata.CompressionRate.Should().Be(0.7f);
        originalMetadata.Warnings.Should().Contain("Original warning");

        // Modified should have new values
        modifiedMetadata.CompressionRate.Should().Be(1.5f);
        modifiedMetadata.Warnings.Should().Contain("New warning");
        modifiedMetadata.Warnings.Should().NotContain("Original warning");

        // Other properties should be preserved
        modifiedMetadata.OutputDimensions.Should().Be(originalMetadata.OutputDimensions);
        modifiedMetadata.FaceConfidence.Should().Be(originalMetadata.FaceConfidence);
    }

    /// <summary>
    /// Tests that ProcessingMetadata handles empty collections correctly
    /// </summary>
    [Test]
    public void ProcessingMetadata_WithEmptyCollections_HandlesCorrectly()
    {
        var metadata = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            0f,
            0.8f,
            15000,
            TimeSpan.FromSeconds(1)
        )
        {
            Warnings = Array.Empty<string>(),
            AdditionalData = new Dictionary<string, object>(),
        };

        metadata.Warnings.Should().BeEmpty();
        metadata.AdditionalData.Should().BeEmpty();
        metadata.Warnings.Should().NotBeNull();
        metadata.AdditionalData.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ProcessingMetadata collections are read-only
    /// </summary>
    [Test]
    public void ProcessingMetadata_Collections_AreReadOnly()
    {
        var warnings = new List<string> { "Warning 1" };
        var additionalData = new Dictionary<string, object> { ["key"] = "value" };

        var metadata = new ProcessingMetadata(
            new ImageDimensions(420, 560),
            0f,
            0.8f,
            15000,
            TimeSpan.FromSeconds(1)
        )
        {
            Warnings = warnings,
            AdditionalData = additionalData,
        };

        // Should be read-only interfaces
        metadata.Warnings.Should().BeAssignableTo<IReadOnlyList<string>>();
        metadata.AdditionalData.Should().BeAssignableTo<IReadOnlyDictionary<string, object>>();
    }
}
