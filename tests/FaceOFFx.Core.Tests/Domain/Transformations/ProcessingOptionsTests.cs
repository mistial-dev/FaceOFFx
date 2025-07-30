using AwesomeAssertions;
using FaceOFFx.Core.Domain.Transformations;
using NUnit.Framework;

namespace FaceOFFx.Core.Tests.Domain.Transformations;

/// <summary>
/// Tests for ProcessingOptions record and presets
/// </summary>
[TestFixture]
public class ProcessingOptionsTests
{
    /// <summary>
    /// Tests that default ProcessingOptions have expected values
    /// </summary>
    [Test]
    public void ProcessingOptions_Default_HasExpectedValues()
    {
        var options = new ProcessingOptions();

        options.MinFaceConfidence.Should().Be(0.8f);
        options.RequireSingleFace.Should().BeTrue();
        options.MaxRetries.Should().Be(2);
        options.ProcessingTimeout.Should().Be(TimeSpan.FromSeconds(30));
        options.PreserveMetadata.Should().BeFalse();
        options.RoiStartLevel.Should().Be(3);
        options.EnableRoi.Should().BeTrue();
        options.AlignRoi.Should().BeFalse();
        options.Strategy.Should().BeOfType<FixedRateStrategy>();
        ((FixedRateStrategy)options.Strategy).Rate.Should().Be(0.7f);
    }

    /// <summary>
    /// Tests TwicMax preset has correct values for TWIC compatibility
    /// </summary>
    [Test]
    public void ProcessingOptions_TwicMax_HasCorrectValues()
    {
        var options = ProcessingOptions.TwicMax;

        options.Strategy.Should().BeOfType<TargetSizeStrategy>();
        ((TargetSizeStrategy)options.Strategy).TargetBytes.Should().Be(14000);
        options.RoiStartLevel.Should().Be(2); // More aggressive ROI for smaller size
        options.MinFaceConfidence.Should().Be(0.8f); // Default confidence
        options.RequireSingleFace.Should().BeTrue();
        options.EnableRoi.Should().BeTrue();
    }

    /// <summary>
    /// Tests PivMin preset has correct values for minimum PIV size
    /// </summary>
    [Test]
    public void ProcessingOptions_PivMin_HasCorrectValues()
    {
        var options = ProcessingOptions.PivMin;

        options.Strategy.Should().BeOfType<TargetSizeStrategy>();
        ((TargetSizeStrategy)options.Strategy).TargetBytes.Should().Be(12000);
        options.RoiStartLevel.Should().Be(1); // Aggressive ROI for minimal size
        options.MinFaceConfidence.Should().Be(0.8f); // Default confidence
        options.RequireSingleFace.Should().BeTrue();
        options.EnableRoi.Should().BeTrue();
    }

    /// <summary>
    /// Tests PivStandard preset has correct values for standard PIV
    /// </summary>
    [Test]
    public void ProcessingOptions_PivStandard_HasCorrectValues()
    {
        var options = ProcessingOptions.PivBalanced;

        options.Strategy.Should().BeOfType<TargetSizeStrategy>();
        ((TargetSizeStrategy)options.Strategy).TargetBytes.Should().Be(20000);
        options.RoiStartLevel.Should().Be(3); // Smoothest transitions
        options.MinFaceConfidence.Should().Be(0.8f);
        options.RequireSingleFace.Should().BeTrue();
        options.EnableRoi.Should().BeTrue();
        options.AlignRoi.Should().BeFalse();
    }

    /// <summary>
    /// Tests PivHigh preset has correct values for high quality PIV
    /// </summary>
    [Test]
    public void ProcessingOptions_PivHigh_HasCorrectValues()
    {
        var options = ProcessingOptions.PivHigh;

        options.Strategy.Should().BeOfType<TargetSizeStrategy>();
        ((TargetSizeStrategy)options.Strategy).TargetBytes.Should().Be(30000);
        options.RoiStartLevel.Should().Be(3); // Smoothest transitions
        options.MinFaceConfidence.Should().Be(0.8f);
        options.RequireSingleFace.Should().BeTrue();
        options.EnableRoi.Should().BeTrue();
    }

    /// <summary>
    /// Tests Archival preset has correct values for long-term storage
    /// </summary>
    [Test]
    public void ProcessingOptions_Archival_HasCorrectValues()
    {
        var options = ProcessingOptions.Archival;

        options.Strategy.Should().BeOfType<FixedRateStrategy>();
        ((FixedRateStrategy)options.Strategy).Rate.Should().Be(4.0f);
        options.RoiStartLevel.Should().Be(3);
        options.PreserveMetadata.Should().BeTrue(); // Preserve for archival
        options.MinFaceConfidence.Should().Be(0.95f); // Higher quality threshold
        options.RequireSingleFace.Should().BeTrue();
        options.EnableRoi.Should().BeTrue();
    }

