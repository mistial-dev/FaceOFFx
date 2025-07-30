# FaceOFFx Release Notes

## Version 1.0.0 - Initial Release

### Overview
FaceOFFx is a specialized facial processing library for .NET focused on PIV (Personal Identity Verification) compliance for government ID cards and credentials. This initial release provides complete PIV/TWIC-compliant facial image processing with advanced JPEG 2000 ROI encoding capabilities.

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

#### Simple API
- One-line conversion from JPEG to PIV-compliant JP2
- Sensible defaults (20KB, ROI level 3)
- Async/await support throughout
- Result<T> pattern for robust error handling

#### CLI Tool
- Global .NET tool for command-line processing
- Process command for PIV transformation
- ROI visualization command for quality inspection
- Configurable compression and ROI parameters

### Technical Details

#### Architecture
- Clean architecture with domain-driven design
- Direct ONNX Runtime integration (no Python dependencies)
- Functional programming patterns (Result<T>, Maybe<T>)
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

#### CLI Tool
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
- GitHub Issues: https://github.com/mistial-dev/FaceOFFx/issues
- Documentation: https://github.com/mistial-dev/FaceOFFx/wiki
- Samples: See docs/samples/ directory

### License
MIT License - See LICENSE file for details

---

*"I want to take his face... off." - Castor Troy, Face/Off (1997)*