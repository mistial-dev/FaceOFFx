using CSharpFunctionalExtensions;
using FaceOFFx.Core.Domain.Detection;
using FaceOFFx.Core.Domain.Standards;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FaceOFFx.Cli.Services;

/// <summary>
/// Provides visualization services for facial ROI Inner Region on images.
/// Draws bounding box and annotations for JPEG 2000 ROI encoding visualization.
/// </summary>
/// <remarks>
/// This service creates visual overlays showing the single Inner Region used for JPEG 2000 encoding:
/// - Inner Region: Highest priority, drawn in red (includes complete facial area)
///
/// The visualization helps verify ROI placement and provides visual feedback for
/// JPEG 2000 encoding priority region based on 68-point facial landmarks.
/// </remarks>
public static class RoiVisualizationService
{
    /// <summary>
    /// Color scheme for ROI visualization based on encoding priority.
    /// </summary>
    private static readonly Dictionary<int, Color> PriorityColors = new Dictionary<int, Color>
    {
        { 3, Color.Red }, // Highest priority - Inner Region
    };

    /// <summary>
    /// Draws the ROI Inner Region on the provided image with colored bounding box and label.
    /// </summary>
    /// <param name="sourceImage">The image to draw ROI Inner Region on. Will be cloned, original remains unchanged.</param>
    /// <param name="roiSet">The facial ROI set containing the single Inner Region to visualize.</param>
    /// <param name="strokeWidth">The width of the bounding box strokes (default: 3 pixels). Set to 0 to skip drawing boxes.</param>
    /// <param name="showLabels">Whether to draw priority labels on the ROI (default: true).</param>
    /// <param name="logger">Optional logger for diagnostic output and debugging information.</param>
    /// <returns>
    /// A Result containing either:
    /// - Success: New image with ROI Inner Region drawn as colored bounding box
    /// - Failure: Error message if visualization fails
    /// </returns>
    /// <remarks>
    /// Creates a visual overlay showing:
    /// - Red box: Inner Region - complete facial area with optimized boundaries
    ///
    /// The box is drawn with the specified stroke width and optional priority label.
    /// The source image is cloned to preserve the original.
    /// </remarks>
    /// <example>
    /// <code>
    /// var roiResult = landmarks.CalculateRoiSet(image.Width, image.Height);
    /// if (roiResult.IsSuccess)
    /// {
    ///     var visualizedResult = RoiVisualizationService.DrawRoiRegions(image, roiResult.Value);
    ///     if (visualizedResult.IsSuccess)
    ///     {
    ///         var annotatedImage = visualizedResult.Value;
    ///         // Save or display the annotated image
    ///     }
    /// }
    /// </code>
    /// </example>
    public static Result<Image<Rgba32>> DrawRoiRegions(
        Image<Rgba32> sourceImage,
        FacialRoiSet roiSet,
        float strokeWidth = 3f,
        bool showLabels = true,
        ILogger? logger = null
    )
    {
        logger?.LogDebug(
            "Starting DrawRoiRegions with strokeWidth={StrokeWidth}, showLabels={ShowLabels}",
            strokeWidth,
            showLabels
        );

            // Validate inputs
            var validationResult = roiSet.Validate();
            if (validationResult.IsFailure)
            {
                logger?.LogError(
                    "Invalid ROI set validation failed: {Error}",
                    validationResult.Error
                );
                return Result.Failure<Image<Rgba32>>($"Invalid ROI set: {validationResult.Error}");
            }

            logger?.LogDebug(
                "ROI set validated successfully. Contains {RegionCount} region",
                roiSet.AllRegions.Count()
            );

            // Clone the image to avoid modifying the original
            logger?.LogDebug(
                "Cloning source image with dimensions {Width}x{Height}",
                sourceImage.Width,
                sourceImage.Height
            );
            var annotatedImage = sourceImage.Clone(ctx =>
            {
                // Skip drawing if strokeWidth is 0
                if (strokeWidth <= 0)
                {
                    logger?.LogDebug(
                        "Skipping ROI drawing - strokeWidth is {StrokeWidth}",
                        strokeWidth
                    );
                    return;
                }

                // Draw the ROI Inner Region
                foreach (var region in roiSet.AllRegions)
                {
                    var color = GetColorForPriority(region.Priority);
                    var bbox = region.BoundingBox;

                    logger?.LogDebug(
                        "Drawing ROI {RegionName} with priority {Priority} at ({X},{Y}) size {Width}x{Height}",
                        region.Name,
                        region.Priority,
                        bbox.X,
                        bbox.Y,
                        bbox.Width,
                        bbox.Height
                    );

                    // Draw the bounding rectangle
                    var rectangle = new RectangleF(bbox.X, bbox.Y, bbox.Width, bbox.Height);
                    ctx.Draw(color, strokeWidth, rectangle);

                    // Draw priority label if requested
                    if (showLabels)
                    {
                        logger?.LogDebug(
                            "Drawing priority label for region {RegionName}",
                            region.Name
                        );
                        DrawPriorityLabel(ctx, region, color, logger);
                    }
                }
            });

            logger?.LogDebug("Successfully created annotated image with ROI Inner Region");
            return Result.Success(annotatedImage);
    }