    /// <summary>
    /// Tests Fast preset has correct values for quick processing
    /// </summary>
    [Test]
    public void ProcessingOptions_Fast_HasCorrectValues()
    {
        var options = ProcessingOptions.Fast;

        options.Strategy.Should().BeOfType<FixedRateStrategy>();
        ((FixedRateStrategy)options.Strategy).Rate.Should().Be(0.5f);
        options.RoiStartLevel.Should().Be(0); // Aggressive ROI for speed
        options.MinFaceConfidence.Should().Be(0.7f); // Lower threshold for speed
        options.MaxRetries.Should().Be(1); // Fewer retries for speed
        options.ProcessingTimeout.Should().Be(TimeSpan.FromSeconds(10)); // Shorter timeout
        options.RequireSingleFace.Should().BeTrue();
    }

    /// <summary>
    /// Tests that record with expressions work correctly
    /// </summary>
    [Test]
    public void ProcessingOptions_WithExpression_ModifiesCorrectly()
    {
        var baseOptions = ProcessingOptions.PivBalanced;
        var modifiedOptions = baseOptions with
        {
            MinFaceConfidence = 0.9f,
            Strategy = EncodingStrategy.FixedRate(2.0f),
        };

        // Original should be unchanged
        baseOptions.MinFaceConfidence.Should().Be(0.8f);
        baseOptions.Strategy.Should().BeOfType<TargetSizeStrategy>();

        // Modified should have new values
        modifiedOptions.MinFaceConfidence.Should().Be(0.9f);
        modifiedOptions.Strategy.Should().BeOfType<FixedRateStrategy>();
        ((FixedRateStrategy)modifiedOptions.Strategy).Rate.Should().Be(2.0f);

        // Other properties should be preserved
        modifiedOptions.RoiStartLevel.Should().Be(baseOptions.RoiStartLevel);
        modifiedOptions.RequireSingleFace.Should().Be(baseOptions.RequireSingleFace);
    }

    /// <summary>
    /// Tests that ProcessingOptions can be created with custom strategy
    /// </summary>
    [Test]
    public void ProcessingOptions_WithCustomStrategy_WorksCorrectly()
    {
        var customTargetSize = 25000;
        var options = ProcessingOptions.PivBalanced with
        {
            Strategy = EncodingStrategy.TargetSize(customTargetSize),
        };

        options.Strategy.Should().BeOfType<TargetSizeStrategy>();
        ((TargetSizeStrategy)options.Strategy).TargetBytes.Should().Be(customTargetSize);
    }

    /// <summary>
    /// Tests equality comparison for ProcessingOptions
    /// </summary>
    [Test]
    public void ProcessingOptions_Equality_WorksCorrectly()
    {
        var options1 = ProcessingOptions.PivBalanced;
        var options2 = ProcessingOptions.PivBalanced;
        var options3 = ProcessingOptions.TwicMax;

        // Same preset should be equal
        options1.Should().Be(options2);
        options1.GetHashCode().Should().Be(options2.GetHashCode());

        // Different presets should not be equal
        options1.Should().NotBe(options3);
        options1.GetHashCode().Should().NotBe(options3.GetHashCode());
    }

    /// <summary>
    /// Tests that all presets have valid timeout values
    /// </summary>
    [Test]
    public void ProcessingOptions_AllPresets_HaveValidTimeouts()
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
            preset.ProcessingTimeout.Should().BeGreaterThan(TimeSpan.Zero);
            (preset.ProcessingTimeout <= TimeSpan.FromMinutes(5)).Should().BeTrue(); // Reasonable upper bound
        }
    }

    /// <summary>
    /// Tests that all presets have valid confidence thresholds
    /// </summary>
    [Test]
    public void ProcessingOptions_AllPresets_HaveValidConfidenceThresholds()
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
            (preset.MinFaceConfidence >= 0.0f).Should().BeTrue();
            (preset.MinFaceConfidence <= 1.0f).Should().BeTrue();
            (preset.MaxRetries >= 0).Should().BeTrue();
            (preset.MaxRetries <= 10).Should().BeTrue(); // Reasonable upper bound
        }
    }

    /// <summary>
    /// Tests that all presets have valid ROI settings
    /// </summary>
    [Test]
    public void ProcessingOptions_AllPresets_HaveValidRoiSettings()
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
            (preset.RoiStartLevel >= 0).Should().BeTrue();
            (preset.RoiStartLevel <= 3).Should().BeTrue();
        }
    }
}
