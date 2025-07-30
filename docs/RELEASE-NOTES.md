# FaceOFFx Release Notes

## 2.0.0 (2025-07-30)

### Major Breaking Changes & Code Quality Improvements

This major release improves code quality by adopting functional programming patterns
internally while providing a simplified public API. The release also corrects 
terminology to reflect PIV-compatible (not PIV-compliant) image processing.

### Breaking Changes

#### 1. New Simplified Public API

- New `FacialImageEncoder` class provides standard .NET API without functional extensions
- Methods now throw exceptions instead of returning Result<T>
- Optional TryProcessAsync method available for non-throwing pattern
- Example migration:

  ```csharp
  // Old (internal API)
  var result = await FaceProcessor.ProcessAsync(imageData);
  if (result.IsSuccess)
  {
      File.WriteAllBytes("output.jp2", result.Value.ImageData);
  }
  
  // New (public API)
  var result = await FacialImageEncoder.ProcessAsync(imageData);
  File.WriteAllBytes("output.jp2", result.ImageData);
  ```

#### 2. Internal Improvements

- Internal code uses Maybe<T> pattern for optional values (not exposed publicly)
- Internal methods use Result<T> for error handling (wrapped at public boundaries)
- Improved error propagation and handling throughout the library

#### 3. Terminology Correction: PIV-Compatible

- All references to "PIV-compliant" changed to "PIV-compatible"
- This accurately reflects that the library produces images compatible with PIV standards
- No functional changes, documentation and naming only

### New Features

#### Command Line Enhancements

- Added `--preset` parameter for predefined quality settings
- Added `--target-size` parameter for specific file size targets
- Presets include: piv-high (30KB), piv-balanced (20KB), twic-max (14KB), piv-min (12KB)

### Bug Fixes

- Fixed misleading comment about transformation order (was "crop first, then rotate")
- Improved error handling throughout the codebase
- Better null safety with compile-time guarantees

### Code Quality Improvements

- Adopted CSharpFunctionalExtensions patterns consistently
- Replaced exception-based error handling with Result<T>
- Improved immutability and functional composition
- Better separation of concerns with pure functions

### Migration Guide

To migrate from 1.0.x to 2.0.0:

1. Update all calls to `PivResult.Success()` to use Maybe<T> for optional parameters
2. Handle Result<T> return values instead of try-catch blocks
3. Update any references from "PIV-compliant" to "PIV-compatible" in your code
4. Review ProcessCommand usage if using the CLI programmatically

### Dependencies

- Updated package dependencies to latest stable versions
- No new dependencies added

---

## 1.0.1 (2025-07-30)

### Package Consolidation & Bug Fixes

This patch release consolidates the NuGet package structure and fixes several
packaging and workflow issues discovered after the initial release.

### Package Structure

- Consolidated NuGet packages: All library assemblies (Core, Infrastructure,
  Models, Application) are now bundled into a single `FaceOFFx` package
- Simplified installation: Users now only need to install one package instead
  of multiple dependencies
- Two packages total: `FaceOFFx` (library) and `FaceOFFx.Cli` (command-line tool)

### Bug Fixes

- Fixed publish workflow to reference the renamed FaceOFFx project correctly
- Fixed README images for NuGet.org compatibility
- Fixed GitHub Actions workflows for proper build and release processes
- Removed local testing repository references
- Various documentation and unit test fixes

### Build & Release

- Updated release building workflow to handle new package structure
- Improved MSBuild targets for proper packaging
- Fixed workflow dependencies and build order

### Breaking Changes

None - The API remains unchanged. Users who installed individual packages should
uninstall them and install the consolidated `FaceOFFx` package instead.

### Migration from 1.0.0

```bash
# Remove old packages
dotnet remove package FaceOFFx.Core
dotnet remove package FaceOFFx.Infrastructure
dotnet remove package FaceOFFx.Application

# Install consolidated package
dotnet add package FaceOFFx
```

---

## 1.0.0 (2025-07-29)

### Initial Release