    /// <summary>
    /// Draws only the landmark points on the image without ROI bounding box.
    /// </summary>
    /// <param name="sourceImage">The image to draw landmarks on. Will be cloned, original remains unchanged.</param>
    /// <param name="landmarks">The 68-point facial landmarks to visualize.</param>
    /// <param name="pointSize">The radius of each landmark point in pixels (default: 2).</param>
    /// <param name="pointColor">The color to use for landmark points (default: blue).</param>
    /// <param name="logger">Optional logger for diagnostic output and debugging information.</param>
    /// <returns>
    /// A Result containing either:
    /// - Success: New image with landmark points drawn
    /// - Failure: Error message if visualization fails
    /// </returns>
    /// <remarks>
    /// Draws all 68 facial landmark points as small circles on the image.
    /// Useful for verifying landmark detection accuracy and understanding
    /// the relationship between landmarks and the ROI Inner Region.
    /// </remarks>
    public static Result<Image<Rgba32>> DrawLandmarkPoints(
        Image<Rgba32> sourceImage,
        FaceLandmarks68 landmarks,
        float pointSize = 2f,
        Color? pointColor = null,
        ILogger? logger = null
    )
    {
        logger?.LogDebug(
            "Starting DrawLandmarkPoints with pointSize={PointSize}, pointColor={Color}",
            pointSize,
            pointColor.HasValue ? "specified" : "Blue"
        );

            if (!landmarks.IsValid)
            {
                logger?.LogError(
                    "Invalid landmarks - expected 68 points, got {Count}",
                    landmarks.Points.Count
                );
                return Result.Failure<Image<Rgba32>>(
                    "Invalid landmarks: must have exactly 68 points"
                );
            }

            logger?.LogDebug(
                "Landmarks validated successfully - {Count} points found",
                landmarks.Points.Count
            );

            var color = pointColor ?? Color.Blue;
            logger?.LogDebug("Using specified color for landmark points");

            var annotatedImage = sourceImage.Clone(ctx =>
            {
                var pointIndex = 0;
                foreach (
                    var circle in landmarks.Points.Select(point => new EllipsePolygon(
                        point.X,
                        point.Y,
                        pointSize
                    ))
                )
                {
                    if (pointIndex % 10 == 0) // Log every 10th point to avoid spam
                    {
                        logger?.LogDebug(
                            "Drawing landmark point {Index} at ({X},{Y})",
                            pointIndex,
                            landmarks.Points[pointIndex].X,
                            landmarks.Points[pointIndex].Y
                        );
                    }
                    ctx.Fill(color, circle);
                    pointIndex++;
                }
                logger?.LogDebug("Drew {Count} landmark points", pointIndex);
            });

            logger?.LogDebug("Successfully created annotated image with landmark points");
            return Result.Success(annotatedImage);
    }

    /// <summary>
    /// Creates a comprehensive visualization showing both ROI Inner Region and landmark points.
    /// </summary>
    /// <param name="sourceImage">The image to annotate. Will be cloned, original remains unchanged.</param>
    /// <param name="landmarks">The 68-point facial landmarks.</param>
    /// <param name="roiSet">The facial ROI set containing the Inner Region to visualize.</param>
    /// <param name="strokeWidth">The width of ROI bounding box strokes (default: 3 pixels).</param>
    /// <param name="pointSize">The radius of landmark points (default: 1.5 pixels).</param>
    /// <param name="showLabels">Whether to show ROI priority label (default: true).</param>
    /// <param name="logger">Optional logger for diagnostic output and debugging information.</param>
    /// <returns>
    /// A Result containing either:
    /// - Success: New image with both ROI Inner Region and landmark points drawn
    /// - Failure: Error message if visualization fails
    /// </returns>
    /// <remarks>
    /// Combines ROI bounding box with landmark point visualization to provide
    /// a complete view of the facial analysis results. The ROI Inner Region is drawn
    /// as a colored rectangle while landmarks appear as small blue dots.
    /// </remarks>
    public static Result<Image<Rgba32>> DrawCompleteVisualization(
        Image<Rgba32> sourceImage,
        FaceLandmarks68 landmarks,
        FacialRoiSet roiSet,
        float strokeWidth = 3f,
        float pointSize = 1.5f,
        bool showLabels = true,
        ILogger? logger = null
    )
    {
        logger?.LogDebug(
            "Starting complete visualization with strokeWidth={StrokeWidth}, pointSize={PointSize}, showLabels={ShowLabels}",
            strokeWidth,
                pointSize,
                showLabels
            );

            // First draw the ROI Inner Region
            logger?.LogDebug("Drawing ROI Inner Region");
            var roiResult = DrawRoiRegions(sourceImage, roiSet, strokeWidth, showLabels, logger);
            if (roiResult.IsFailure)
            {
                logger?.LogError("Failed to draw ROI Inner Region: {Error}", roiResult.Error);
                return roiResult;
            }

            // Then overlay the landmark points
            logger?.LogDebug("Overlaying landmark points");
            var completeResult = DrawLandmarkPoints(
                roiResult.Value,
                landmarks,
                pointSize,
                Color.Blue,
                logger
            );
            if (completeResult.IsFailure)
            {
                logger?.LogError("Failed to draw landmark points: {Error}", completeResult.Error);
                return completeResult;
            }

            logger?.LogDebug(
                "Successfully created complete visualization with ROI Inner Region and landmark points"
            );
            return completeResult;
    }

