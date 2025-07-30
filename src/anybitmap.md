Class AnyBitmap
A universally compatible Bitmap format for .NET 7, .NET 6, .NET 5, and .NET Core. As well as compatibility with Windows, NanoServer, IIS, macOS, Mobile, Xamarin, iOS, Android, Google Cloud, Azure, AWS, and Linux.

Works nicely with popular Image and Bitmap formats such as System.Drawing.Bitmap, SkiaSharp, SixLabors.ImageSharp, Microsoft.Maui.Graphics.

Implicit casting means that using this class to input and output Bitmap and image types from public API's gives full compatibility to all image type fully supported by Microsoft.

When casting to and from AnyBitmap, please remember to dispose your original Bitmap object (e.g. System.Drawing.Bitmap) to avoid unnecessary memory allocation.

Unlike System.Drawing.Bitmap this bitmap object is self-memory-managing and does not need to be explicitly 'used' or 'disposed'.

Inheritance
System.Object
AnyBitmap
Implements
System.IDisposable
IronSoftware.IAnyImage
System.ICloneable
Namespace: IronSoftware.Drawing
Assembly: IronSoftware.Drawing.Common.dll
Syntax
public class AnyBitmap : Object, IDisposable, IAnyImage, ICloneable
Constructors
AnyBitmap(AnyBitmap, Int32, Int32)
Declaration
public AnyBitmap(AnyBitmap original, int width, int height)
Parameters
Type	Name	Description
AnyBitmap	original	
The AnyBitmap from which to create the new AnyBitmap.

System.Int32	width	
The width of the new AnyBitmap.

System.Int32	height	
The height of the new AnyBitmap.

AnyBitmap(Byte[])
Construct a new Bitmap from binary data (bytes).

Declaration
public AnyBitmap(byte[] bytes)
Parameters
Type	Name	Description
System.Byte[]	bytes	
A ByteArray of image data in any common format.

See Also
FromBytes(Byte[])
AnyBitmap
AnyBitmap(Byte[], Boolean)
Construct a new Bitmap out of binary data with a byte array.