FaceOFFx is a specialized facial processing library for .NET focused on PIV
(Personal Identity Verification) compliance for government ID cards and
credentials. This initial release provides complete PIV/TWIC-compliant facial
image processing with advanced JPEG 2000 ROI encoding capabilities.

### Key Features

#### PIV Compliance

- FIPS 201-3 compliant facial image processing
- Automated 420×560 pixel output with proper face positioning
- Eye position alignment (55-60% from bottom)
- Minimum face width validation (240 pixels)
- Rotation correction (±5 degrees maximum)

#### Face Detection & Landmarks

- RetinaFace model for accurate face detection (MIT licensed)
- 68-point facial landmark detection using PFLD model
- Frontal face filtering for optimal results
- Multi-face rejection for security compliance

#### JPEG 2000 ROI Encoding

- Smart ROI (Region of Interest) compression for facial features
- Configurable compression rates targeting specific file sizes:
  - 0.6 bpp → ~17KB files
  - 0.7 bpp → ~20KB files (default)
  - 0.8 bpp → ~24KB files
  - 1.0 bpp → ~30KB files
- ROI priority levels (0-3) for quality distribution control
- Single-tile encoding for optimal compression efficiency

### Simple API

- One-line conversion from JPEG to PIV-compliant JP2
- Sensible defaults (20KB, ROI level 3)
- Async/await support throughout
- Result\<T> pattern for robust error handling

### CLI Tool

- Global .NET tool for command-line processing
- Process command for PIV transformation
- ROI visualization command for quality inspection
- Configurable compression and ROI parameters

### Technical Details

#### Architecture

- Clean architecture with domain-driven design
- Direct ONNX Runtime integration (no Python dependencies)
- Functional programming patterns (Result\<T>, Maybe\<T>)
- Zero nulls in domain models
- Minimal abstractions for simplicity

#### Performance

- CPU-based inference (GPU optional for Release builds)
- Efficient memory usage with streaming processing
- Single-image and batch processing support
- Model warmup recommended for production use

#### Dependencies

- .NET 8.0 or later
- Microsoft.ML.OnnxRuntime for model inference
- SixLabors.ImageSharp for image processing
- CoreJ2K for JPEG 2000 encoding
- CSharpFunctionalExtensions for functional patterns

### Installation

#### NuGet Package

```bash
dotnet add package FaceOFFx
```

### CLI Tool Installation

```bash
dotnet tool install -g FaceOFFx.Cli
```

### Usage Examples

#### Simple API

```csharp
var result = await PivProcessor.ConvertJpegToPivJp2Async(
    "input.jpg",
    "output.jp2", 
    faceDetector,
    landmarkExtractor,
    jpeg2000Encoder);
```

#### CLI

```bash
# Process with defaults (20KB, ROI enabled)
faceoffx process photo.jpg

# Custom compression rate
faceoffx process photo.jpg --rate 0.8 --roi-level 2

# Generate ROI visualization
faceoffx roi photo.jpg --show-piv-lines
```

### Known Limitations

- 68-point landmarks don't include true ear positions (uses jaw width)
- Single face required (multiple faces cause rejection)
- Maximum ±5 degree rotation correction
- JPEG 2000 encoding is CPU-intensive

### Standards Compliance

- FIPS 201-3: Personal Identity Verification (2022)
- INCITS 385-2004: Face Recognition Format for Data Interchange
- NIST SP 800-76-2: Biometric Specifications for PIV
- TWIC Next Generation specifications

### Acknowledgments

This project is derived from the excellent FaceONNX library and incorporates:

- RetinaFace model for face detection (MIT)
- PFLD 68-point landmark model (MIT)
- CoreJ2K for JPEG 2000 support (BSD-2-Clause)

### Support

- GitHub Issues: <https://github.com/mistial-dev/FaceOFFx/issues>
- Documentation: <https://github.com/mistial-dev/FaceOFFx/wiki>
- Samples: See docs/samples/ directory

### License

MIT License - See LICENSE file for details

---

### Quote

"I want to take his face... off." - Castor Troy, Face/Off (1997)