    /// <summary>
    /// Gets the appropriate color for the ROI Inner Region based on its priority level.
    /// </summary>
    /// <param name="priority">The priority level (1=lowest, 3=highest).</param>
    /// <returns>The color associated with the priority level.</returns>
    /// <remarks>
    /// Uses a standardized color scheme:
    /// - Priority 3 (highest): Red - for the Inner Region
    /// - Unknown priorities: Default to gray for safety
    /// </remarks>
    private static Color GetColorForPriority(int priority)
    {
        var color = PriorityColors.TryGetValue(priority, out var c) ? c : Color.Gray;
        return color;
    }

    /// <summary>
    /// Draws a priority label for the ROI Inner Region.
    /// </summary>
    /// <param name="context">The image processing context.</param>
    /// <param name="region">The ROI Inner Region to label.</param>
    /// <param name="color">The color to use for the label.</param>
    /// <param name="logger">Optional logger for diagnostic output and debugging information.</param>
    /// <remarks>
    /// Places a small text label showing the ROI priority and name
    /// in the top-left corner of each bounding box.
    /// </remarks>
    private static void DrawPriorityLabel(
        IImageProcessingContext context,
        RoiRegion region,
        Color color,
        ILogger? logger = null
    )
    {
        try
        {
            logger?.LogDebug(
                "Attempting to draw priority label for region {RegionName} with priority {Priority}",
                region.Name,
                region.Priority
            );

            // Note: Text rendering requires SixLabors.Fonts package which is not currently included
            // The colored bounding box provides sufficient visual distinction for the ROI Inner Region
            // Once the fonts package is added, implement text rendering here

            logger?.LogDebug(
                "Text rendering not available - SixLabors.Fonts package not included. "
                    + "Using colored bounding boxes for ROI distinction"
            );
        }
        catch (Exception ex)
        {
            logger?.LogWarning(
                ex,
                "Failed to draw priority label for region {RegionName}",
                region.Name
            );
            // Silently ignore label drawing failures to avoid breaking ROI visualization
        }
    }

