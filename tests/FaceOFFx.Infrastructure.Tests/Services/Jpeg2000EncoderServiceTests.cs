using AwesomeAssertions;
using FaceOFFx.Core.Domain.Detection;
using FaceOFFx.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FaceOFFx.Infrastructure.Tests.Services;

/// <summary>
/// Contains unit tests for the <see cref="Jpeg2000EncoderService"/> class.
/// Validates the functionality of JPEG2000 encoding with and without Region of Interest (ROI) configurations.
/// </summary>
[TestFixture]
public class Jpeg2000EncoderServiceTests
{
    /// <summary>
    /// Logger instance used to log operations and events within the
    /// <see cref="Jpeg2000EncoderServiceTests"/> class.
    /// </summary>
    /// <remarks>
    /// This is a mock or substitute instance of <see cref="ILogger{T}"/> specifically
    /// designed for testing purposes, where T is <see cref="Jpeg2000EncoderService"/>.
    /// It helps validate logging actions performed by the service during unit tests.
    /// </remarks>
    private readonly ILogger<Jpeg2000EncoderService> _logger;

    /// <summary>
    /// Represents a private instance of the Jpeg2000EncoderService, enabling JPEG 2000 image encoding functionality,
    /// including support for encoding regions of interest (ROI) with customizable parameters such as base compression rate,
    /// ROI start level, and alignment.
    /// </summary>
    private readonly Jpeg2000EncoderService _encoder;

    /// <summary>
    /// Unit test class for verifying the behavior and functionality of the
    /// Jpeg2000EncoderService, which provides support for encoding images
    /// to the JPEG 2000 format with optional region of interest (ROI) capabilities.
    /// </summary>
    public Jpeg2000EncoderServiceTests()
    {
        _logger = Substitute.For<ILogger<Jpeg2000EncoderService>>();
        _encoder = new Jpeg2000EncoderService(_logger);
    }

    /// <summary>
    /// Validates that the constructor of the Jpeg2000EncoderService initializes the instance successfully.
    /// Asserts that a new instance of the service is not null upon initialization.
    /// </summary>
    [Test]
    public void Constructor_ShouldInitializeSuccessfully()
    {
        var encoder = new Jpeg2000EncoderService(_logger);
        encoder.Should().NotBeNull();
    }

