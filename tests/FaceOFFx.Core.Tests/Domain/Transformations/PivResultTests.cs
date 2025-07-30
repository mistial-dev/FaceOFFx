using FaceOFFx.Core.Domain.Common;
using FaceOFFx.Core.Domain.Detection;
using FaceOFFx.Core.Domain.Transformations;
using AwesomeAssertions;
using CSharpFunctionalExtensions;
using NUnit.Framework;

namespace FaceOFFx.Core.Tests.Domain.Transformations;

[TestFixture]
public class PivResultTests
{
    private readonly byte[] _sampleImageData = new byte[] { 1, 2, 3, 4, 5 };
    private readonly DetectedFace _sampleFace;
    private readonly PivTransform _sampleTransform;

    public PivResultTests()
    {
        var faceBox = FaceBox.Create(100, 100, 200, 200).Value;
        _sampleFace = new DetectedFace(faceBox, 0.95f, Maybe<FaceLandmarks5>.None);
        
        _sampleTransform = new PivTransform
        {
            RotationDegrees = 2.5f,
            CropRegion = new CropRect { Left = 0.1f, Top = 0.1f, Width = 0.6f, Height = 0.7f },
            ScaleFactor = 1.2f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = true
        };
    }

    [Test]
    public void Success_WithAllParameters_ShouldCreateValidResult()
    {
        var metadata = new Dictionary<string, object> { ["ProcessingTime"] = "150ms" };
        var warnings = new List<string> { "Minor rotation applied" };
        
        var result = PivResult.Success(
            _sampleImageData,
            "image/jpeg",
            new ImageDimensions(420, 560),
            _sampleTransform,
            _sampleFace,
            "Custom summary",
            warnings,
            metadata);
        
        result.ImageData.Should().Equal(_sampleImageData);
        result.MimeType.Should().Be("image/jpeg");
        result.Dimensions.Width.Should().Be(420);
        result.Dimensions.Height.Should().Be(560);
        result.AppliedTransform.Should().Be(_sampleTransform);
        result.SourceFace.Should().Be(_sampleFace);
        result.ProcessingSummary.Should().Be("Custom summary");
        result.Warnings.Should().Contain("Minor rotation applied");
        result.Metadata.Should().ContainKey("ProcessingTime");
        result.IsPivCompliant.Should().BeTrue();
    }

    [Test]
    public void Success_WithMinimalParameters_ShouldUseDefaults()
    {
        var result = PivResult.Success(
            _sampleImageData,
            "image/jpeg",
            new ImageDimensions(420, 560),
            _sampleTransform,
            _sampleFace);
        
        result.ProcessingSummary.Should().NotBeEmpty();
        result.ProcessingSummary.Should().Contain("rotated 2.5°");
        result.ProcessingSummary.Should().Contain("upscaled 1.20x");
        result.ProcessingSummary.Should().Contain("resized to 420x560");
        result.Warnings.Should().BeEmpty();
        result.Metadata.Should().BeEmpty();
    }

    [Test]
    public void GenerateDefaultSummary_WithNoTransformations_ShouldIndicateCompliance()
    {
        var noOpTransform = new PivTransform
        {
            RotationDegrees = 0f,
            CropRegion = CropRect.Full,
            ScaleFactor = 1.0f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = true
        };
        
        var result = PivResult.Success(
            _sampleImageData,
            "image/jpeg",
            new ImageDimensions(420, 560),
            noOpTransform,
            _sampleFace);
        
        result.ProcessingSummary.Should().Contain("resized to 420x560");
    }

    [Test]
    public void GenerateDefaultSummary_WithDownscale_ShouldIndicateCorrectly()
    {
        var transform = new PivTransform
        {
            RotationDegrees = 0f,
            CropRegion = CropRect.Full,
            ScaleFactor = 0.75f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = true
        };
        
        var result = PivResult.Success(
            _sampleImageData,
            "image/jpeg",
            new ImageDimensions(420, 560),
            transform,
            _sampleFace);
        
        result.ProcessingSummary.Should().Contain("downscaled 0.75x");
    }

    [Test]
    public void GenerateDefaultSummary_WithSignificantCrop_ShouldIndicate()
    {
        var transform = new PivTransform
        {
            RotationDegrees = 0f,
            CropRegion = new CropRect { Left = 0, Top = 0, Width = 0.5f, Height = 0.5f },
            ScaleFactor = 1.0f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = true
        };
        
        var result = PivResult.Success(
            _sampleImageData,
            "image/jpeg",
            new ImageDimensions(420, 560),
            transform,
            _sampleFace);
        
        result.ProcessingSummary.Should().Contain("cropped to");
    }

