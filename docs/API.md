# FaceOFFx API Documentation

FaceOFFx is a specialized facial processing library for .NET that transforms facial images to comply with PIV (Personal Identity Verification) and TWIC (Transportation Worker Identification Credential) standards. This documentation covers the complete public API surface with extensive examples and use cases.

## Table of Contents

- [Quick Start](#quick-start)
- [Installation](#installation)
- [API Architecture](#api-architecture)
- [FacialImageEncoder API (Recommended)](#facialimageencoder-api-recommended)
- [PivProcessor API (Advanced)](#pivprocessor-api-advanced)
- [Configuration Options](#configuration-options)
- [Data Models & Results](#data-models--results)
- [Examples by Use Case](#examples-by-use-case)
- [Advanced Topics](#advanced-topics)
- [Error Handling](#error-handling)

## Quick Start

The simplest way to process a facial image using standard .NET exception handling:

```csharp
using FaceOFFx.Infrastructure.Services;

// Load image bytes from file
byte[] imageData = await File.ReadAllBytesAsync("photo.jpg");

try
{
    // Process to PIV-compatible JPEG 2000 format (20KB target)
    var result = await FacialImageEncoder.ProcessAsync(imageData);

    Console.WriteLine($"Processing successful!");
    Console.WriteLine($"Output size: {result.Metadata.FileSize:N0} bytes");
    Console.WriteLine($"Face confidence: {result.Metadata.FaceConfidence:P1}");
    Console.WriteLine($"Rotation applied: {result.Metadata.RotationApplied:F1}°");

    // Save the processed image
    await File.WriteAllBytesAsync("output.jp2", result.ImageData);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Processing failed: {ex.Message}");
}
```

## Installation

Install the FaceOFFx NuGet package:

```bash
dotnet add package FaceOFFx
```

Or via Package Manager:

```powershell
Install-Package FaceOFFx
```

The library requires .NET 8.0 or later and includes all necessary ONNX models via Git LFS.

## API Architecture

FaceOFFx provides two distinct API levels designed for different user needs:

### 1. FacialImageEncoder (Public API - Recommended)

- **Uses standard .NET exception handling** - no functional programming knowledge required
- **Returns concrete results** - `ProcessingResultDto` with all metadata
- **Automatic service management** - no manual disposal or setup needed  
- **Simple error handling** - standard try/catch blocks
- **Target audience**: Regular .NET developers, production applications

### 2. PivProcessor (Advanced API)

- **Uses functional error handling** - requires CSharpFunctionalExtensions knowledge
- **Returns `Result<T>` types** - functional programming patterns
- **Manual service management** - you control service lifecycle
- **Detailed control** - full access to transformation pipeline
- **Target audience**: Advanced developers, custom integrations, functional programming enthusiasts

## FacialImageEncoder API (Recommended)

The `FacialImageEncoder` is the primary public API that uses standard .NET conventions.

### Basic Processing

```csharp
using FaceOFFx.Infrastructure.Services;
using FaceOFFx.Core.Domain.Transformations;

// Process with default PIV settings (20KB target)
var result = await FacialImageEncoder.ProcessAsync(imageData);
```

### Processing with Custom Options

The library uses immutable C# records for configuration. Use the `with` syntax to modify options:

```csharp
// Start with a preset and customize
var customOptions = ProcessingOptions.PivBalanced with
{
    MinFaceConfidence = 0.9f,      // Require 90% confidence
    MaxRotationDegrees = 5.0f,     // Limit rotation to ±5°
    RequireSingleFace = false,     // Allow multiple faces (use best)
    MaxRetries = 3,                // More bitrate attempts for size targeting
    PreserveMetadata = true,       // Keep EXIF data
    RoiStartLevel = 2              // Different ROI smoothness
};

var result = await FacialImageEncoder.ProcessAsync(imageData, customOptions);
```

### Preset-based Processing

```csharp
// TWIC card processing (14KB maximum)
var twicResult = await FacialImageEncoder.ProcessForTwicAsync(imageData);

// Standard PIV processing (20KB target)  
var pivResult = await FacialImageEncoder.ProcessForPivAsync(imageData);

// Custom target size
var customSizeResult = await FacialImageEncoder.ProcessToSizeAsync(imageData, 25000);

// Fixed compression rate
var rateResult = await FacialImageEncoder.ProcessWithRateAsync(imageData, 1.5f);
```

### Error Handling with FacialImageEncoder

Standard .NET exception handling - no functional programming required:

```csharp
try
{
    var result = await FacialImageEncoder.ProcessAsync(imageData);
    
    // Success - use the result
    Console.WriteLine($"Success: {result.Metadata.FileSize} bytes");
    await File.WriteAllBytesAsync("output.jp2", result.ImageData);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Invalid input: {ex.Message}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid image or options: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Processing failed: {ex.Message}");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Processing timeout: {ex.Message}");
}
```

### Try Pattern (Alternative Error Handling)

If you prefer not to use exceptions:

```csharp
var (success, result, errorMessage) = await FacialImageEncoder.TryProcessAsync(imageData);

if (success)
{
    Console.WriteLine($"Success: {result!.Metadata.FileSize} bytes");
    await File.WriteAllBytesAsync("output.jp2", result.ImageData);
}
else
{
    Console.WriteLine($"Failed: {errorMessage}");
}
```

## PivProcessor API (Advanced)

The `PivProcessor` provides functional error handling and full control over the processing pipeline. **Requires knowledge of CSharpFunctionalExtensions.**

### Manual Service Management

```csharp
using FaceOFFx.Core.Domain.Transformations;
using FaceOFFx.Infrastructure.Services;
using FaceOFFx.Core.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

// Initialize services manually
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("FaceOFFx");
var faceDetector = new RetinaFaceDetector(logger);
var landmarkExtractor = new OnnxLandmarkExtractor(logger);
var jpeg2000Encoder = new Jpeg2000EncoderService(logger);

// Load image
using var sourceImage = await Image.LoadAsync<Rgba32>("input.jpg");

// Process with functional error handling
var result = await PivProcessor.ProcessAsync(
    sourceImage,
    faceDetector,
    landmarkExtractor,
    jpeg2000Encoder,
    ProcessingOptions.PivHigh,
    enableRoi: true,
    roiAlign: false,
    logger);

// Functional error handling
if (result.IsSuccess)
{
    var pivResult = result.Value;
    
    // Access detailed transformation data
    Console.WriteLine($"PIV compliant: {pivResult.IsPivCompliant}");
    Console.WriteLine($"Applied rotation: {pivResult.AppliedTransform.RotationDegrees}°");
    Console.WriteLine($"Crop region: {pivResult.AppliedTransform.CropRegion}");
    
    // Save result
    await File.WriteAllBytesAsync("output.jp2", pivResult.ImageData);
}
else
{
    Console.WriteLine($"Processing failed: {result.Error}");
}

// Clean up services
faceDetector.Dispose();
landmarkExtractor.Dispose();
```

### Simple File Conversion (PivProcessor)

```csharp
// Convert JPEG to PIV-compliant JP2 in one call
var result = await PivProcessor.ConvertJpegToPivJp2Async(
    "input.jpg",
    "output.jp2", 
    faceDetector,
    landmarkExtractor,
    jpeg2000Encoder);

if (result.IsSuccess)
{
    Console.WriteLine("Conversion successful!");
}
else
{
    Console.WriteLine($"Conversion failed: {result.Error}");
}
```

## Configuration Options

Both APIs use the same `ProcessingOptions` record for configuration.

### Built-in Presets

```csharp
// Government card presets
ProcessingOptions.TwicMax       // 14KB - TWIC card maximum
ProcessingOptions.PivMin        // 12KB - PIV minimum space  
ProcessingOptions.PivBalanced   // 20KB - Standard PIV (recommended)
ProcessingOptions.PivHigh       // 30KB - Enhanced PIV quality
ProcessingOptions.PivVeryHigh   // 50KB - Premium quality

// Special purpose presets
ProcessingOptions.Archival      // ~82KB - Long-term preservation
ProcessingOptions.Minimal       // ~15KB - Smallest file size
ProcessingOptions.Fast          // Same as PivBalanced but fail-fast
```

#### Preset Comparison

| Preset      | Target Size | Actual Size | Compression Rate | Use Case                      |
|-------------|-------------|-------------|------------------|-------------------------------|
| TwicMax     | 14KB        | ~12KB       | Variable         | TWIC card compatibility       |
| PivMin      | 12KB        | ~11.8KB     | 0.36 bpp         | PIV minimum space             |
| PivBalanced | 20KB        | ~17.7KB     | 0.55 bpp         | **Default** - Optimal balance |
| PivHigh     | 30KB        | ~29.4KB     | 0.96 bpp         | Enhanced quality              |
| PivVeryHigh | 50KB        | ~49.7KB     | 1.70 bpp         | Premium quality               |
| Archival    | -           | ~82KB       | 4.00 bpp         | Long-term preservation        |
| Minimal     | -           | ~14.7KB     | 0.48 bpp         | Smallest possible             |
| Fast        | 20KB        | ~17.7KB     | 0.55 bpp         | Quick processing              |

### Custom Configuration

ProcessingOptions is an immutable record. Use the `with` syntax to create variations:

```csharp
// Start with a preset and modify specific properties
var highSecurityOptions = ProcessingOptions.Archival with
{
    MinFaceConfidence = 0.95f,        // Very high confidence required
    RequireSingleFace = true,         // Must be exactly one face
    MaxRotationDegrees = 5.0f,        // Limit rotation for consistency
    MaxRetries = 1,                   // Fail fast for security
    ProcessingTimeout = TimeSpan.FromSeconds(15)
};

// Create completely custom options
var customOptions = new ProcessingOptions
{
    Strategy = EncodingStrategy.TargetSize(18000),  // 18KB target
    MinFaceConfidence = 0.85f,
    RequireSingleFace = false,
    MaxRetries = 3,
    ProcessingTimeout = TimeSpan.FromSeconds(45),
    RoiStartLevel = 3,                // Smoothest quality transitions
    EnableRoi = true,                 // Enable ROI encoding
    AlignRoi = false,                 // Smooth transitions 
    PreserveMetadata = false,         // Strip EXIF for privacy
    MaxRotationDegrees = 10.0f
};
```

### Encoding Strategies

#### Target Size Strategy

```csharp
// Target specific file size (library tries multiple compression rates)
var targetSizeOptions = ProcessingOptions.PivBalanced with
{
    Strategy = EncodingStrategy.TargetSize(22000)  // Target 22KB
};
```

#### Fixed Rate Strategy

```csharp
// Use fixed compression rate (predictable behavior)
var fixedRateOptions = ProcessingOptions.PivBalanced with
{
    Strategy = EncodingStrategy.FixedRate(1.2f)    // 1.2 bits per pixel
};
```

## Data Models & Results

### ProcessingResultDto (FacialImageEncoder)

The non-functional result type returned by `FacialImageEncoder`:

```csharp
public sealed record ProcessingResultDto(byte[] ImageData, ProcessingMetadataDto Metadata);

public sealed record ProcessingMetadataDto(
    ImageDimensions OutputDimensions,    // Always 420x560 for PIV
    float RotationApplied,               // Degrees of rotation correction
    float FaceConfidence,                // Detection confidence (0.0-1.0)
    int FileSize,                        // Final compressed size in bytes
    TimeSpan ProcessingTime              // Total processing duration
)
{
    public float CompressionRate { get; init; }           // Actual compression rate used
    public int? TargetSize { get; init; }                 // Target size if using TargetSizeStrategy
    public IReadOnlyList<string> Warnings { get; init; }  // Processing warnings
    public IReadOnlyDictionary<string, object> AdditionalData { get; init; }
}
```

#### Usage Example

```csharp
var result = await FacialImageEncoder.ProcessAsync(imageData);

var metadata = result.Metadata;
Console.WriteLine($"Output dimensions: {metadata.OutputDimensions.Width}x{metadata.OutputDimensions.Height}");
Console.WriteLine($"File size: {metadata.FileSize:N0} bytes");
Console.WriteLine($"Face confidence: {metadata.FaceConfidence:P1}");
Console.WriteLine($"Rotation applied: {metadata.RotationApplied:F1}°");
Console.WriteLine($"Compression rate: {metadata.CompressionRate:F2} bpp");
Console.WriteLine($"Processing time: {metadata.ProcessingTime.TotalMilliseconds:F0}ms");

if (metadata.TargetSize.HasValue)
{
    Console.WriteLine($"Target size: {metadata.TargetSize.Value:N0} bytes");
}

if (metadata.Warnings.Any())
{
    Console.WriteLine($"Warnings: {string.Join(", ", metadata.Warnings)}");
}
```

## Examples by Use Case

### Government ID Card Processing

#### Standard PIV Processing

```csharp
// Standard government PIV cards with custom quality
byte[] photoData = await File.ReadAllBytesAsync("employee_photo.jpg");

var options = ProcessingOptions.PivHigh with
{
    MinFaceConfidence = 0.95f,  // High confidence for compliance
    PreserveMetadata = false   // Strip metadata for privacy
};

try
{
    var result = await FacialImageEncoder.ProcessAsync(photoData, options);

    Console.WriteLine($"PIV processing complete:");
    Console.WriteLine($"  File size: {result.Metadata.FileSize:N0} bytes");
    Console.WriteLine($"  Face confidence: {result.Metadata.FaceConfidence:P1}");
    Console.WriteLine($"  Processing time: {result.Metadata.ProcessingTime.TotalMilliseconds:F0}ms");

    await File.WriteAllBytesAsync("piv_photo.jp2", result.ImageData);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"PIV processing failed: {ex.Message}");
}
```

#### TWIC Card Processing

```csharp
// TWIC cards have strict 14KB size limits
try
{
    var twicResult = await FacialImageEncoder.ProcessForTwicAsync(photoData);
    
    if (twicResult.Metadata.FileSize <= 14000)
    {
        await File.WriteAllBytesAsync("twic_photo.jp2", twicResult.ImageData);
        Console.WriteLine("TWIC processing successful - size compliant");
    }
    else
    {
        // Won't happen due to TWIC targeting limits
        Console.WriteLine($"Warning: File size {twicResult.Metadata.FileSize} may exceed TWIC limits");
    }
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"TWIC processing failed: {ex.Message}");
}
```

### Batch Processing

```csharp
public async Task ProcessPhotosBatchAsync(string inputDirectory, string outputDirectory)
{
    var imageFiles = Directory.GetFiles(inputDirectory, "*.jpg");
    var results = new List<(string File, bool Success, int Size, string Error)>();
    
    // Use Fast preset for quick batch processing
    var batchOptions = ProcessingOptions.Fast with
    {
        ProcessingTimeout = TimeSpan.FromSeconds(10),  // Quick timeout
        MaxRetries = 1                                 // Minimal retries
    };
    
    foreach (var imageFile in imageFiles)
    {
        try
        {
            var imageData = await File.ReadAllBytesAsync(imageFile);
            var result = await FacialImageEncoder.ProcessAsync(imageData, batchOptions);
            
            var outputFile = Path.Combine(outputDirectory, 
                Path.GetFileNameWithoutExtension(imageFile) + ".jp2");
            await File.WriteAllBytesAsync(outputFile, result.ImageData);
            
            results.Add((imageFile, true, result.Metadata.FileSize, ""));
            Console.WriteLine($"✓ {Path.GetFileName(imageFile)} -> {result.Metadata.FileSize:N0} bytes");
        }
        catch (Exception ex)
        {
            results.Add((imageFile, false, 0, ex.Message));
            Console.WriteLine($"✗ {Path.GetFileName(imageFile)}: {ex.Message}");
        }
    }
    
    // Summary
    var successful = results.Count(r => r.Success);
    var avgSize = results.Where(r => r.Success).Average(r => r.Size);
    
    Console.WriteLine($"\nBatch processing complete:");
    Console.WriteLine($"  Processed: {successful}/{results.Count} files");
    Console.WriteLine($"  Average size: {avgSize:N0} bytes");
    Console.WriteLine($"  Failed files: {results.Count(r => !r.Success)}");
}
```

### Quality Comparison

```csharp
public async Task CompareQualityPresetsAsync(byte[] imageData)
{
    var presets = new[]
    {
        ("Minimal", ProcessingOptions.Minimal),
        ("PIV Min", ProcessingOptions.PivMin),
        ("PIV Balanced", ProcessingOptions.PivBalanced),
        ("PIV High", ProcessingOptions.PivHigh),
        ("PIV Very High", ProcessingOptions.PivVeryHigh),
        ("Archival", ProcessingOptions.Archival)
    };
    
    Console.WriteLine("Quality Preset Comparison:");
    Console.WriteLine("".PadRight(70, '-'));
    Console.WriteLine($"{"Preset",-15} | {"Size",8} | {"Rate",6} | {"Time",6} | {"Confidence",10}");
    Console.WriteLine("".PadRight(70, '-'));
    
    foreach (var (name, preset) in presets)
    {
        try
        {
            var result = await FacialImageEncoder.ProcessAsync(imageData, preset);
            var meta = result.Metadata;
            
            Console.WriteLine($"{name,-15} | " +
                            $"{meta.FileSize,6:N0}B | " +
                            $"{meta.CompressionRate,4:F2}bp | " +
                            $"{meta.ProcessingTime.TotalMilliseconds,5:F0}ms | " +
                            $"{meta.FaceConfidence,9:P1}");
            
            // Save for visual comparison
            await File.WriteAllBytesAsync($"comparison_{name.ToLower().Replace(" ", "_")}.jp2", 
                result.ImageData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{name,-15} | FAILED: {ex.Message}");
        }
    }
}
```

## Advanced Topics

### JPEG 2000 ROI Encoding

FaceOFFx uses advanced Region of Interest (ROI) encoding to optimize quality distribution:

```csharp
// ROI levels control quality transition smoothness
var roiOptions = ProcessingOptions.PivBalanced with
{
    RoiStartLevel = 0,    // Aggressive ROI priority - sharp quality differences
    EnableRoi = true,     // Enable ROI encoding
    AlignRoi = false      // Smooth transitions (recommended)
};

// Disable ROI for uniform quality throughout the image
var uniformOptions = ProcessingOptions.PivBalanced with
{
    EnableRoi = false     // Uniform quality across entire image
};
```

**ROI Start Levels:**

- **Level 0**: Aggressive ROI priority - maximum facial quality, sharp transitions
- **Level 1**: Balanced quality distribution
- **Level 2**: Conservative ROI priority
- **Level 3**: Smoothest quality transitions (default, recommended)

### Performance Optimization

```csharp
// For high-throughput scenarios, optimize processing options
var performanceOptions = ProcessingOptions.Fast with
{
    MaxRetries = 0,                                // Single bitrate attempt for speed
    ProcessingTimeout = TimeSpan.FromSeconds(5),   // Quick timeout
    Strategy = EncodingStrategy.FixedRate(0.6f)    // Fixed rate avoids size targeting overhead
};
```

## Error Handling

### Standard .NET Exception Handling

```csharp
public async Task<ProcessingResultDto?> ProcessWithFullErrorHandlingAsync(byte[] imageData)
{
    try
    {
        var result = await FacialImageEncoder.ProcessAsync(imageData, ProcessingOptions.PivBalanced);
        return result;
    }
    catch (ArgumentNullException)
    {
        Console.WriteLine("Error: Image data is null");
        return null;
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Error: Invalid image data or options - {ex.Message}");
        return null;
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("No faces detected"))
    {
        Console.WriteLine("Error: No faces found in image");
        
        // Retry with lower confidence
        try
        {
            var retryOptions = ProcessingOptions.PivBalanced with { MinFaceConfidence = 0.6f };
            return await FacialImageEncoder.ProcessAsync(imageData, retryOptions);
        }
        catch
        {
            Console.WriteLine("Retry with lower confidence also failed");
            return null;
        }
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Multiple faces detected"))
    {
        Console.WriteLine("Error: Multiple faces detected, trying to use best face");
        
        // Allow multiple faces
        try
        {
            var multiOptions = ProcessingOptions.PivBalanced with { RequireSingleFace = false };
            return await FacialImageEncoder.ProcessAsync(imageData, multiOptions);
        }
        catch
        {
            Console.WriteLine("Multi-face processing also failed");
            return null;
        }
    }
    catch (TimeoutException ex)
    {
        Console.WriteLine($"Error: Processing timeout - {ex.Message}");
        return null;
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Error: Processing failed - {ex.Message}");
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
        return null;
    }
}
```

---

This completes the comprehensive API documentation for FaceOFFx. The library provides two distinct pathways: a simple exception-based API (`FacialImageEncoder`) for most developers, and a functional API (`PivProcessor`) for advanced scenarios requiring detailed error handling and full control over the processing pipeline.