Declaration
public AnyBitmap(byte[] bytes, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.Byte[]	bytes	
A byte array of image data in any common format.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

See Also
FromBytes(Byte[], Boolean)
AnyBitmap
AnyBitmap(Int32, Int32, Color)
Construct a new Bitmap from width and height.

Declaration
public AnyBitmap(int width, int height, Color backgroundColor = null)
Parameters
Type	Name	Description
System.Int32	width	
Width of new AnyBitmap

System.Int32	height	
Height of new AnyBitmap

Color	backgroundColor	
Background color of new AnyBitmap

AnyBitmap(MemoryStream)
Construct a new Bitmap from a System.IO.Stream (bytes).

Declaration
public AnyBitmap(MemoryStream stream)
Parameters
Type	Name	Description
System.IO.MemoryStream	stream	
A System.IO.Stream of image data in any common format.

See Also
FromStream(Stream, Boolean)
AnyBitmap
AnyBitmap(MemoryStream, Boolean)
Construct a new Bitmap from a System.IO.Stream (bytes).

Declaration
public AnyBitmap(MemoryStream stream, bool preserveOriginalFormat = true)
Parameters
Type	Name	Description
System.IO.MemoryStream	stream	
A System.IO.Stream of image data in any common format.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

See Also
FromStream(Stream, Boolean)
AnyBitmap
AnyBitmap(Stream)
Construct a new Bitmap from a System.IO.Stream (bytes).

Declaration
public AnyBitmap(Stream stream)
Parameters
Type	Name	Description
System.IO.Stream	stream	
A System.IO.Stream of image data in any common format.

See Also
FromStream(MemoryStream, Boolean)
AnyBitmap
AnyBitmap(Stream, Boolean)
Construct a new Bitmap from a System.IO.Stream (bytes).

Declaration
public AnyBitmap(Stream stream, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.IO.Stream	stream	
A System.IO.Stream of image data in any common format.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

See Also
FromStream(MemoryStream, Boolean)
AnyBitmap
AnyBitmap(ReadOnlySpan<Byte>)
Construct a new Bitmap from binary data (byte span).

Declaration
public AnyBitmap(ReadOnlySpan<byte> span)
Parameters
Type	Name	Description
System.ReadOnlySpan<System.Byte>	span	
A byte span of image data in any common format.

See Also
AnyBitmap
AnyBitmap(ReadOnlySpan<Byte>, Boolean)
Construct a new Bitmap out of binary data with a byte span.

Declaration
public AnyBitmap(ReadOnlySpan<byte> span, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.ReadOnlySpan<System.Byte>	span	
A byte span of image data in any common format.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

See Also
AnyBitmap
AnyBitmap(String)
Construct a new Bitmap from a file.

Declaration
public AnyBitmap(string file)
Parameters
Type	Name	Description
System.String	file	
A fully qualified file path./

See Also
FromFile(String)
AnyBitmap
AnyBitmap(String, Boolean)
Construct a new Bitmap from a file.

Declaration
public AnyBitmap(string file, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.String	file	
A fully qualified file path./

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

See Also
FromFile(String, Boolean)
AnyBitmap
AnyBitmap(Uri)
Construct a new Bitmap from a Uri.

Declaration
public AnyBitmap(Uri uri)
Parameters
Type	Name	Description
System.Uri	uri	
The uri of the image.

See Also
FromUriAsync(Uri)
AnyBitmap
AnyBitmap(Uri, Boolean)
Construct a new Bitmap from a Uri.

Declaration
public AnyBitmap(Uri uri, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.Uri	uri	
The uri of the image.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

See Also
FromUriAsync(Uri, Boolean)
AnyBitmap
Properties
BitsPerPixel
Gets colors depth, in number of bits per pixel.

Further Documentation:
Code Example

Declaration
public int BitsPerPixel { get; }
Property Value
Type	Description
System.Int32	
FrameCount
Returns the number of frames in our loaded Image. Each “frame” is a page of an image such as Tiff or Gif. All other image formats return 1.

Further Documentation:
Code Example

Declaration
public int FrameCount { get; }
Property Value
Type	Description
System.Int32	
See Also
GetAllFrames
GetAllFrames
Returns all of the cloned frames in our loaded Image. Each "frame" is a page of an image such as Tiff or Gif. All other image formats return an IEnumerable of length 1.

Further Documentation:
Code Example

Declaration
public IEnumerable<AnyBitmap> GetAllFrames { get; }
Property Value
Type	Description
System.Collections.Generic.IEnumerable<AnyBitmap>	
See Also
FrameCount
System.Linq
Height
Height of the image.

Declaration
public int Height { get; }
Property Value
Type	Description
System.Int32	
HorizontalResolution
Gets the resolution of the image in x-direction.

Declaration
public Nullable<double> HorizontalResolution { get; }
Property Value
Type	Description
System.Nullable<System.Double>	
Length
Number of raw image bytes stored

Declaration
public int Length { get; }
Property Value
Type	Description
System.Int32	
MimeType
Returns the HTTP MIME types of the image.

must be one of the following: image/bmp, image/jpeg, image/png, image/gif, image/tiff, image/webp, or image/unknown.

Declaration
public string MimeType { get; }
Property Value
Type	Description
System.String	
Scan0
Gets the address of the first pixel data in the AnyBitmap. This can also be thought of as the first scan line in the AnyBitmap.

Declaration
public IntPtr Scan0 { get; }
Property Value
Type	Description
System.IntPtr	
The address of the first 32bpp BGRA pixel data in the AnyBitmap.

Stride
Gets the stride width (also called scan width) of the AnyBitmap object.

Declaration
public int Stride { get; }
Property Value
Type	Description
System.Int32	
VerticalResolution
Gets the resolution of the image in y-direction.

Declaration
public Nullable<double> VerticalResolution { get; }
Property Value
Type	Description
System.Nullable<System.Double>	
Width
Width of the image.

Declaration
public int Width { get; }
Property Value
Type	Description
System.Int32	
Methods
Clone()
Creates an exact duplicate AnyBitmap

Further Documentation:
Code Example

Declaration
public AnyBitmap Clone()
Returns
Type	Description
AnyBitmap	
Clone(Rectangle)
Creates an exact duplicate AnyBitmap of the cropped area.

Further Documentation:
Code Example

Declaration
public AnyBitmap Clone(Rectangle rectangle)
Parameters
Type	Name	Description
Rectangle	rectangle	
Defines the portion of this AnyBitmap to copy.

Returns
Type	Description
AnyBitmap	
CreateMultiFrameGif(IEnumerable<AnyBitmap>)
Creates a multi-frame GIF image from multiple AnyBitmaps.

All images should have the same dimension.

If not dimension will be scaling to the largest width and height.

The image dimension still the same with original dimension with background transparent.

Declaration
public static AnyBitmap CreateMultiFrameGif(IEnumerable<AnyBitmap> images)
Parameters
Type	Name	Description
System.Collections.Generic.IEnumerable<AnyBitmap>	images	
Array of AnyBitmap to merge into Gif image.

Returns
Type	Description
AnyBitmap	
CreateMultiFrameGif(IEnumerable<String>)
Creates a multi-frame GIF image from multiple AnyBitmaps.

All images should have the same dimension.

If not dimension will be scaling to the largest width and height.

The image dimension still the same with original dimension with background transparent.

Declaration
public static AnyBitmap CreateMultiFrameGif(IEnumerable<string> imagePaths)
Parameters
Type	Name	Description
System.Collections.Generic.IEnumerable<System.String>	imagePaths	
Array of fully qualified file path to merge into Gif image.

Returns
Type	Description
AnyBitmap	
CreateMultiFrameTiff(IEnumerable<AnyBitmap>)
Creates a multi-frame TIFF image from multiple AnyBitmaps.

All images should have the same dimension.

If not dimension will be scaling to the largest width and height.

The image dimension still the same with original dimension with black background.

Declaration
public static AnyBitmap CreateMultiFrameTiff(IEnumerable<AnyBitmap> images)
Parameters
Type	Name	Description
System.Collections.Generic.IEnumerable<AnyBitmap>	images	
Array of AnyBitmap to merge into Tiff image.

Returns
Type	Description
AnyBitmap	
CreateMultiFrameTiff(IEnumerable<String>)
Creates a multi-frame TIFF image from multiple AnyBitmaps.

All images should have the same dimension.

If not dimension will be scaling to the largest width and height.

The image dimension still the same with original dimension with black background.

Declaration
public static AnyBitmap CreateMultiFrameTiff(IEnumerable<string> imagePaths)
Parameters
Type	Name	Description
System.Collections.Generic.IEnumerable<System.String>	imagePaths	
Array of fully qualified file path to merge into Tiff image.

Returns
Type	Description
AnyBitmap	
Dispose()
Releases all resources used by this AnyBitmap.

Declaration
public void Dispose()
Dispose(Boolean)
Releases all resources used by this AnyBitmap.

Declaration
protected virtual void Dispose(bool disposing)
Parameters
Type	Name	Description
System.Boolean	disposing	
ExportBytes(AnyBitmap.ImageFormat, Int32)
Exports the Bitmap as bytes encoded in the AnyBitmap.ImageFormat of your choice.

Add SkiaSharp, System.Drawing.Common, or SixLabors.ImageSharp to your project to enable this feature.

Declaration
public byte[] ExportBytes(AnyBitmap.ImageFormat format, int lossy = 100)
Parameters
Type	Name	Description
AnyBitmap.ImageFormat	format	
An image encoding format.

System.Int32	lossy	
JPEG and WebP encoding quality (ignored for all other values of AnyBitmap.ImageFormat). Higher values return larger file sizes. 0 is lowest quality , 100 is highest.

Returns
Type	Description
System.Byte[]	
Transcoded image bytes.

ExportBytesAsJpg()
Declaration
public byte[] ExportBytesAsJpg()
Returns
Type	Description
System.Byte[]	
ExportFile(String, AnyBitmap.ImageFormat, Int32)
Exports the Bitmap as a file encoded in the AnyBitmap.ImageFormat of your choice.

Add SkiaSharp, System.Drawing.Common, or SixLabors.ImageSharp to your project to enable the encoding feature.

Further Documentation:
Code Example

Declaration
public void ExportFile(string file, AnyBitmap.ImageFormat format, int lossy = 100)
Parameters
Type	Name	Description
System.String	file	
A fully qualified file path.

AnyBitmap.ImageFormat	format	
An image encoding format.

System.Int32	lossy	
JPEG and WebP encoding quality (ignored for all other values of AnyBitmap.ImageFormat). Higher values return larger file sizes. 0 is lowest quality, 100 is highest.

ExportStream(Stream, AnyBitmap.ImageFormat, Int32)
Saves the Bitmap to an existing System.IO.Stream encoded in the AnyBitmap.ImageFormat of your choice.

Add SkiaSharp, System.Drawing.Common, or SixLabors.ImageSharp to your project to enable the encoding feature.

Declaration
public void ExportStream(Stream stream, AnyBitmap.ImageFormat format, int lossy = 100)
Parameters
Type	Name	Description
System.IO.Stream	stream	
An image encoding format.

AnyBitmap.ImageFormat	format	
An image encoding format.

System.Int32	lossy	
JPEG and WebP encoding quality (ignored for all other values of AnyBitmap.ImageFormat). Higher values return larger file sizes. 0 is lowest quality, 100 is highest.

ExtractAlphaData()
Extracts the alpha channel data from an image.

Declaration
public byte[] ExtractAlphaData()
Returns
Type	Description
System.Byte[]	
An array of bytes representing the alpha values of the image's pixels.

Exceptions
Type	Condition
System.NotSupportedException	
Thrown when the image's bit depth is not 32 bpp.

Finalize()
AnyBitmap destructor

Declaration
protected override void Finalize()
FromBitmap<T>(T)
Generic method to convert popular image types to AnyBitmap.

Support includes SixLabors.ImageSharp.Image, SkiaSharp.SKImage, SkiaSharp.SKBitmap, System.Drawing.Bitmap, System.Drawing.Image and Microsoft.Maui.Graphics formats.

Syntax sugar. Explicit casts already also exist to and from AnyBitmap and all supported types.

Declaration
public static AnyBitmap FromBitmap<T>(T otherBitmapFormat)
Parameters
Type	Name	Description
T	otherBitmapFormat	
A bitmap or image format from another graphics library.

Returns
Type	Description
AnyBitmap	
A AnyBitmap

Type Parameters
Name	Description
T	
The Type to cast from. Support includes SixLabors.ImageSharp.Image, SkiaSharp.SKImage, SkiaSharp.SKBitmap, System.Drawing.Bitmap, System.Drawing.Image and Microsoft.Maui.Graphics formats.

FromBytes(Byte[])
Create a new Bitmap from a a byte array.

Declaration
public static AnyBitmap FromBytes(byte[] bytes)
Parameters
Type	Name	Description
System.Byte[]	bytes	
A byte array of image data in any common format.

Returns
Type	Description
AnyBitmap	
FromBytes(Byte[], Boolean)
Create a new Bitmap from a a byte array.

Declaration
public static AnyBitmap FromBytes(byte[] bytes, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.Byte[]	bytes	
A byte array of image data in any common format.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

Returns
Type	Description
AnyBitmap	
FromFile(String)
Create a new Bitmap from a file.

Declaration
public static AnyBitmap FromFile(string file)
Parameters
Type	Name	Description
System.String	file	
A fully qualified file path.

Returns
Type	Description
AnyBitmap	
See Also
FromFile(String)
AnyBitmap
FromFile(String, Boolean)
Create a new Bitmap from a file.

Declaration
public static AnyBitmap FromFile(string file, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.String	file	
A fully qualified file path.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

Returns
Type	Description
AnyBitmap	
See Also
FromFile(String, Boolean)
AnyBitmap
FromSpan(ReadOnlySpan<Byte>)
Create a new Bitmap from a a Byte Span.

Declaration
public static AnyBitmap FromSpan(ReadOnlySpan<byte> span)
Parameters
Type	Name	Description
System.ReadOnlySpan<System.Byte>	span	
A Byte Span of image data in any common format.

Returns
Type	Description
AnyBitmap	
FromSpan(ReadOnlySpan<Byte>, Boolean)
Create a new Bitmap from a a byte span.

Declaration
public static AnyBitmap FromSpan(ReadOnlySpan<byte> span, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.ReadOnlySpan<System.Byte>	span	
A byte span of image data in any common format.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

Returns
Type	Description
AnyBitmap	
FromStream(MemoryStream)
Declaration
public static AnyBitmap FromStream(MemoryStream stream)
Parameters
Type	Name	Description
System.IO.MemoryStream	stream	
Returns
Type	Description
AnyBitmap	
FromStream(MemoryStream, Boolean)
Create a new Bitmap from a System.IO.Stream (bytes).

Declaration
public static AnyBitmap FromStream(MemoryStream stream, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.IO.MemoryStream	stream	
A System.IO.Stream of image data in any common format.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

Returns
Type	Description
AnyBitmap	
See Also
FromStream(Stream, Boolean)
AnyBitmap
FromStream(Stream)
Create a new Bitmap from a System.IO.Stream (bytes).

Declaration
public static AnyBitmap FromStream(Stream stream)
Parameters
Type	Name	Description
System.IO.Stream	stream	
A System.IO.Stream of image data in any common format.

Returns
Type	Description
AnyBitmap	
See Also
FromStream(MemoryStream, Boolean)
AnyBitmap
FromStream(Stream, Boolean)
Create a new Bitmap from a System.IO.Stream (bytes).

Declaration
public static AnyBitmap FromStream(Stream stream, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.IO.Stream	stream	
A System.IO.Stream of image data in any common format.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

Returns
Type	Description
AnyBitmap	
See Also
FromStream(MemoryStream, Boolean)
AnyBitmap
FromUri(Uri)
Construct a new Bitmap from a Uri

Declaration
public static AnyBitmap FromUri(Uri uri)
Parameters
Type	Name	Description
System.Uri	uri	
The uri of the image.

Returns
Type	Description
AnyBitmap	
See Also
AnyBitmap
FromUriAsync(Uri)
FromUri(Uri, Boolean)
Construct a new Bitmap from a Uri.

Declaration
public static AnyBitmap FromUri(Uri uri, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.Uri	uri	
The uri of the image.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

Returns
Type	Description
AnyBitmap	
See Also
AnyBitmap
FromUriAsync(Uri, Boolean)
FromUriAsync(Uri)
Construct a new Bitmap from a Uri.

Declaration
public static Task<AnyBitmap> FromUriAsync(Uri uri)
Parameters
Type	Name	Description
System.Uri	uri	
The uri of the image.

Returns
Type	Description
System.Threading.Tasks.Task<AnyBitmap>	
See Also
AnyBitmap
FromUri(Uri)
FromUriAsync(Uri)
FromUriAsync(Uri, Boolean)
Construct a new Bitmap from a Uri.

Declaration
public static Task<AnyBitmap> FromUriAsync(Uri uri, bool preserveOriginalFormat)
Parameters
Type	Name	Description
System.Uri	uri	
The uri of the image.

System.Boolean	preserveOriginalFormat	
Determine whether to load SixLabors.ImageSharp.Image as its original pixel format or Rgba32.

Returns
Type	Description
System.Threading.Tasks.Task<AnyBitmap>	
See Also
AnyBitmap
FromUri(Uri, Boolean)
FromUriAsync(Uri, Boolean)
GetBytes()
The raw image data as byte[] (ByteArray)"/>

Declaration
public byte[] GetBytes()
Returns
Type	Description
System.Byte[]	
A byte[] (ByteArray)

GetHashCode()
Hashing integer based on image raw binary data.

Declaration
public override int GetHashCode()
Returns
Type	Description
System.Int32	
Int

GetImageFormat()
Image formats which AnyBitmap readed.

Declaration
public AnyBitmap.ImageFormat GetImageFormat()
Returns
Type	Description
AnyBitmap.ImageFormat	
AnyBitmap.ImageFormat

GetPixel(Int32, Int32)
Gets the Color of the specified pixel in this AnyBitmap

This always return an Rgba32 color format.

Declaration
public Color GetPixel(int x, int y)
Parameters
Type	Name	Description
System.Int32	x	
The x-coordinate of the pixel to retrieve.

System.Int32	y	
The y-coordinate of the pixel to retrieve.

Returns
Type	Description
Color	
A Color structure that represents the color of the specified pixel.

GetRGBBuffer()
Retrieves the RGB buffer from the image at the specified path.

Declaration
public byte[] GetRGBBuffer()
Returns
Type	Description
System.Byte[]	
An array of bytes representing the RGB buffer of the image.

Remarks
Each pixel is represented by three bytes in the order: red, green, blue. The pixels are read from the image row by row, from top to bottom and left to right within each row.

GetStream()
The raw image data as a System.IO.MemoryStream

Further Documentation:
Code Example

Declaration
public MemoryStream GetStream()
Returns
Type	Description
System.IO.MemoryStream	
System.IO.MemoryStream

LoadAnyBitmapFromRGBBuffer(Byte[], Int32, Int32)
Creates an AnyBitmap object from a buffer of RGB pixel data.

Declaration
public static AnyBitmap LoadAnyBitmapFromRGBBuffer(byte[] buffer, int width, int height)
Parameters
Type	Name	Description
System.Byte[]	buffer	
An array of bytes representing the RGB pixel data. This should contain 3 bytes (one each for red, green, and blue) for each pixel in the image.

System.Int32	width	
The width of the image, in pixels.

System.Int32	height	
The height of the image, in pixels.

Returns
Type	Description
AnyBitmap	
An AnyBitmap object that represents the image defined by the provided pixel data, width, and height.

Redact(AnyBitmap, Rectangle, Color)
Creates a new bitmap with the region defined by the specified rectangle in the specified bitmap redacted with the specified color.

Declaration
public static AnyBitmap Redact(AnyBitmap bitmap, Rectangle Rectangle, Color color)
Parameters
Type	Name	Description
AnyBitmap	bitmap	
The bitmap to redact.

Rectangle	Rectangle	
The rectangle defining the region to redact.

Color	color	
The color to use for redaction.

Returns
Type	Description
AnyBitmap	
A new bitmap with the specified region redacted.

Redact(Rectangle, Color)
Creates a new bitmap with the region defined by the specified rectangle redacted with the specified color.

Declaration
public AnyBitmap Redact(Rectangle Rectangle, Color color)
Parameters
Type	Name	Description
Rectangle	Rectangle	
The rectangle defining the region to redact.

Color	color	
The color to use for redaction.

Returns
Type	Description
AnyBitmap	
A new bitmap with the specified region redacted.

RotateFlip(AnyBitmap, AnyBitmap.RotateMode, AnyBitmap.FlipMode)
Specifies how much an image is rotated and the axis used to flip the image.

Declaration
public static AnyBitmap RotateFlip(AnyBitmap bitmap, AnyBitmap.RotateMode rotateMode, AnyBitmap.FlipMode flipMode)
Parameters
Type	Name	Description
AnyBitmap	bitmap	
The AnyBitmap to perform the transformation on.

AnyBitmap.RotateMode	rotateMode	
Provides enumeration over how the image should be rotated.

AnyBitmap.FlipMode	flipMode	
Provides enumeration over how a image should be flipped.

Returns
Type	Description
AnyBitmap	
Transformed image

RotateFlip(AnyBitmap.RotateFlipType)
Specifies how much an AnyBitmap is rotated and the axis used to flip the image.

Declaration
public AnyBitmap RotateFlip(AnyBitmap.RotateFlipType rotateFlipType)
Parameters
Type	Name	Description
AnyBitmap.RotateFlipType	rotateFlipType	
Provides enumeration over how the image should be rotated.

Returns
Type	Description
AnyBitmap	
Transformed image

RotateFlip(AnyBitmap.RotateMode, AnyBitmap.FlipMode)
Specifies how much an AnyBitmap is rotated and the axis used to flip the image.

Declaration
public AnyBitmap RotateFlip(AnyBitmap.RotateMode rotateMode, AnyBitmap.FlipMode flipMode)
Parameters
Type	Name	Description
AnyBitmap.RotateMode	rotateMode	
Provides enumeration over how the image should be rotated.

AnyBitmap.FlipMode	flipMode	
Provides enumeration over how a image should be flipped.

Returns
Type	Description
AnyBitmap	
Transformed image

SaveAs(String)
Saves the raw image data to a file.

Declaration
public void SaveAs(string file)
Parameters
Type	Name	Description
System.String	file	
A fully qualified file path.

See Also
TrySaveAs(String)
SaveAs(String, AnyBitmap.ImageFormat, Int32)
Saves the image data to a file. Allows for the image to be transcoded to popular image formats.

Add SkiaSharp, System.Drawing.Common, or SixLabors.ImageSharp to your project to enable the encoding feature.

Declaration
public void SaveAs(string file, AnyBitmap.ImageFormat format, int lossy = 100)
Parameters
Type	Name	Description
System.String	file	
A fully qualified file path.

AnyBitmap.ImageFormat	format	
An image encoding format.

System.Int32	lossy	
JPEG and WebP encoding quality (ignored for all other values of AnyBitmap.ImageFormat). Higher values return larger file sizes. 0 is lowest quality , 100 is highest.

See Also
TrySaveAs(String, AnyBitmap.ImageFormat, Int32)
TrySaveAs(String)
SetPixel(Int32, Int32, Color)
Sets the Color of the specified pixel in this AnyBitmap

Set in Rgb24 color format.

Declaration
public void SetPixel(int x, int y, Color color)
Parameters
Type	Name	Description
System.Int32	x	
The x-coordinate of the pixel to retrieve.

System.Int32	y	
The y-coordinate of the pixel to retrieve.

Color	color	
The color to set the pixel.

ToBitmap<T>()
Generic method to convert AnyBitmap to popular image types.

Support includes SixLabors.ImageSharp.Image, SkiaSharp.SKImage, SkiaSharp.SKBitmap, System.Drawing.Bitmap, System.Drawing.Image and Microsoft.Maui.Graphics formats.

Syntax sugar. Explicit casts already also exist to and from AnyBitmap and all supported types.

Declaration
public T ToBitmap<T>()
Returns
Type	Description
T	
A AnyBitmap

Type Parameters
Name	Description
T	
The Type to cast to. Support includes SixLabors.ImageSharp.Image, SkiaSharp.SKImage, SkiaSharp.SKBitmap, System.Drawing.Bitmap, System.Drawing.Image and Microsoft.Maui.Graphics formats.

ToStream(AnyBitmap.ImageFormat, Int32)
Exports the Bitmap as a System.IO.MemoryStream encoded in the AnyBitmap.ImageFormat of your choice.

Add SkiaSharp, System.Drawing.Common, or SixLabors.ImageSharp to your project to enable the encoding feature.

Further Documentation:
Code Example

Declaration
public MemoryStream ToStream(AnyBitmap.ImageFormat format, int lossy = 100)
Parameters
Type	Name	Description
AnyBitmap.ImageFormat	format	
An image encoding format.

System.Int32	lossy	
JPEG and WebP encoding quality (ignored for all other values of AnyBitmap.ImageFormat). Higher values return larger file sizes. 0 is lowest quality, 100 is highest.

Returns
Type	Description
System.IO.MemoryStream	
Transcoded image bytes in a System.IO.MemoryStream.

ToStreamFn(AnyBitmap.ImageFormat, Int32)
Exports the Bitmap as a FuncSystem.IO.MemoryStream> encoded in the AnyBitmap.ImageFormat of your choice.

Add SkiaSharp, System.Drawing.Common, or SixLabors.ImageSharp to your project to enable the encoding feature.

Declaration
public Func<Stream> ToStreamFn(AnyBitmap.ImageFormat format, int lossy = 100)
Parameters
Type	Name	Description
AnyBitmap.ImageFormat	format	
An image encoding format.

System.Int32	lossy	
JPEG and WebP encoding quality (ignored for all other values of AnyBitmap.ImageFormat). Higher values return larger file sizes. 0 is lowest quality, 100 is highest.

Returns
Type	Description
System.Func<System.IO.Stream>	
Transcoded image bytes in a Func System.IO.MemoryStream

ToString()
A Base64 encoded string representation of the raw image binary data.

Further Documentation:
Code Example

Declaration
public override string ToString()
Returns
Type	Description
System.String	
The bitmap data as a Base64 string.

See Also
System.Convert.ToBase64String(System.Byte[])
TrySaveAs(String)
Tries to Save the raw image data to a file. returns true on success, false on failure.

Declaration
public bool TrySaveAs(string file)
Parameters
Type	Name	Description
System.String	file	
A fully qualified file path.

Returns
Type	Description
System.Boolean	
See Also
SaveAs(String)
TrySaveAs(String, AnyBitmap.ImageFormat, Int32)
Tries to Save the image data to a file. Allows for the image to be transcoded to popular image formats.

Add SkiaSharp, System.Drawing.Common, or SixLabors.ImageSharp to your project to enable the encoding feature.

Declaration
public bool TrySaveAs(string file, AnyBitmap.ImageFormat format, int lossy = 100)
Parameters
Type	Name	Description
System.String	file	
A fully qualified file path.

AnyBitmap.ImageFormat	format	
An image encoding format.

System.Int32	lossy	
JPEG and WebP encoding quality (ignored for all other values of AnyBitmap.ImageFormat). Higher values return larger file sizes. 0 is lowest quality , 100 is highest.

Returns
Type	Description
System.Boolean	
returns true on success, false on failure.

See Also
SaveAs(String, AnyBitmap.ImageFormat, Int32)
Operators
Implicit(AnyBitmap to PlatformImage)
Implicitly casts to Microsoft.Maui.Graphics.Platform.PlatformImage objects from AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support Microsoft.Maui.Graphics as well.

When casting to and from AnyBitmap, please remember to dispose your original IronSoftware.Drawing.AnyBitmap object to avoid unnecessary memory allocation.

Declaration
public static implicit operator PlatformImage(AnyBitmap bitmap)
Parameters
Type	Name	Description
AnyBitmap	bitmap	
AnyBitmap is implicitly cast to a Microsoft.Maui.Graphics.Platform.PlatformImage.

Returns
Type	Description
Microsoft.Maui.Graphics.Platform.PlatformImage	
Implicit(AnyBitmap to Image)
Implicitly casts to SixLabors.ImageSharp.Image objects from AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support ImageSharp as well.

When casting to and from AnyBitmap, please remember to dispose your original IronSoftware.Drawing.AnyBitmap object to avoid unnecessary memory allocation.

Declaration
public static implicit operator Image(AnyBitmap bitmap)
Parameters
Type	Name	Description
AnyBitmap	bitmap	
AnyBitmap is implicitly cast to a SixLabors.ImageSharp.Image.

Returns
Type	Description
SixLabors.ImageSharp.Image	
Implicit(AnyBitmap to Image<Rgb24>)
Implicitly casts to SixLabors.ImageSharp.Image objects from AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support ImageSharp as well.

When casting to and from AnyBitmap, please remember to dispose your original IronSoftware.Drawing.AnyBitmap object to avoid unnecessary memory allocation.

Declaration
public static implicit operator Image<Rgb24>(AnyBitmap bitmap)
Parameters
Type	Name	Description
AnyBitmap	bitmap	
AnyBitmap is implicitly cast to a SixLabors.ImageSharp.Image.

Returns
Type	Description
SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgb24>	
Implicit(AnyBitmap to Image<Rgba32>)
Implicitly casts to SixLabors.ImageSharp.Image objects from AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support ImageSharp as well.

When casting to and from AnyBitmap, please remember to dispose your original IronSoftware.Drawing.AnyBitmap object to avoid unnecessary memory allocation.

Declaration
public static implicit operator Image<Rgba32>(AnyBitmap bitmap)
Parameters
Type	Name	Description
AnyBitmap	bitmap	
AnyBitmap is implicitly cast to a SixLabors.ImageSharp.Image.

Returns
Type	Description
SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>	
Implicit(AnyBitmap to SKBitmap)
Implicitly casts to SkiaSharp.SKBitmap objects from AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support SkiaSharp.SKBitmap as well.

When casting to and from AnyBitmap, please remember to dispose your original IronSoftware.Drawing.AnyBitmap object to avoid unnecessary memory allocation.

Declaration
public static implicit operator SKBitmap(AnyBitmap bitmap)
Parameters
Type	Name	Description
AnyBitmap	bitmap	
AnyBitmap is explicitly cast to a SkiaSharp.SKBitmap.

Returns
Type	Description
SkiaSharp.SKBitmap	
Implicit(AnyBitmap to SKImage)
Implicitly casts to SkiaSharp.SKImage objects from AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support SkiaSharp.SKImage as well.

When casting to and from AnyBitmap, please remember to dispose your original IronSoftware.Drawing.AnyBitmap object to avoid unnecessary memory allocation.

Declaration
public static implicit operator SKImage(AnyBitmap bitmap)
Parameters
Type	Name	Description
AnyBitmap	bitmap	
AnyBitmap is implicitly cast to a SkiaSharp.SKImage.

Returns
Type	Description
SkiaSharp.SKImage	
Implicit(AnyBitmap to Bitmap)
Implicitly casts to System.Drawing.Bitmap objects from AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support System.Drawing.Common as well.

When casting to and from AnyBitmap, please remember to dispose your original IronSoftware.Drawing.AnyBitmap object to avoid unnecessary memory allocation.

Declaration
public static implicit operator Bitmap(AnyBitmap bitmap)
Parameters
Type	Name	Description
AnyBitmap	bitmap	
AnyBitmap is implicitly cast to a System.Drawing.Bitmap.

Returns
Type	Description
System.Drawing.Bitmap	
Implicit(AnyBitmap to Image)
Implicitly casts to System.Drawing.Image objects from AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support System.Drawing.Common as well.

When casting to and from AnyBitmap, please remember to dispose your original IronSoftware.Drawing.AnyBitmap object to avoid unnecessary memory allocation.

Declaration
public static implicit operator Image(AnyBitmap bitmap)
Parameters
Type	Name	Description
AnyBitmap	bitmap	
AnyBitmap is implicitly cast to a System.Drawing.Image.

Returns
Type	Description
System.Drawing.Image	
Implicit(PlatformImage to AnyBitmap)
Implicitly casts Microsoft.Maui.Graphics.Platform.PlatformImage objects to AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support Microsoft.Maui.Graphics as well.

When casting to and from AnyBitmap, please remember to dispose your original Microsoft.Maui.Graphics.Platform.PlatformImage object to avoid unnecessary memory allocation.

Declaration
public static implicit operator AnyBitmap(PlatformImage image)
Parameters
Type	Name	Description
Microsoft.Maui.Graphics.Platform.PlatformImage	image	
Microsoft.Maui.Graphics.Platform.PlatformImage will automatically be casted to AnyBitmap.

Returns
Type	Description
AnyBitmap	
Implicit(Image to AnyBitmap)
Implicitly casts SixLabors.ImageSharp.Image objects to AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support ImageSharp as well.

When casting to and from AnyBitmap, please remember to dispose your original SixLabors.ImageSharp.Image object to avoid unnecessary memory allocation.

Declaration
public static implicit operator AnyBitmap(Image image)
Parameters
Type	Name	Description
SixLabors.ImageSharp.Image	image	
SixLabors.ImageSharp.Image will automatically be casted to AnyBitmap.

Returns
Type	Description
AnyBitmap	
Implicit(Image<Rgb24> to AnyBitmap)
Implicitly casts SixLabors.ImageSharp.Image objects to AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support ImageSharp as well.

When casting to and from AnyBitmap, please remember to dispose your original SixLabors.ImageSharp.Image object to avoid unnecessary memory allocation.

Declaration
public static implicit operator AnyBitmap(Image<Rgb24> image)
Parameters
Type	Name	Description
SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgb24>	image	
SixLabors.ImageSharp.Image will automatically be casted to AnyBitmap.

Returns
Type	Description
AnyBitmap	
Implicit(Image<Rgba32> to AnyBitmap)
Implicitly casts SixLabors.ImageSharp.Image objects to AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support ImageSharp as well.

When casting to and from AnyBitmap, please remember to dispose your original SixLabors.ImageSharp.Image object to avoid unnecessary memory allocation.

Declaration
public static implicit operator AnyBitmap(Image<Rgba32> image)
Parameters
Type	Name	Description
SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>	image	
SixLabors.ImageSharp.Image will automatically be cast to AnyBitmap.

Returns
Type	Description
AnyBitmap	
Implicit(SKBitmap to AnyBitmap)
Implicitly casts SkiaSharp.SKBitmap objects to AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support SkiaSharp as well.

When casting to and from AnyBitmap, please remember to dispose your original SkiaSharp.SKBitmap object to avoid unnecessary memory allocation.

Declaration
public static implicit operator AnyBitmap(SKBitmap image)
Parameters
Type	Name	Description
SkiaSharp.SKBitmap	image	
SkiaSharp.SKBitmap will automatically be casted to AnyBitmap.

Returns
Type	Description
AnyBitmap	
Implicit(SKImage to AnyBitmap)
Implicitly casts SkiaSharp.SKImage objects to AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support SkiaSharp as well.

When casting to and from AnyBitmap, please remember to dispose your original SkiaSharp.SKImage object to avoid unnecessary memory allocation.

Declaration
public static implicit operator AnyBitmap(SKImage image)
Parameters
Type	Name	Description
SkiaSharp.SKImage	image	
SkiaSharp.SKImage will automatically be casted to AnyBitmap.

Returns
Type	Description
AnyBitmap	
Implicit(Bitmap to AnyBitmap)
Implicitly casts System.Drawing.Bitmap objects to AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support System.Drawing.Common as well.

When casting to and from AnyBitmap, please remember to dispose your original System.Drawing.Bitmap object to avoid unnecessary memory allocation.

Declaration
public static implicit operator AnyBitmap(Bitmap image)
Parameters
Type	Name	Description
System.Drawing.Bitmap	image	
System.Drawing.Bitmap will automatically be casted to AnyBitmap

Returns
Type	Description
AnyBitmap	
Implicit(Image to AnyBitmap)
Implicitly casts System.Drawing.Image objects to AnyBitmap.

When your .NET Class methods use AnyBitmap as parameters or return types, you now automatically support System.Drawing.Common as well.

When casting to and from AnyBitmap, please remember to dispose your original System.Drawing.Image object to avoid unnecessary memory allocation.

Declaration
public static implicit operator AnyBitmap(Image image)
Parameters
Type	Name	Description
System.Drawing.Image	image	
System.Drawing.Image will automatically be casted to AnyBitmap

Returns
Type	Description
AnyBitmap	
Implements
System.IDisposable
IronSoftware.IAnyImage
System.ICloneable
