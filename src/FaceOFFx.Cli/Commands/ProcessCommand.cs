using System.ComponentModel;
using FaceOFFx.Core.Abstractions;
using FaceOFFx.Core.Domain.Standards;
using FaceOFFx.Core.Domain.Transformations;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
        [DefaultValue("0.7")]
        public string BaseRate { get; set; } = "0.7";

        /// <summary>
        /// ROI resolution level priority: 0=aggressive, 1=balanced, 2=conservative
        /// </summary>
        [CommandOption("--roi-level <LEVEL>")]
        [Description(
            "ROI resolution level priority: 0=aggressive, 1=balanced, 2=conservative, 3=smoothest"
        )]
        [DefaultValue("3")]
        public string RoiStartLevel { get; set; } = "3";

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

            // Parse ROI quality parameters
            var (baseRate, startLevel) = PivConstants.RoiQuality.Default;

            if (float.TryParse(settings.BaseRate, out var parsedRate) && parsedRate > 0)
            {
                baseRate = parsedRate;
                _logger.LogDebug("Parsed custom base rate: {BaseRate}", baseRate);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to parse base rate '{BaseRate}', using default: {DefaultRate}",
                    settings.BaseRate,
                    baseRate
                );
            }

            if (
                int.TryParse(settings.RoiStartLevel, out var parsedLevel)
                && parsedLevel >= 0
                && parsedLevel <= 3
            )
            {
                startLevel = parsedLevel;
                _logger.LogDebug("Parsed custom ROI start level: {StartLevel}", startLevel);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to parse ROI start level '{RoiStartLevel}', using default: {DefaultLevel}",
                    settings.RoiStartLevel,
                    startLevel
                );
            }

            // Parse tile size parameter

            // Configure processing options
            var options = new PivProcessingOptions
            {
                BaseRate = baseRate,
                RoiStartLevel = startLevel,
            };

            // ROI enabled by default for 20KB level 3 quality
            var enableRoi = !settings.NoRoi;
            var roiAlign = settings.Align;

            // Process the image
            bool success = false;

            if (settings.Debug)
            {
                // In debug mode, don't use status spinner as it conflicts with logging
                AnsiConsole.MarkupLine("[grey]Loading image...[/]");
                using var sourceImage = await Image.LoadAsync<Rgba32>(settings.InputPath);

                if (settings.Verbose)
                {
                    AnsiConsole.MarkupLine(
                        $"[blue]Source image: {sourceImage.Width}x{sourceImage.Height} pixels[/]"
                    );
                }

                AnsiConsole.MarkupLine("[grey]Processing with PIV processor...[/]");
                _logger.LogDebug(
                    "Starting PIV processing with options - EnableRoi: {EnableRoi}, RoiAlign: {RoiAlign}",
                    enableRoi,
                    roiAlign
                );

                var result = await PivProcessor.ProcessAsync(
                    sourceImage,
                    _faceDetector,
                    _landmarkExtractor,
                    _jpeg2000Encoder,
                    options,
                    enableRoi,
                    roiAlign,
                    _logger
                );

                if (result.IsSuccess)
                {
                    _logger.LogInformation("PIV processing completed successfully");
                    await HandleSuccess(
                        result.Value,
                        outputPath,
                        settings,
                        baseRate,
                        startLevel,
                        enableRoi,
                        _logger
                    );
                    success = true;
                }
                else
                {
                    _logger.LogError("PIV processing failed: {Error}", result.Error);
                    HandleFailure(result.Error, _logger);
                    success = false;
                }
            }
            else
            {
                // Normal mode with status spinner
                await AnsiConsole
                    .Status()
                    .StartAsync(
                        "Processing image for PIV compliance...",
                        async ctx =>
                        {
                            ctx.Status("Loading image...");
                            using var sourceImage = await Image.LoadAsync<Rgba32>(
                                settings.InputPath
                            );

                            if (settings.Verbose)
                            {
                                AnsiConsole.MarkupLine(
                                    $"[blue]Source image: {sourceImage.Width}x{sourceImage.Height} pixels[/]"
                                );
                            }

                            ctx.Status("Processing with PIV processor...");
                            _logger.LogDebug(
                                "Starting PIV processing with options - EnableRoi: {EnableRoi}, RoiAlign: {RoiAlign}",
                                enableRoi,
                                roiAlign
                            );

                            var result = await PivProcessor.ProcessAsync(
                                sourceImage,
                                _faceDetector,
                                _landmarkExtractor,
                                _jpeg2000Encoder,
                                options,
                                enableRoi,
                                roiAlign,
                                _logger
                            );

                            if (result.IsSuccess)
                            {
                                _logger.LogInformation("PIV processing completed successfully");
                                await HandleSuccess(
                                    result.Value,
                                    outputPath,
                                    settings,
                                    baseRate,
                                    startLevel,
                                    enableRoi,
                                    _logger
                                );
                                success = true;
                            }
                            else
                            {
                                _logger.LogError("PIV processing failed: {Error}", result.Error);
                                HandleFailure(result.Error, _logger);
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
    /// Handles encoding success
    /// </summary>
    /// <param name="result"></param>
    /// <param name="outputPath"></param>
    /// <param name="settings"></param>
    /// <param name="baseRate"></param>
    /// <param name="startLevel"></param>
    /// <param name="enableRoi"></param>
    /// <param name="logger"></param>
    private static async Task HandleSuccess(
        PivResult result,
        string outputPath,
        Settings settings,
        float baseRate,
        int startLevel,
        bool enableRoi,
        ILogger<ProcessCommand> logger
    )
    {
        // Save the already-encoded JPEG 2000 image data directly
        await File.WriteAllBytesAsync(outputPath, result.ImageData);

        if (settings.Verbose)
        {
            if (!enableRoi)
            {
                AnsiConsole.MarkupLine(
                    $"[green]✓ JPEG 2000 encoded without ROI (uniform quality)[/]"
                );
            }
            else
            {
                AnsiConsole.MarkupLine(
                    $"[green]✓ JPEG 2000 encoded with ROI (single Inner Region)[/]"
                );
            }

            AnsiConsole.MarkupLine($"[cyan]  Base rate: {baseRate:F1} bits/pixel[/]");
            AnsiConsole.MarkupLine(
                $"[cyan]  Single tile: {PivConstants.Width}x{PivConstants.Height} pixels[/]"
            );
            if (enableRoi)
            {
                var roiDesc = GetRoiLevelDescription(startLevel);
                AnsiConsole.MarkupLine($"[cyan]  ROI level: {startLevel} ({roiDesc})[/]");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]✓ PIV processing completed successfully![/]");
        AnsiConsole.MarkupLine($"[green]Output saved to: {outputPath}[/]");

        // Show simple results
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]PIV Processing Results[/]");

        table.AddColumn("Property");
        table.AddColumn("Value");

        table.AddRow("PIV Compliant", result.IsPivCompliant ? "[green]✓ Yes[/]" : "[red]✗ No[/]");
        table.AddRow("Output Dimensions", $"{result.Dimensions.Width}x{result.Dimensions.Height}");
        table.AddRow("Processing Summary", result.ProcessingSummary);

        AnsiConsole.Write(table);

        // Show verbose details
        if (settings.Verbose)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[cyan]Transform Details:[/]");
            var transform = result.AppliedTransform;

            AnsiConsole.MarkupLine(
                $"[cyan]  Crop Region: ({transform.CropRegion.Left:F3}, {transform.CropRegion.Top:F3}) "
                    + $"{transform.CropRegion.Width:F3}x{transform.CropRegion.Height:F3}[/]"
            );

            if (result.Metadata.Any())
            {
                AnsiConsole.MarkupLine("[cyan]Metadata:[/]");
                foreach (var kvp in result.Metadata)
                {
                    var valueStr = kvp.Value?.ToString() ?? "null";
                    AnsiConsole.MarkupLine(
                        $"[cyan]  {kvp.Key}: {valueStr.Replace("[", "[[").Replace("]", "]]")}[/]"
                    );
                }
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

    private static string GetRoiLevelDescription(int level)
    {
        return level switch
        {
            0 => "aggressive ROI priority",
            1 => "balanced quality",
            2 => "conservative ROI priority",
            3 => "smoothest transitions",
            _ => "custom",
        };
    }
}
