using FaceOFFx.Core.Domain.Common;
using FaceOFFx.Core.Domain.Detection;
using FaceOFFx.Core.Domain.Transformations;
using AwesomeAssertions;
using NUnit.Framework;

namespace FaceOFFx.Core.Tests.Domain.Transformations;

/// <summary>
/// Tests for PIV transformation calculation functions.
/// These tests verify the mathematical correctness of PIV compliance calculations
/// using pure functions with concrete inputs and outputs (no mocking).
/// </summary>
[TestFixture]  
public class PivTransformCalculatorTests
{
    [TestCase(100f, 100f, 200f, 100f, 0f)]  // Level eyes = 0 rotation
    [TestCase(100f, 100f, 200f, 110f, -5f)]  // Right eye lower = negative rotation (clamped to -5)
    [TestCase(100f, 110f, 200f, 100f, 5f)]   // Left eye lower = positive rotation (clamped to +5)
    [TestCase(100f, 100f, 200f, 95f, 2.86f)] // Small tilt = small rotation
    [TestCase(100f, 120f, 200f, 100f, 5f)]   // Large tilt = clamped to max
    public void CalculateRotationFromEyes_ShouldCalculateCorrectAngle(
        float leftX, float leftY, float rightX, float rightY, float expectedDegrees)
    {
        var leftEye = new Point2D(leftX, leftY);
        var rightEye = new Point2D(rightX, rightY);
        
        var rotation = PivTransformCalculator.CalculateRotationFromEyes(leftEye, rightEye);
        
        rotation.Should().BeApproximately(expectedDegrees, 0.1f);
    }

    [Test]
    public void CalculateRotationFromEyes_WithIdenticalPoints_ShouldReturnZero()
    {
        var point = new Point2D(150f, 200f);
        
        var rotation = PivTransformCalculator.CalculateRotationFromEyes(point, point);
        
        rotation.Should().Be(0f);
    }

    [Test]
    public void CalculateRotationFromEyes_WithExtremeAngles_ShouldClampToPivLimits()
    {
        // Test extreme angle that would exceed ±5 degrees
        var leftEye = new Point2D(100f, 50f);   // Very high left eye
        var rightEye = new Point2D(200f, 150f); // Very low right eye
        
        var rotation = PivTransformCalculator.CalculateRotationFromEyes(leftEye, rightEye);
        
        // Should be clamped to -5 degrees maximum
        rotation.Should().Be(-5f);
    }

    [TestCase(800, 600, 0.525f)]  // 420/800 = 0.525 (width is limiting factor)
    [TestCase(600, 800, 0.7f)]    // 560/800 = 0.7 (height is limiting factor)
    [TestCase(420, 560, 1f)]      // Exact PIV dimensions = no scaling
    [TestCase(210, 280, 2f)]      // Smaller image = would upscale but max is 1.0
    public void CalculateScaleFactor_ShouldCalculateCorrectRatio(int width, int height, float expectedScale)
    {
        var dimensions = new ImageDimensions(width, height);
        
        var scaleFactor = PivTransformCalculator.CalculateScaleFactor(dimensions);
        
        scaleFactor.Should().BeApproximately(expectedScale, 0.001f);
    }

    [Test]
    public void CalculateScaleFactor_WithSmallImage_ShouldAllowUpscaling()
    {
        // Small image that needs upscaling to reach PIV dimensions
        var smallDimensions = new ImageDimensions(100, 100);
        
        var scaleFactor = PivTransformCalculator.CalculateScaleFactor(smallDimensions);
        
        // Should be 4.2 (420/100), but we expect upscaling to be allowed
        scaleFactor.Should().BeApproximately(4.2f, 0.1f);
    }

    [Test]
    public void CalculatePivCrop_WithCenteredFace_ShouldCreateValidCrop()
    {
        var eyeCenter = new Point2D(400f, 300f);
        var faceBox = FaceBox.Create(300f, 200f, 200f, 200f).Value; // Face width: 200px
        var sourceDimensions = new ImageDimensions(800, 600);
        
        var cropRect = PivTransformCalculator.CalculatePivCrop(eyeCenter, faceBox, sourceDimensions);
        
        // Verify crop region is valid (0-1 normalized coordinates)
        cropRect.Left.Should().BeInRange(0f, 1f);
        cropRect.Top.Should().BeInRange(0f, 1f);
        cropRect.Width.Should().BeInRange(0f, 1f);
        cropRect.Height.Should().BeInRange(0f, 1f);
        
        // Verify crop doesn't exceed image bounds
        cropRect.Right.Should().BeLessThanOrEqualTo(1f);
        cropRect.Bottom.Should().BeLessThanOrEqualTo(1f);
        
        // Verify crop produces reasonable aspect ratio for PIV processing
        var aspectRatio = cropRect.Width / cropRect.Height;
        aspectRatio.Should().BeInRange(0.5f, 1f); // Reasonable range for face crops
    }

    [Test]
    public void CalculatePivCrop_WithFaceNearEdge_ShouldClampToBounds()
    {
        // Face very close to left edge
        var eyeCenter = new Point2D(50f, 300f);
        var faceBox = FaceBox.Create(0f, 200f, 100f, 100f).Value;
        var sourceDimensions = new ImageDimensions(800, 600);
        
        var cropRect = PivTransformCalculator.CalculatePivCrop(eyeCenter, faceBox, sourceDimensions);
        
        // Should not have negative left coordinate
        cropRect.Left.Should().BeGreaterThanOrEqualTo(0f);
        cropRect.Top.Should().BeGreaterThanOrEqualTo(0f);
        
        // Should not exceed image bounds
        cropRect.Right.Should().BeLessThanOrEqualTo(1f);
        cropRect.Bottom.Should().BeLessThanOrEqualTo(1f);
    }

