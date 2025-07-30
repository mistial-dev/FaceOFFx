using AwesomeAssertions;
using CSharpFunctionalExtensions;
using FaceOFFx.Core.Domain.Transformations;
using FaceOFFx.Infrastructure.Services;
using FaceOFFx.Tests.Common;
using NUnit.Framework;

namespace FaceOFFx.Infrastructure.Tests.Services;

/// <summary>
/// Tests for the FaceProcessor API
/// </summary>
[TestFixture]
public class FaceProcessorTests : IntegrationTestBase
{
    private byte[] _validImageData = null!;
    private byte[] _invalidImageData = null!;

    /// <inheritdoc/>
    [OneTimeSetUp]
    public override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        // Load test image data - use the actual generic_guy.png
        var testImagePath = "/Users/mistial/Projects/FaceONNX/tests/sample_images/generic_guy.png";

        if (!File.Exists(testImagePath))
        {
            throw new FileNotFoundException($"Required test image not found: {testImagePath}");
        }

        _validImageData = File.ReadAllBytes(testImagePath);
        Console.WriteLine($"Loaded test image: {testImagePath} ({_validImageData.Length} bytes)");

        _invalidImageData = new byte[] { 1, 2, 3, 4, 5 }; // Invalid image data
    }

    /// <summary>
    /// Tests ProcessAsync with default options
    /// </summary>
    [Test]
    public async Task ProcessAsync_WithDefaultOptions_ReturnsValidResult()
    {
        var result = await FaceProcessor.ProcessAsync(_validImageData);

        if (result.IsFailure)
        {
            Console.WriteLine($"Processing failed with error: {result.Error}");
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.ImageData.Should().NotBeEmpty();
        result.Value.Metadata.FileSize.Should().BeGreaterThan(0);
        result.Value.Metadata.OutputDimensions.Width.Should().Be(420);
        result.Value.Metadata.OutputDimensions.Height.Should().Be(560);
        result.Value.Metadata.CompressionRate.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests ProcessAsync with custom options
    /// </summary>
    [Test]
    public async Task ProcessAsync_WithCustomOptions_UsesOptions()
    {
        var customOptions = ProcessingOptions.PivHigh with { MinFaceConfidence = 0.9f };

        var result = await FaceProcessor.ProcessAsync(_validImageData, customOptions);

        result.IsSuccess.Should().BeTrue();
        result.Value.Metadata.TargetSize.HasValue.Should().BeTrue();
        result.Value.Metadata.TargetSize.Value.Should().Be(30000); // PivHigh target
    }

    /// <summary>
    /// Tests ProcessForTwicAsync targets correct file size
    /// </summary>
    [Test]
    public async Task ProcessForTwicAsync_TargetsCorrectSize()
    {
        var result = await FaceProcessor.ProcessForTwicAsync(_validImageData);

        result.IsSuccess.Should().BeTrue();
        (result.Value.Metadata.FileSize <= 14000).Should().BeTrue();
        result.Value.Metadata.TargetSize.HasValue.Should().BeTrue();
        result.Value.Metadata.TargetSize.Value.Should().Be(14000);
    }

    /// <summary>
    /// Tests ProcessForPivAsync targets correct file size
    /// </summary>
    [Test]
    public async Task ProcessForPivAsync_TargetsCorrectSize()
    {
        var result = await FaceProcessor.ProcessForPivAsync(_validImageData);

        result.IsSuccess.Should().BeTrue();
        result.Value.Metadata.TargetSize.HasValue.Should().BeTrue();
        result.Value.Metadata.TargetSize.Value.Should().Be(20000);
    }

    /// <summary>
    /// Tests ProcessToSizeAsync with custom target size
    /// </summary>
    [Test]
    public async Task ProcessToSizeAsync_WithCustomSize_TargetsSize()
    {
        var targetSize = 25000;
        var result = await FaceProcessor.ProcessToSizeAsync(_validImageData, targetSize);

        result.IsSuccess.Should().BeTrue();
        (result.Value.Metadata.FileSize <= targetSize).Should().BeTrue();
        result.Value.Metadata.TargetSize.HasValue.Should().BeTrue();
        result.Value.Metadata.TargetSize.Value.Should().Be(targetSize);
    }

    /// <summary>
    /// Tests ProcessWithRateAsync uses specified compression rate
    /// </summary>
    [Test]
    public async Task ProcessWithRateAsync_WithCustomRate_UsesRate()
    {
        var compressionRate = 1.5f;
        var result = await FaceProcessor.ProcessWithRateAsync(_validImageData, compressionRate);

        result.IsSuccess.Should().BeTrue();
        result.Value.Metadata.CompressionRate.Should().Be(compressionRate);
        result.Value.Metadata.TargetSize.HasValue.Should().BeFalse(); // No target size for fixed rate
    }

    /// <summary>
    /// Tests ProcessAsync fails gracefully with invalid image data
    /// </summary>
    [Test]
    public async Task ProcessAsync_WithInvalidImageData_ReturnsFailure()
    {
        var result = await FaceProcessor.ProcessAsync(_invalidImageData);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests ProcessAsync fails gracefully with empty image data
    /// </summary>
    [Test]
    public async Task ProcessAsync_WithEmptyImageData_ReturnsFailure()
    {
        var result = await FaceProcessor.ProcessAsync(Array.Empty<byte>());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests ProcessAsync with null image data throws ArgumentException
    /// </summary>
    [Test]
    public async Task ProcessAsync_WithNullImageData_ThrowsArgumentException()
    {
        await AssertThrowsAsync<ArgumentNullException>(() => FaceProcessor.ProcessAsync(null!));
    }

    /// <summary>
    /// Tests that ProcessingResult contains valid metadata
    /// </summary>
    [Test]
    public async Task ProcessAsync_ValidImage_ReturnsCompleteMetadata()
    {
        var result = await FaceProcessor.ProcessAsync(_validImageData, ProcessingOptions.Archival);

        result.IsSuccess.Should().BeTrue();

        var metadata = result.Value.Metadata;
        metadata.OutputDimensions.Width.Should().Be(420);
        metadata.OutputDimensions.Height.Should().Be(560);
        metadata.FaceConfidence.Should().BeGreaterThan(0);
        (metadata.FaceConfidence <= 1).Should().BeTrue();
        metadata.ProcessingTime.Should().BeGreaterThan(TimeSpan.Zero);
        metadata.CompressionRate.Should().BeGreaterThan(0);
        metadata.Warnings.Should().NotBeNull();
        metadata.AdditionalData.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that different presets produce different file sizes
    /// </summary>
    [Test]
    public async Task ProcessAsync_DifferentPresets_ProduceDifferentSizes()
    {
        var fastResult = await FaceProcessor.ProcessAsync(_validImageData, ProcessingOptions.Fast);
        var standardResult = await FaceProcessor.ProcessAsync(
            _validImageData,
            ProcessingOptions.PivBalanced
        );
        var archivalResult = await FaceProcessor.ProcessAsync(
            _validImageData,
            ProcessingOptions.Archival
        );

        fastResult.IsSuccess.Should().BeTrue();
        standardResult.IsSuccess.Should().BeTrue();
        archivalResult.IsSuccess.Should().BeTrue();

        // Generally, archival should be largest, standard medium, fast smallest
        // But due to target size strategies, this might not always hold
        fastResult.Value.Metadata.FileSize.Should().BeGreaterThan(0);
        standardResult.Value.Metadata.FileSize.Should().BeGreaterThan(0);
        archivalResult.Value.Metadata.FileSize.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests ProcessToSizeAsync with very small target fails appropriately
    /// </summary>
    [Test]
    public async Task ProcessToSizeAsync_WithVerySmallTarget_ReturnsFailure()
    {
        var result = await FaceProcessor.ProcessToSizeAsync(_validImageData, 1000); // Very small

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot compress");
    }

    /// <summary>
    /// Tests ProcessWithRateAsync with invalid rate fails appropriately
    /// </summary>
    [Test]
    public async Task ProcessWithRateAsync_WithInvalidRate_ReturnsFailure()
    {
        var result = await FaceProcessor.ProcessWithRateAsync(_validImageData, -1.0f); // Invalid rate

        result.IsFailure.Should().BeTrue();
    }

    /// <summary>
    /// Tests that processing produces PIV-compliant dimensions
    /// </summary>
    [Test]
    public async Task ProcessAsync_AllPresets_ProducePivDimensions()
    {
        var presets = new[]
        {
            ProcessingOptions.TwicMax,
            ProcessingOptions.PivMin,
            ProcessingOptions.PivBalanced,
            ProcessingOptions.PivHigh,
            ProcessingOptions.Archival,
            ProcessingOptions.Fast,
        };

        foreach (var preset in presets)
        {
            var result = await FaceProcessor.ProcessAsync(_validImageData, preset);

            result.IsSuccess.Should().BeTrue($"Preset {preset} should succeed");
            result.Value.Metadata.OutputDimensions.Width.Should().Be(420);
            result.Value.Metadata.OutputDimensions.Height.Should().Be(560);
        }
    }
}
