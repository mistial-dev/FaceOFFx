using System.ComponentModel;
using FaceOFFx.Core.Abstractions;
using FaceOFFx.Core.Domain.Transformations;
using FaceOFFx.Infrastructure.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FaceOFFx.Cli.Commands;

/// <summary>
/// Clean PIV command - just: input image → PIV compliant output
/// </summary>
/// <remarks>
/// Processes an input image for PIV usage
/// </remarks>
/// <param name="faceDetector"></param>
/// <param name="landmarkExtractor"></param>
/// <param name="jpeg2000Encoder"></param>
/// <param name="logger"></param>
/// <exception cref="ArgumentNullException"></exception>
[Description("Process image for PIV compliance")]
public sealed class ProcessCommand(
    IFaceDetector faceDetector,
    ILandmarkExtractor landmarkExtractor,
    IJpeg2000Encoder jpeg2000Encoder,
    ILogger<ProcessCommand> logger
) : AsyncCommand<ProcessCommand.Settings>
{
    /// <inheritdoc />
    [UsedImplicitly]
    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// Input file path
        /// </summary>
        [CommandArgument(0, "<INPUT>")]
        [Description("Input image file path")]
        [UsedImplicitly]
        public string InputPath { get; set; } = string.Empty;

        /// <summary>
        /// Output File Path
        /// </summary>
        [CommandOption("-o|--output <PATH>")]
        [Description("Output file path")]
        [UsedImplicitly]
        public string? OutputPath { get; set; }

        /// <summary>
        /// JPEG 2000 base compression rate in bits per pixel
        /// </summary>
        [CommandOption("--rate <RATE>")]
        [Description("JPEG 2000 base compression rate in bits per pixel")]
        [DefaultValue(0.7f)]
        public float BaseRate { get; set; } = 0.7f;

        /// <summary>
        /// ROI resolution level priority: 0=aggressive, 1=balanced, 2=conservative
        /// </summary>
        [CommandOption("--roi-level <LEVEL>")]
        [Description(
            "ROI resolution level priority: 0=aggressive, 1=balanced, 2=conservative, 3=smoothest"
        )]
        [DefaultValue(3)]
        public int RoiStartLevel { get; set; } = 3;

        /// <summary>
        /// If true, disable ROI encoding (enabled by default with level 3)
        /// </summary>
        [CommandOption("--no-roi")]
        [Description("Disable ROI encoding for uniform quality")]
        public bool NoRoi { get; set; }

        /// <summary>
        /// If true, enable ROI alignment with blocks (disabled by default for smoothest transitions)
        /// </summary>
        [CommandOption("--align")]
        [Description("Enable ROI alignment with compression blocks (may create harsh boundaries)")]
        public bool Align { get; set; }

        /// <summary>
        /// Show detailed processing information
        /// </summary>
        [CommandOption("--verbose")]
        [Description("Show detailed processing information")]
        public bool Verbose { get; set; }

        /// <summary>
        /// Enable debug logging
        /// </summary>
        [CommandOption("--debug")]
        [Description("Enable debug logging for troubleshooting")]
        public bool Debug { get; set; }

        /// <summary>
        /// Processing preset to use
        /// </summary>
        [CommandOption("--preset <PRESET>")]
        [Description(
            "Processing preset: piv-high (30KB), piv-balanced (20KB), twic-max (14KB), piv-min (12KB)"
        )]
        public string? Preset { get; set; }

        /// <summary>
        /// Target file size in bytes (overrides preset and rate)
        /// </summary>
        [CommandOption("--target-size <SIZE>")]
        [Description("Target file size in bytes (overrides preset and rate)")]
        public int? TargetSize { get; set; }

        /// <summary>
        /// Minimum face detection confidence
        /// </summary>
        [CommandOption("--min-confidence <CONFIDENCE>")]
        [Description("Minimum face detection confidence (0.0 to 1.0)")]
        [DefaultValue(0.8f)]
        public float MinConfidence { get; set; } = 0.8f;

        /// <summary>
        /// Maximum rotation angle for face alignment
        /// </summary>
        [CommandOption("--max-rotation <DEGREES>")]
        [Description("Maximum rotation angle in degrees for face alignment")]
        [DefaultValue(15.0f)]
        public float MaxRotation { get; set; } = 15.0f;
    }

    private readonly IFaceDetector _faceDetector =
        faceDetector ?? throw new ArgumentNullException(nameof(faceDetector));
    private readonly ILandmarkExtractor _landmarkExtractor =
        landmarkExtractor ?? throw new ArgumentNullException(nameof(landmarkExtractor));
    private readonly IJpeg2000Encoder _jpeg2000Encoder =
        jpeg2000Encoder ?? throw new ArgumentNullException(nameof(jpeg2000Encoder));
    private readonly ILogger<ProcessCommand> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            // Validate input
            if (!File.Exists(settings.InputPath))
            {
                _logger.LogError("Input file not found: {InputPath}", settings.InputPath);
                AnsiConsole.MarkupLine(
                    $"[red]Error: Input file '{settings.InputPath}' not found.[/]"
                );
                return 1;
            }

            // Determine the output path
            var outputPath = settings.OutputPath ?? GenerateOutputPath(settings.InputPath);
            _logger.LogDebug("Output path determined: {OutputPath}", outputPath);

            // Determine processing options based on parameters
            var processingOptions = DetermineProcessingOptions(settings, _logger);

            // Process the image
            bool success = false;

            if (settings.Debug)
            {
                // In debug mode, don't use status spinner as it conflicts with logging
                AnsiConsole.MarkupLine("[grey]Loading image...[/]");

                var imageData = await File.ReadAllBytesAsync(settings.InputPath);

                if (settings.Verbose)
                {
                    AnsiConsole.MarkupLine($"[blue]Source image: {imageData.Length} bytes[/]");
                }

                AnsiConsole.MarkupLine("[grey]Processing with facial image encoder...[/]");
                _logger.LogDebug(
                    "Starting face processing with options: {Options}",
                    processingOptions
                );

                try
                {
                    var result = await FacialImageEncoder.ProcessAsync(
                        imageData,
                        processingOptions,
                        _logger
                    );

                    _logger.LogInformation("Face processing completed successfully");
                    await HandleSuccess(result, outputPath, settings, _logger);
                    success = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Face processing failed: {Error}", ex.Message);
                    HandleFailure(ex.Message, _logger);
                    success = false;
                }
            }
            else
            {
                // Normal mode with status spinner
                await AnsiConsole
                    .Status()
                    .StartAsync(
                        "Processing image for face recognition...",
                        async ctx =>
                        {
                            ctx.Status("Loading image...");
                            var imageData = await File.ReadAllBytesAsync(settings.InputPath);

                            if (settings.Verbose)
                            {
                                AnsiConsole.MarkupLine(
                                    $"[blue]Source image: {imageData.Length} bytes[/]"
                                );
                            }

                            ctx.Status("Processing with facial image encoder...");
                            _logger.LogDebug(
                                "Starting face processing with options: {Options}",
                                processingOptions
                            );

                            try
                            {
                                var result = await FacialImageEncoder.ProcessAsync(
                                    imageData,
                                    processingOptions,
                                    _logger
                                );

                                _logger.LogInformation("Face processing completed successfully");
                                await HandleSuccess(result, outputPath, settings, _logger);
                                success = true;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("Face processing failed: {Error}", ex.Message);
                                HandleFailure(ex.Message, _logger);
                                success = false;
                            }
                        }
                    );
            }

            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in ProcessCommand.ExecuteAsync");
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    /// <summary>
    /// Handles processing success
    /// </summary>
    /// <param name="result">Processing result</param>
    /// <param name="outputPath">Output file path</param>
    /// <param name="settings">Command settings</param>
    /// <param name="logger">Logger instance</param>
    private static async Task HandleSuccess(
        ProcessingResultDto result,
        string outputPath,
        Settings settings,
        ILogger<ProcessCommand> logger
    )
    {
        // Save the encoded JPEG 2000 image data
        await File.WriteAllBytesAsync(outputPath, result.ImageData);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]✓ Face processing completed successfully![/]");
        AnsiConsole.MarkupLine($"[green]Output saved to: {outputPath}[/]");

        // Show processing results
        var table = new Table().Border(TableBorder.Rounded).Title("[bold]Processing Results[/]");

        table.AddColumn("Property");
        table.AddColumn("Value");

        var metadata = result.Metadata;
        table.AddRow(
            "Output Dimensions",
            $"{metadata.OutputDimensions.Width}x{metadata.OutputDimensions.Height}"
        );
        table.AddRow("File Size", $"{metadata.FileSize:N0} bytes");
        table.AddRow("Face Confidence", $"{metadata.FaceConfidence:F2}");
        table.AddRow("Processing Time", $"{metadata.ProcessingTime.TotalMilliseconds:F0}ms");

        if (metadata.CompressionRate > 0)
            table.AddRow("Compression Rate", $"{metadata.CompressionRate:F2} bpp");

        if (metadata.TargetSize.HasValue)
            table.AddRow("Target Size", $"{metadata.TargetSize.Value:N0} bytes");

        AnsiConsole.Write(table);

        // Show verbose details
        if (settings.Verbose && metadata.AdditionalData.Any())
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[cyan]Additional Details:[/]");
            foreach (var kvp in metadata.AdditionalData)
            {
                var valueStr = kvp.Value?.ToString() ?? "null";
                AnsiConsole.MarkupLine(
                    $"[cyan]  {kvp.Key}: {valueStr.Replace("[", "[[").Replace("]", "]]")}[/]"
                );
            }
        }
    }

    /// <summary>
    /// Handles encoding failures
    /// </summary>
    /// <param name="error"></param>
    /// <param name="logger"></param>
    private static void HandleFailure(string error, ILogger<ProcessCommand> logger)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[red]✗ Processing failed: {error}[/]");

        // Provide helpful suggestions based on error type
        if (error.Contains("No faces detected"))
        {
            AnsiConsole.MarkupLine("[yellow]💡 Suggestions:[/]");
            AnsiConsole.MarkupLine(
                "[yellow]  • Ensure the image contains a clear, visible face[/]"
            );
            AnsiConsole.MarkupLine("[yellow]  • Check that the image is well-lit and in focus[/]");
        }
        else if (error.Contains("Multiple faces"))
        {
            AnsiConsole.MarkupLine("[yellow]💡 Suggestions:[/]");
            AnsiConsole.MarkupLine("[yellow]  • Crop the image to contain only one face[/]");
        }
    }

    /// <summary>
    /// Converts the input extension to .jp2 for the output file.
    /// </summary>
    /// <param name="inputPath"></param>
    /// <returns></returns>
    private static string GenerateOutputPath(string inputPath)
    {
        var directory = Path.GetDirectoryName(inputPath) ?? ".";
        var nameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
        return Path.Combine(directory, $"{nameWithoutExt}.jp2");
    }

    /// <summary>
    /// Determines processing options based on command line settings
    /// </summary>
    private static ProcessingOptions DetermineProcessingOptions(
        Settings settings,
        ILogger<ProcessCommand> logger
    )
    {
        // Check for preset first
        if (!string.IsNullOrEmpty(settings.Preset))
        {
            var preset = GetPresetFromString(settings.Preset, logger);
            if (preset != null)
            {
                logger.LogDebug("Using preset: {Preset}", settings.Preset);
                return ApplyOverrides(preset, settings, logger);
            }
        }

        // Check for target size
        if (settings.TargetSize.HasValue)
        {
            logger.LogDebug("Using target size: {TargetSize} bytes", settings.TargetSize.Value);
            var options = ProcessingOptions.PivBalanced with
            {
                Strategy = EncodingStrategy.TargetSize(settings.TargetSize.Value),
            };
            return ApplyOverrides(options, settings, logger);
        }

        // Use custom rate if specified
        if (settings.BaseRate > 0)
        {
            logger.LogDebug("Using custom rate: {Rate} bpp", settings.BaseRate);
            var options = ProcessingOptions.PivBalanced with
            {
                Strategy = EncodingStrategy.FixedRate(settings.BaseRate),
            };
            return ApplyOverrides(options, settings, logger);
        }

        // Default to PIV standard
        logger.LogDebug("Using default PIV standard options");
        return ApplyOverrides(ProcessingOptions.PivBalanced, settings, logger);
    }

    /// <summary>
    /// Gets a processing preset from string name
    /// </summary>
    private static ProcessingOptions? GetPresetFromString(
        string presetName,
        ILogger<ProcessCommand> logger
    )
    {
        return presetName.ToLowerInvariant() switch
        {
            "twic-max" => ProcessingOptions.TwicMax,
            "piv-min" => ProcessingOptions.PivMin,
            "piv-balanced" => ProcessingOptions.PivBalanced,
            "piv-high" => ProcessingOptions.PivHigh,
            "piv-veryhigh" => ProcessingOptions.PivVeryHigh,
            "archival" => ProcessingOptions.Archival,
            "minimal" => ProcessingOptions.Minimal,
            "fast" => ProcessingOptions.Fast,
            _ => null,
        };
    }

    /// <summary>
    /// Applies CLI setting overrides to processing options
    /// </summary>
    private static ProcessingOptions ApplyOverrides(
        ProcessingOptions baseOptions,
        Settings settings,
        ILogger<ProcessCommand> logger
    )
    {
        var options = baseOptions;

        // Apply ROI level override
        if (settings.RoiStartLevel >= 0 && settings.RoiStartLevel <= 3)
        {
            if (settings.RoiStartLevel != baseOptions.RoiStartLevel)
            {
                logger.LogDebug(
                    "Overriding ROI level: {OldLevel} → {NewLevel}",
                    baseOptions.RoiStartLevel,
                    settings.RoiStartLevel
                );
                options = options with { RoiStartLevel = settings.RoiStartLevel };
            }
        }

        // Apply ROI enable/disable override
        if (settings.NoRoi && baseOptions.EnableRoi)
        {
            logger.LogDebug("Disabling ROI encoding");
            options = options with { EnableRoi = false };
        }

        // Apply ROI alignment override
        if (settings.Align && !baseOptions.AlignRoi)
        {
            logger.LogDebug("Enabling ROI alignment");
            options = options with { AlignRoi = true };
        }

        // Apply min confidence override
        if (
            settings.MinConfidence >= 0.0f
            && settings.MinConfidence <= 1.0f
            && Math.Abs(settings.MinConfidence - baseOptions.MinFaceConfidence) > 0.001f
        )
        {
            logger.LogDebug(
                "Overriding minimum face confidence: {OldConfidence} → {NewConfidence}",
                baseOptions.MinFaceConfidence,
                settings.MinConfidence
            );
            options = options with { MinFaceConfidence = settings.MinConfidence };
        }

        // Apply max rotation override
        if (
            settings.MaxRotation > 0.0f
            && Math.Abs(settings.MaxRotation - baseOptions.MaxRotationDegrees) > 0.001f
        )
        {
            logger.LogDebug(
                "Overriding maximum rotation: {OldRotation}° → {NewRotation}°",
                baseOptions.MaxRotationDegrees,
                settings.MaxRotation
            );
            options = options with { MaxRotationDegrees = settings.MaxRotation };
        }

        return options;
    }
}