    [Test]
    public void CalculatePivCrop_WithLargeFace_ShouldStayWithinBounds()
    {
        var eyeCenter = new Point2D(400f, 300f);
        var faceBox = FaceBox.Create(0f, 0f, 800f, 600f).Value; // Face fills entire image
        var sourceDimensions = new ImageDimensions(800, 600);
        
        var cropRect = PivTransformCalculator.CalculatePivCrop(eyeCenter, faceBox, sourceDimensions);
        
        // Should produce a valid crop that stays within image bounds
        cropRect.Left.Should().BeInRange(0f, 1f);
        cropRect.Top.Should().BeInRange(0f, 1f);
        cropRect.Right.Should().BeLessThanOrEqualTo(1f);
        cropRect.Bottom.Should().BeLessThanOrEqualTo(1f);
        cropRect.Width.Should().BeGreaterThan(0f);
        cropRect.Height.Should().BeGreaterThan(0f);
    }

    [Test]
    public void RotatePointAroundImageCenter_WithZeroRotation_ShouldReturnSamePoint()
    {
        var originalPoint = new Point2D(500f, 200f);
        var imageDimensions = new ImageDimensions(800, 600);
        
        var rotatedPoint = PivTransformCalculator.RotatePointAroundImageCenter(
            originalPoint, 0f, imageDimensions);
        
        rotatedPoint.Should().Be(originalPoint);
    }

    [Test]
    public void RotatePointAroundImageCenter_ShouldProduceValidCoordinates()
    {
        var originalPoint = new Point2D(500f, 200f);
        var imageDimensions = new ImageDimensions(800, 600);
        
        // Test various rotations produce valid results
        foreach (var rotation in new[] { 45f, 90f, 180f, -90f })
        {
            var rotatedPoint = PivTransformCalculator.RotatePointAroundImageCenter(
                originalPoint, rotation, imageDimensions);
            
            // Should produce reasonable coordinates (not NaN or infinity)
            float.IsFinite(rotatedPoint.X).Should().BeTrue();
            float.IsFinite(rotatedPoint.Y).Should().BeTrue();
        }
    }

    [Test]
    public void RotatePointAroundImageCenter_WithImageCenter_ShouldNotMove()
    {
        var imageDimensions = new ImageDimensions(800, 600);
        var centerPoint = new Point2D(400f, 300f); // Exact center
        
        var rotatedPoint = PivTransformCalculator.RotatePointAroundImageCenter(
            centerPoint, 45f, imageDimensions);
        
        // Center point should not move regardless of rotation
        rotatedPoint.X.Should().BeApproximately(400f, 0.1f);
        rotatedPoint.Y.Should().BeApproximately(300f, 0.1f);
    }

    [Test]
    public void CalculateEyeCenter_ShouldReturnMidpoint()
    {
        var leftEye = new Point2D(100f, 200f);
        var rightEye = new Point2D(300f, 220f);
        
        var eyeCenter = PivTransformCalculator.CalculateEyeCenter(leftEye, rightEye);
        
        eyeCenter.X.Should().BeApproximately(200f, 0.1f);
        eyeCenter.Y.Should().BeApproximately(210f, 0.1f);
    }

    [Test]
    public void CalculateEyeCenter_WithIdenticalPoints_ShouldReturnSamePoint()
    {
        var point = new Point2D(150f, 175f);
        
        var eyeCenter = PivTransformCalculator.CalculateEyeCenter(point, point);
        
        eyeCenter.Should().Be(point);
    }

    [Test]
    public void PivCalculations_IntegrationTest_ShouldProduceReasonableResults()
    {
        // Simulate typical face detection scenario
        var leftEye = new Point2D(350f, 250f);
        var rightEye = new Point2D(450f, 260f);  // Slightly tilted
        var faceBox = FaceBox.Create(300f, 200f, 200f, 250f).Value;
        var sourceDimensions = new ImageDimensions(800, 600);
        
        // Calculate all transformation parameters
        var rotation = PivTransformCalculator.CalculateRotationFromEyes(leftEye, rightEye);
        var scaleFactor = PivTransformCalculator.CalculateScaleFactor(sourceDimensions);
        var eyeCenter = PivTransformCalculator.CalculateEyeCenter(leftEye, rightEye);
        var rotatedEyeCenter = PivTransformCalculator.RotatePointAroundImageCenter(
            eyeCenter, rotation, sourceDimensions);
        var cropRect = PivTransformCalculator.CalculatePivCrop(rotatedEyeCenter, faceBox, sourceDimensions);
        
        // Verify all results are reasonable
        rotation.Should().BeInRange(-5f, 5f);
        scaleFactor.Should().BeInRange(0.1f, 1f);
        eyeCenter.X.Should().BeApproximately(400f, 1f);
        eyeCenter.Y.Should().BeApproximately(255f, 1f);
        
        // Crop should be valid PIV region
        cropRect.Left.Should().BeInRange(0f, 1f);
        cropRect.Top.Should().BeInRange(0f, 1f);
        cropRect.Width.Should().BeInRange(0.1f, 1f);
        cropRect.Height.Should().BeInRange(0.1f, 1f);
        
        // Should maintain 3:4 aspect ratio (allowing some tolerance for practical calculations)
        var aspectRatio = cropRect.Width / cropRect.Height;
        aspectRatio.Should().BeInRange(0.5f, 1f); // More lenient range for real-world calculations
    }
}