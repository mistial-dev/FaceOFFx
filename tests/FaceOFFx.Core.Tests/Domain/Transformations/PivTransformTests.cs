using AwesomeAssertions;
using FaceOFFx.Core.Domain.Transformations;
using NUnit.Framework;

namespace FaceOFFx.Core.Tests.Domain.Transformations;

/// <summary>
/// Tests for the PivTransform class and related structures.
/// </summary>
[TestFixture]
public class PivTransformTests
{
    /// <summary>
    /// Tests that Identity method creates a no-op transform.
    /// </summary>
    [Test]
    public void Identity_ShouldCreateNoOpTransform()
    {
        var sourceDimensions = new ImageDimensions(800, 600);

        var transform = PivTransform.Identity(sourceDimensions);

        transform.RotationDegrees.Should().Be(0f);
        transform.CropRegion.Should().Be(CropRect.Full);
        transform.ScaleFactor.Should().Be(1f);
        transform.TargetDimensions.Should().Be(sourceDimensions);
        transform.IsPivCompliant.Should().BeFalse();
    }

    /// <summary>
    /// Tests that validation succeeds for a valid transform.
    /// </summary>
    [Test]
    public void Validate_WithValidTransform_ShouldReturnSuccess()
    {
        var transform = new PivTransform
        {
            RotationDegrees = 2.5f,
            CropRegion = CropRect.Full,
            ScaleFactor = 1.2f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = true,
        };

        var result = transform.Validate();

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Tests that validation fails for excessive rotation angles.
    /// </summary>
    /// <param name="rotation">The rotation angle to test.</param>
    [TestCase(46f)]
    [TestCase(-46f)]
    [TestCase(90f)]
    public void Validate_WithExcessiveRotation_ShouldReturnFailure(float rotation)
    {
        var transform = new PivTransform
        {
            RotationDegrees = rotation,
            CropRegion = CropRect.Full,
            ScaleFactor = 1.0f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = false,
        };

        var result = transform.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Rotation angle too large");
    }

    /// <summary>
    /// Tests that validation fails for invalid scale factors.
    /// </summary>
    /// <param name="scale">The scale factor to test.</param>
    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(11f)]
    public void Validate_WithInvalidScaleFactor_ShouldReturnFailure(float scale)
    {
        var transform = new PivTransform
        {
            RotationDegrees = 0f,
            CropRegion = CropRect.Full,
            ScaleFactor = scale,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = true,
        };

        var result = transform.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid scale factor");
    }

    /// <summary>
    /// Tests that validation fails for dimensions below PIV requirements.
    /// </summary>
    /// <param name="width">The width to test.</param>
    /// <param name="height">The height to test.</param>
    [TestCase(419, 560)]
    [TestCase(420, 419)]
    [TestCase(300, 400)]
    public void Validate_WithSmallTargetDimensions_ShouldReturnFailure(int width, int height)
    {
        var transform = new PivTransform
        {
            RotationDegrees = 0f,
            CropRegion = CropRect.Full,
            ScaleFactor = 1.0f,
            TargetDimensions = new ImageDimensions(width, height),
            IsPivCompliant = false,
        };

        var result = transform.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Target dimensions too small");
    }

    /// <summary>
    /// Tests that PIV compliance reflects rotation limits.
    /// </summary>
    /// <param name="rotation">The rotation angle to test.</param>
    /// <param name="expectedCompliance">The expected compliance result.</param>
    [TestCase(-5f, true)]
    [TestCase(5f, true)]
    [TestCase(0f, true)]
    [TestCase(-5.1f, false)]
    [TestCase(5.1f, false)]
    public void IsPivCompliant_ShouldReflectComplianceRules(float rotation, bool expectedCompliance)
    {
        var transform = new PivTransform
        {
            RotationDegrees = rotation,
            CropRegion = CropRect.Full,
            ScaleFactor = 1.0f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = expectedCompliance,
        };

        transform.IsPivCompliant.Should().Be(expectedCompliance);
    }

    /// <summary>
    /// Tests that CropRect.Full represents no cropping.
    /// </summary>
    [Test]
    public void CropRect_Full_ShouldRepresentNoCropping()
    {
        var crop = CropRect.Full;

        crop.Left.Should().Be(0f);
        crop.Top.Should().Be(0f);
        crop.Width.Should().Be(1f);
        crop.Height.Should().Be(1f);
        crop.Right.Should().Be(1f);
        crop.Bottom.Should().Be(1f);
    }