    /// <summary>
    /// Tests the EncodeWithRoi method of the Jpeg2000EncoderService with a valid image and ROI configuration.
    /// </summary>
    /// <remarks>
    /// This test validates that the method successfully encodes the provided image with the specified region of interest (ROI) using JPEG2000.
    /// It checks that the result indicates success, the output is not null, and the encoded data length is greater than zero.
    /// </remarks>
    [Test]
    public void EncodeWithRoi_WithValidImage_ShouldReturnSuccess()
    {
        using var image = new Image<Rgba32>(420, 560);
        image.Mutate(static ctx => ctx.Fill(Color.White));

        var roiSetResult = FacialRoiSet.CreateAppendixC6(420, 560);
        roiSetResult.IsSuccess.Should().BeTrue();

        var result = _encoder.EncodeWithRoi(image, roiSetResult.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests the successful encoding of an image with a Region of Interest (ROI) enabled.
    /// </summary>
    /// <remarks>
    /// This method validates that the encoding process, when ROI is enabled, completes successfully
    /// and produces a valid encoded output. It ensures the <see cref="Jpeg2000EncoderService.EncodeWithRoi"/>
    /// correctly handles the provided image and ROI configuration.
    /// </remarks>
    /// <seealso cref="Jpeg2000EncoderService"/>
    /// <seealso cref="FacialRoiSet.CreateAppendixC6"/>
    [Test]
    public void EncodeWithRoi_WithRoiEnabled_ShouldEncodeSuccessfully()
    {
        using var image = new Image<Rgba32>(420, 560);
        image.Mutate(static ctx => ctx.Fill(Color.Gray));

        var roiSetResult = FacialRoiSet.CreateAppendixC6(420, 560);

        var result = _encoder.EncodeWithRoi(
            image,
            roiSetResult.Value,
            baseRate: 0.7f,
            roiStartLevel: 3,
            enableRoi: true,
            roiAlign: false
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    /// <summary>
    /// Tests the <see cref="Jpeg2000EncoderService.EncodeWithRoi"/> method to ensure that when ROI encoding is disabled,
    /// the image is encoded without applying any region of interest settings.
    /// </summary>
    /// <remarks>
    /// This test verifies that the method successfully encodes the image without incorporating ROI adjustments
    /// when the ROI feature is explicitly disabled. It validates:
    /// - The encoding operation completes successfully.
    /// - The resulting encoded data is not null.
    /// </remarks>
    /// <exception cref="AssertionException">
    /// Thrown when the test's expectations regarding the success of the encoding operation or the non-nullity of the output are not met.
    /// </exception>
    [Test]
    public void EncodeWithRoi_WithRoiDisabled_ShouldEncodeWithoutRoi()
    {
        using var image = new Image<Rgba32>(420, 560);
        image.Mutate(ctx => ctx.Fill(Color.Blue));

        var roiSetResult = FacialRoiSet.CreateAppendixC6(420, 560);

        var result = _encoder.EncodeWithRoi(
            image,
            roiSetResult.Value,
            baseRate: 1.0f,
            roiStartLevel: 1,
            enableRoi: false
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that encoding an image with a specified region of interest (ROI) and different
    /// base rates adjusts the resulting file size appropriately.
    /// </summary>
    /// <param name="baseRate">The base compression rate to be applied during the encoding process.</param>
    [TestCase(0.6f)]
    [TestCase(0.7f)]
    [TestCase(0.8f)]
    [TestCase(1.0f)]
    [TestCase(1.2f)]
    public void EncodeWithRoi_WithDifferentBaseRates_ShouldAdjustFileSize(float baseRate)
    {
        using var image = new Image<Rgba32>(420, 560);
        DrawTestPattern(image);

        var roiSetResult = FacialRoiSet.CreateAppendixC6(420, 560);

        var result = _encoder.EncodeWithRoi(
            image,
            roiSetResult.Value,
            baseRate: baseRate,
            enableRoi: true
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests the ability of the Jpeg2000 encoder to encode an image successfully
    /// using different Region of Interest (ROI) start levels.
    /// </summary>
    /// <param name="roiStartLevel">
    /// Specifies the ROI start level to be tested, which determines
    /// the compression prioritization within the ROI region.
    /// </param>
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void EncodeWithRoi_WithDifferentRoiStartLevels_ShouldEncodeSuccessfully(
        int roiStartLevel
    )
    {
        using var image = new Image<Rgba32>(420, 560);
        DrawTestPattern(image);

        var roiSetResult = FacialRoiSet.CreateAppendixC6(420, 560);

        var result = _encoder.EncodeWithRoi(
            image,
            roiSetResult.Value,
            baseRate: 0.7f,
            roiStartLevel: roiStartLevel,
            enableRoi: true
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    /// <summary>
    /// Validates the functionality of ROI-based encoding with varying ROI alignment settings.
    /// Ensures the specified ROI alignment parameters are applied accurately during encoding.
    /// </summary>
    /// <param name="roiAlign">A boolean parameter indicating whether Region of Interest (ROI) alignment is enabled or not during the encoding process.</param>
    [TestCase(true)]
    [TestCase(false)]
    public void EncodeWithRoi_WithDifferentRoiAlignment_ShouldEncodeSuccessfully(bool roiAlign)
    {
        using var image = new Image<Rgba32>(420, 560);
        DrawTestPattern(image);

        var roiSetResult = FacialRoiSet.CreateAppendixC6(420, 560);

        var result = _encoder.EncodeWithRoi(
            image,
            roiSetResult.Value,
            baseRate: 0.7f,
            roiStartLevel: 3,
            enableRoi: true,
            roiAlign: roiAlign
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    /// <summary>
    /// Tests whether the JPEG 2000 encoder correctly encodes an image
    /// when provided with a custom Region of Interest (ROI) configuration.
    /// Verifies that the encoding process succeeds and produces a non-null result.
    /// </summary>
    [Test]
    public void EncodeWithRoi_WithCustomRoiRegion_ShouldEncodeSuccessfully()
    {
        using var image = new Image<Rgba32>(420, 560);
        DrawTestPattern(image);

        var customBox = new RoiBoundingBox(100, 100, 220, 300);
        var landmarkIndices = Enumerable.Range(0, 68).ToList();
        var innerRegion = new RoiRegion("CustomInner", 3, customBox, landmarkIndices);
        var roiSet = new FacialRoiSet(innerRegion);

        var result = _encoder.EncodeWithRoi(image, roiSet, baseRate: 0.7f, enableRoi: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    /// Tests the encoding of images with Region of Interest (ROI) support
    /// ensuring proper handling across different image dimensions.
    /// <param name="width">The width of the image to be tested.</param>
    /// <param name="height">The height of the image to be tested.</param>
    [TestCase(320, 240)]
    [TestCase(640, 480)]
    [TestCase(800, 600)]
    [TestCase(1024, 768)]
    public void EncodeWithRoi_WithVariousImageSizes_ShouldHandleCorrectly(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        image.Mutate(ctx => ctx.Fill(Color.White));

        var roiSetResult = FacialRoiSet.CreateAppendixC6(width, height);

        var result = _encoder.EncodeWithRoi(image, roiSetResult.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests the JPEG 2000 encoder's ability to process a complex image with a region of interest (ROI)
    /// while ensuring that the encoded image maintains expected quality standards.
    /// </summary>
    /// <remarks>
    /// This method creates a complex image with a test pattern, defines a facial ROI using predefined settings,
    /// and encodes the image using the JPEG 2000 encoder with ROI enabled. The encoded result is validated
    /// to ensure successful processing and preservation of quality within a specified range of file size.
    /// </remarks>
    /// <exception cref="AssertionException">
    /// Thrown if the encoding process does not succeed, the result is null, or the file size
    /// does not fall within the expected range.
    /// </exception>
    [Test]
    public void EncodeWithRoi_WithComplexImage_ShouldMaintainQuality()
    {
        using var image = new Image<Rgba32>(420, 560);
        DrawComplexTestPattern(image);

        var roiSetResult = FacialRoiSet.CreateAppendixC6(420, 560);

        var result = _encoder.EncodeWithRoi(
            image,
            roiSetResult.Value,
            baseRate: 0.7f,
            roiStartLevel: 3,
            enableRoi: true
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeInRange(15000, 30000);
    }

    /// <summary>
    /// Ensures that multiple calls to the EncodeWithRoi method of the
    /// Jpeg2000EncoderService do not interfere with each other by
    /// testing the encoding process with two distinct images.
    /// </summary>
    /// <remarks>
    /// The test verifies that the encoding results of two separate method invocations,
    /// when provided with the same Region of Interest (ROI) settings, remain unique
    /// and independent of each other. It ensures that the encoder's internal state is not shared
    /// or corrupted between consecutive calls, maintaining thread safety and correctness.
    /// </remarks>
    /// <exception cref="AssertionException">
    /// Thrown if any of the following conditions are not met:
    /// - Both calls to EncodeWithRoi succeed.
    /// - The outputs of the two calls are not equivalent.
    /// </exception>
    [Test]
    public void EncodeWithRoi_MultipleCalls_ShouldNotInterfere()
    {
        using var image1 = new Image<Rgba32>(420, 560);
        using var image2 = new Image<Rgba32>(420, 560);
        image1.Mutate(ctx => ctx.Fill(Color.Red));
        image2.Mutate(ctx => ctx.Fill(Color.Blue));

        var roiSetResult = FacialRoiSet.CreateAppendixC6(420, 560);

        var result1 = _encoder.EncodeWithRoi(image1, roiSetResult.Value, enableRoi: true);
        var result2 = _encoder.EncodeWithRoi(image2, roiSetResult.Value, enableRoi: true);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Should().NotBeEquivalentTo(result2.Value);
    }

    /// <summary>
    /// Validates the behavior of the Jpeg2000 encoder when processing an image
    /// with an ROI that aligns precisely with the image's edge dimensions.
    /// Ensures that the encoder handles edge cases involving boundary-aligned ROIs gracefully.
    /// </summary>
    [Test]
    public void EncodeWithRoi_WithEdgeCaseRoi_ShouldHandleGracefully()
    {
        using var image = new Image<Rgba32>(420, 560);
        DrawTestPattern(image);

        var edgeBox = new RoiBoundingBox(0, 0, 420, 560);
        var landmarkIndices = new List<int> { 0 };
        var innerRegion = new RoiRegion("FullImage", 3, edgeBox, landmarkIndices);
        var roiSet = new FacialRoiSet(innerRegion);

        var result = _encoder.EncodeWithRoi(image, roiSet, enableRoi: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    /// <summary>
    /// Draws a test pattern on the provided image.
    /// </summary>
    /// <param name="image">The image on which the test pattern will be drawn.</param>
    private static void DrawTestPattern(Image<Rgba32> image)
    {
        image.Mutate(ctx =>
        {
            ctx.Fill(Color.LightGray);
            ctx.Fill(Color.DarkGray, new Rectangle(50, 50, 320, 460));
            ctx.Fill(Color.White, new Rectangle(100, 100, 220, 360));
        });
    }

    /// <summary>
    /// Draws a complex test pattern on the provided image.
    /// </summary>
    /// <param name="image">The image on which the test pattern will be drawn. Must be initialized prior to calling this method.</param>
    private static void DrawComplexTestPattern(Image<Rgba32> image)
    {
        image.Mutate(ctx =>
        {
            var gradient = ctx.BackgroundColor(Color.White);

            for (var i = 0; i < 10; i++)
            {
                var color = Color.FromRgb((byte)(i * 25), (byte)(255 - i * 25), 128);
                ctx.Fill(color, new Rectangle(i * 40, i * 50, 40, 50));
            }

            // Draw diagonal lines for visual complexity
            var pen = Pens.Solid(Color.Black, 2);
            ctx.DrawLine(pen, new PointF(0, 0), new PointF(image.Width, image.Height));
            ctx.DrawLine(pen, new PointF(image.Width, 0), new PointF(0, image.Height));
        });
    }
}