    [Test]
    public void Validate_WithValidResult_ShouldReturnSuccess()
    {
        var result = PivResult.Success(
            _sampleImageData,
            "image/jpeg",
            new ImageDimensions(420, 560),
            _sampleTransform,
            _sampleFace);
        
        var validation = result.Validate();
        
        validation.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void Validate_WithNullImageData_ShouldReturnFailure()
    {
        var result = new PivResult
        {
            ImageData = null!,
            MimeType = "image/jpeg",
            Dimensions = new ImageDimensions(420, 560),
            AppliedTransform = _sampleTransform,
            SourceFace = _sampleFace
        };
        
        var validation = result.Validate();
        
        validation.IsFailure.Should().BeTrue();
        validation.Error.Should().Contain("image data is null or empty");
    }

    [Test]
    public void Validate_WithEmptyImageData_ShouldReturnFailure()
    {
        var result = new PivResult
        {
            ImageData = Array.Empty<byte>(),
            MimeType = "image/jpeg",
            Dimensions = new ImageDimensions(420, 560),
            AppliedTransform = _sampleTransform,
            SourceFace = _sampleFace
        };
        
        var validation = result.Validate();
        
        validation.IsFailure.Should().BeTrue();
        validation.Error.Should().Contain("image data is null or empty");
    }

    [Test]
    public void Validate_WithEmptyMimeType_ShouldReturnFailure()
    {
        var result = new PivResult
        {
            ImageData = _sampleImageData,
            MimeType = "",
            Dimensions = new ImageDimensions(420, 560),
            AppliedTransform = _sampleTransform,
            SourceFace = _sampleFace
        };
        
        var validation = result.Validate();
        
        validation.IsFailure.Should().BeTrue();
        validation.Error.Should().Contain("MIME type is required");
    }

    [Test]
    public void Validate_WithTooSmallDimensions_ShouldReturnFailure()
    {
        var result = new PivResult
        {
            ImageData = _sampleImageData,
            MimeType = "image/jpeg",
            Dimensions = new ImageDimensions(300, 400),
            AppliedTransform = _sampleTransform,
            SourceFace = _sampleFace
        };
        
        var validation = result.Validate();
        
        validation.IsFailure.Should().BeTrue();
        validation.Error.Should().Contain("do not meet PIV minimum requirements");
    }

    [Test]
    public void IsPivCompliant_ShouldReflectTransformCompliance()
    {
        var compliantTransform = _sampleTransform with { IsPivCompliant = true };
        var nonCompliantTransform = _sampleTransform with { IsPivCompliant = false };
        
        var compliantResult = PivResult.Success(
            _sampleImageData,
            "image/jpeg",
            new ImageDimensions(420, 560),
            compliantTransform,
            _sampleFace);
        
        var nonCompliantResult = PivResult.Success(
            _sampleImageData,
            "image/jpeg",
            new ImageDimensions(420, 560),
            nonCompliantTransform,
            _sampleFace);
        
        compliantResult.IsPivCompliant.Should().BeTrue();
        nonCompliantResult.IsPivCompliant.Should().BeFalse();
    }

    [Test]
    public void PivProcessingOptions_Default_ShouldHaveExpectedValues()
    {
        var options = PivProcessingOptions.Default;
        
        options.BaseRate.Should().Be(0.7f);
        options.RoiStartLevel.Should().Be(3);
        options.PreserveExifMetadata.Should().BeFalse();
        options.MinFaceConfidence.Should().Be(0.8f);
        options.RequireSingleFace.Should().BeTrue();
    }

    [Test]
    public void PivProcessingOptions_HighQuality_ShouldHaveHigherSettings()
    {
        var options = PivProcessingOptions.HighQuality;
        
        options.BaseRate.Should().Be(2.0f);
        options.RoiStartLevel.Should().Be(2);
        options.PreserveExifMetadata.Should().BeTrue();
        options.MinFaceConfidence.Should().Be(0.9f);
        options.RequireSingleFace.Should().BeTrue();
    }

    [Test]
    public void PivProcessingOptions_Fast_ShouldHaveLowerSettings()
    {
        var options = PivProcessingOptions.Fast;
        
        options.BaseRate.Should().Be(0.8f);
        options.RoiStartLevel.Should().Be(0);
        options.PreserveExifMetadata.Should().BeFalse();
        options.MinFaceConfidence.Should().Be(0.7f);
        options.RequireSingleFace.Should().BeTrue();
    }

    [Test]
    public void ResultEquality_ShouldWorkCorrectly()
    {
        var result1 = PivResult.Success(
            _sampleImageData,
            "image/jpeg",
            new ImageDimensions(420, 560),
            _sampleTransform,
            _sampleFace);
        
        var result2 = PivResult.Success(
            _sampleImageData,
            "image/jpeg",
            new ImageDimensions(420, 560),
            _sampleTransform,
            _sampleFace);
        
        var result3 = PivResult.Success(
            new byte[] { 6, 7, 8 },
            "image/jpeg",
            new ImageDimensions(420, 560),
            _sampleTransform,
            _sampleFace);
        
        result1.ImageData.Should().Equal(result2.ImageData);
        result1.MimeType.Should().Be(result2.MimeType);
        result1.Dimensions.Should().Be(result2.Dimensions);
        result1.IsPivCompliant.Should().Be(result2.IsPivCompliant);
        
        result1.ImageData.Should().NotEqual(result3.ImageData);
    }
}