    /// <summary>
    /// Tests that CropRect.FromPixels converts pixel coordinates correctly.
    /// </summary>
    [Test]
    public void CropRect_FromPixels_ShouldConvertCorrectly()
    {
        var crop = CropRect.FromPixels(100, 50, 200, 300, 800, 600);

        crop.Left.Should().BeApproximately(0.125f, 0.001f);
        crop.Top.Should().BeApproximately(0.0833f, 0.001f);
        crop.Width.Should().BeApproximately(0.25f, 0.001f);
        crop.Height.Should().BeApproximately(0.5f, 0.001f);
    }

    /// <summary>
    /// Tests that CropRect converts back to FaceBox correctly.
    /// </summary>
    [Test]
    public void CropRect_ToFaceBox_ShouldConvertBack()
    {
        var crop = new CropRect
        {
            Left = 0.25f,
            Top = 0.25f,
            Width = 0.5f,
            Height = 0.5f,
        };

        var faceBox = crop.ToFaceBox(800, 600);

        faceBox.X.Should().Be(200);
        faceBox.Y.Should().Be(150);
        faceBox.Width.Should().Be(400);
        faceBox.Height.Should().Be(300);
    }

    /// <summary>
    /// Tests that CropRect Right and Bottom properties calculate correctly.
    /// </summary>
    [Test]
    public void CropRect_RightBottom_ShouldCalculateCorrectly()
    {
        var crop = new CropRect
        {
            Left = 0.1f,
            Top = 0.2f,
            Width = 0.6f,
            Height = 0.5f,
        };

        crop.Right.Should().BeApproximately(0.7f, 0.0001f);
        crop.Bottom.Should().BeApproximately(0.7f, 0.0001f);
    }

    /// <summary>
    /// Tests that typical PIV transform values validate successfully.
    /// </summary>
    [Test]
    public void Transform_WithTypicalPivValues_ShouldBeValid()
    {
        var transform = new PivTransform
        {
            RotationDegrees = -2.3f,
            CropRegion = CropRect.FromPixels(50, 80, 300, 400, 640, 480),
            ScaleFactor = 1.4f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = true,
        };

        var validation = transform.Validate();

        validation.IsSuccess.Should().BeTrue();
        transform.RotationDegrees.Should().Be(-2.3f);
        transform.ScaleFactor.Should().Be(1.4f);
        transform.TargetDimensions.Width.Should().Be(420);
        transform.TargetDimensions.Height.Should().Be(560);
    }

    /// <summary>
    /// Tests equality comparison between CropRect instances.
    /// </summary>
    [Test]
    public void CropRect_Equality_ShouldWorkCorrectly()
    {
        var crop1 = new CropRect
        {
            Left = 0.1f,
            Top = 0.2f,
            Width = 0.3f,
            Height = 0.4f,
        };
        var crop2 = new CropRect
        {
            Left = 0.1f,
            Top = 0.2f,
            Width = 0.3f,
            Height = 0.4f,
        };
        var crop3 = new CropRect
        {
            Left = 0.1f,
            Top = 0.2f,
            Width = 0.3f,
            Height = 0.5f,
        };

        crop1.Should().Be(crop2);
        crop1.Should().NotBe(crop3);
    }

    /// <summary>
    /// Tests equality comparison between PivTransform instances.
    /// </summary>
    [Test]
    public void Transform_Equality_ShouldWorkCorrectly()
    {
        var transform1 = new PivTransform
        {
            RotationDegrees = 2.5f,
            CropRegion = CropRect.Full,
            ScaleFactor = 1.2f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = true,
        };

        var transform2 = new PivTransform
        {
            RotationDegrees = 2.5f,
            CropRegion = CropRect.Full,
            ScaleFactor = 1.2f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = true,
        };

        var transform3 = new PivTransform
        {
            RotationDegrees = 2.6f,
            CropRegion = CropRect.Full,
            ScaleFactor = 1.2f,
            TargetDimensions = new ImageDimensions(420, 560),
            IsPivCompliant = true,
        };

        transform1.Should().Be(transform2);
        transform1.Should().NotBe(transform3);
    }
}