    /// <summary>
    /// Draws PIV compliance lines (AA, BB, CC) on the provided image for validation and debugging.
    /// </summary>
    /// <param name="sourceImage">The image to draw PIV lines on. Will be cloned, original remains unchanged.</param>
    /// <param name="pivLines">The PIV compliance lines to visualize.</param>
    /// <param name="complianceValidation">The compliance validation results for color coding.</param>
    /// <param name="strokeWidth">The width of the line strokes (default: 2 pixels).</param>
    /// <param name="showLabels">Whether to show line labels (default: true).</param>
    /// <param name="logger">Optional logger for debugging information.</param>
    /// <returns>
    /// A Result containing either:
    /// - Success: New image with PIV compliance lines drawn
    /// - Failure: Error message if visualization fails
    /// </returns>
    /// <remarks>
    /// Draws the three PIV compliance lines with color coding:
    /// - Line AA (Vertical Center): Blue if aligned, Red if misaligned
    /// - Line BB (Horizontal Eye): Green if positioned correctly, Orange if incorrect
    /// - Line CC (Head Width): Purple line connecting ear points, with compliance color
    /// Also draws key points (nose center, mouth center, eye centers) as small circles.
    /// </remarks>
    public static Result<Image<Rgba32>> DrawPivComplianceLines(
        Image<Rgba32> sourceImage,
        PivComplianceLines pivLines,
        PivComplianceValidation complianceValidation,
        float strokeWidth = 2f,
        bool showLabels = true,
        ILogger? logger = null
    )
    {
        logger?.LogDebug(
            "Starting PIV compliance lines visualization with strokeWidth={StrokeWidth}",
            strokeWidth
        );

            // Clone the image to avoid modifying the original
            var annotatedImage = sourceImage.Clone(ctx =>
            {
                // Line AA (Vertical Center Line) - Blue if aligned, Red if misaligned
                var lineAaColor = complianceValidation.IsAAAligned ? Color.Blue : Color.Red;
                var verticalLine = new PointF[]
                {
                    new PointF(pivLines.LineAA_X, 0),
                    new PointF(pivLines.LineAA_X, sourceImage.Height),
                };
                ctx.Draw(
                    lineAaColor,
                    strokeWidth,
                    new SixLabors.ImageSharp.Drawing.Path(new LinearLineSegment(verticalLine))
                );
                logger?.LogDebug(
                    "Drew Line AA (vertical center) at X={X} in {Color}",
                    pivLines.LineAA_X,
                    lineAaColor
                );

                // Line BB (Horizontal Eye Line) - Green if positioned correctly, Orange if incorrect
                var lineBbColor = complianceValidation.IsBBPositioned ? Color.Green : Color.Orange;
                var horizontalLine = new PointF[]
                {
                    new PointF(0, pivLines.LineBB_Y),
                    new PointF(sourceImage.Width, pivLines.LineBB_Y),
                };
                ctx.Draw(
                    lineBbColor,
                    strokeWidth,
                    new SixLabors.ImageSharp.Drawing.Path(new LinearLineSegment(horizontalLine))
                );
                logger?.LogDebug(
                    "Drew Line BB (horizontal eye line) at Y={Y} in {Color}",
                    pivLines.LineBB_Y,
                    lineBbColor
                );

                // Line CC (Head Width Line) - Purple if valid ratio, Red if invalid
                var lineCcColor = complianceValidation.IsCCRatioValid ? Color.Purple : Color.Red;
                var headWidthLine = new PointF[]
                {
                    new PointF(pivLines.LeftEarPoint.X, pivLines.LeftEarPoint.Y),
                    new PointF(pivLines.RightEarPoint.X, pivLines.RightEarPoint.Y),
                };
                ctx.Draw(
                    lineCcColor,
                    strokeWidth,
                    new SixLabors.ImageSharp.Drawing.Path(new LinearLineSegment(headWidthLine))
                );
                logger?.LogDebug(
                    "Drew Line CC (head width) from ({X1},{Y1}) to ({X2},{Y2}) in {Color}",
                    pivLines.LeftEarPoint.X,
                    pivLines.LeftEarPoint.Y,
                    pivLines.RightEarPoint.X,
                    pivLines.RightEarPoint.Y,
                    lineCcColor
                );

                // Draw key points as small circles
                var pointRadius = strokeWidth + 1;

                // Nose center - Cyan
                var noseCircle = new EllipsePolygon(
                    pivLines.NoseCenter.X,
                    pivLines.NoseCenter.Y,
                    pointRadius
                );
                ctx.Fill(Color.Cyan, noseCircle);

                // Mouth center - Magenta
                var mouthCircle = new EllipsePolygon(
                    pivLines.MouthCenter.X,
                    pivLines.MouthCenter.Y,
                    pointRadius
                );
                ctx.Fill(Color.Magenta, mouthCircle);

                // Eye centers - Yellow
                var leftEyeCircle = new EllipsePolygon(
                    pivLines.LeftEyeCenter.X,
                    pivLines.LeftEyeCenter.Y,
                    pointRadius
                );
                var rightEyeCircle = new EllipsePolygon(
                    pivLines.RightEyeCenter.X,
                    pivLines.RightEyeCenter.Y,
                    pointRadius
                );
                ctx.Fill(Color.Yellow, leftEyeCircle);
                ctx.Fill(Color.Yellow, rightEyeCircle);

                // Ear points - White
                var leftEarCircle = new EllipsePolygon(
                    pivLines.LeftEarPoint.X,
                    pivLines.LeftEarPoint.Y,
                    pointRadius
                );
                var rightEarCircle = new EllipsePolygon(
                    pivLines.RightEarPoint.X,
                    pivLines.RightEarPoint.Y,
                    pointRadius
                );
                ctx.Fill(Color.White, leftEarCircle);
                ctx.Fill(Color.White, rightEarCircle);

                logger?.LogDebug(
                    "Drew key points: nose (cyan), mouth (magenta), eyes (yellow), ears (white)"
                );
            });

            logger?.LogDebug("Successfully created PIV compliance lines visualization");
            return Result.Success(annotatedImage);
    }
}
