namespace FaceOFFx.Core.Domain.Transformations;

/// <summary>
/// Represents a crop region with normalized coordinates (0-1)
/// </summary>
public sealed record CropRegion
{
    /// <summary>
    /// Gets the normalized X coordinate of the top-left corner (0-1)
    /// </summary>
    public float X { get; }
    
    /// <summary>
    /// Gets the normalized Y coordinate of the top-left corner (0-1)
    /// </summary>
    public float Y { get; }
    
    /// <summary>
    /// Gets the normalized width of the crop region (0-1)
    /// </summary>
    public float Width { get; }
    
    /// <summary>
    /// Gets the normalized height of the crop region (0-1)
    /// </summary>
    public float Height { get; }
    
    /// <summary>
    /// Right edge position (X + Width)
    /// </summary>
    public float Right => X + Width;
    
    /// <summary>
    /// Bottom edge position (Y + Height)
    /// </summary>
    public float Bottom => Y + Height;
    
    /// <summary>
    /// Aspect ratio of the crop region
    /// </summary>
    public float AspectRatio => Width / Height;
    
    private CropRegion(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    /// <summary>
    /// Creates a new crop region with normalized coordinates
    /// </summary>
    /// <param name="x">Normalized X coordinate (0-1)</param>
    /// <param name="y">Normalized Y coordinate (0-1)</param>
    /// <param name="width">Normalized width (0-1)</param>
    /// <param name="height">Normalized height (0-1)</param>
    /// <returns>Success with CropRegion if valid, otherwise failure</returns>
    /// <remarks>
    /// All values must be between 0 and 1, and the crop region must not extend beyond the image bounds
    /// </remarks>
    public static Result<CropRegion> Create(float x, float y, float width, float height)
    {
        if (x is < 0 or >= 1)
        {
            return Result.Failure<CropRegion>("X coordinate must be between 0 and 1");
        }
        if (y is < 0 or >= 1)
        {
            return Result.Failure<CropRegion>("Y coordinate must be between 0 and 1");
        }
        if (width is <= 0 or > 1)
        {
            return Result.Failure<CropRegion>("Width must be between 0 and 1");
        }
        if (height is <= 0 or > 1)
        {
            return Result.Failure<CropRegion>("Height must be between 0 and 1");
        }
        if (x + width > 1)
        {
            return Result.Failure<CropRegion>("Crop extends beyond right edge");
        }
        if (y + height > 1)
        {
            return Result.Failure<CropRegion>("Crop extends beyond bottom edge");
        }

        return Result.Success(new CropRegion(x, y, width, height));
    }
    
    /// <summary>
    /// Creates a crop region from pixel coordinates
    /// </summary>
    public static Result<CropRegion> FromPixels(
        int pixelX, int pixelY, 
        int pixelWidth, int pixelHeight,
        int imageWidth, int imageHeight)
    {
        if (imageWidth <= 0 || imageHeight <= 0)
        {
            return Result.Failure<CropRegion>("Image dimensions must be positive");
        }

        var x = (float)pixelX / imageWidth;
        var y = (float)pixelY / imageHeight;
        var width = (float)pixelWidth / imageWidth;
        var height = (float)pixelHeight / imageHeight;
        
        return Create(x, y, width, height);
    }
    
    /// <summary>
    /// Converts normalized coordinates to pixel coordinates
    /// </summary>
    public (int x, int y, int width, int height) ToPixels(int imageWidth, int imageHeight)
    {
        var pixelX = (int)Math.Round(X * imageWidth);
        var pixelY = (int)Math.Round(Y * imageHeight);
        var pixelWidth = (int)Math.Round(Width * imageWidth);
        var pixelHeight = (int)Math.Round(Height * imageHeight);
        
        // Ensure we don't exceed image bounds due to rounding
        pixelX = Math.Min(pixelX, imageWidth - 1);
        pixelY = Math.Min(pixelY, imageHeight - 1);
        pixelWidth = Math.Min(pixelWidth, imageWidth - pixelX);
        pixelHeight = Math.Min(pixelHeight, imageHeight - pixelY);
        
        return (pixelX, pixelY, pixelWidth, pixelHeight);
    }
    
    /// <summary>
    /// Full image crop (no cropping)
    /// </summary>
    public static CropRegion Full => new(0f, 0f, 1f, 1f);
    
    /// <summary>
    /// Whether this represents actual cropping
    /// </summary>
    public bool IsSignificant => X > 0.001f || Y > 0.001f || 
                                 Width < 0.999f || Height < 0.999f;
}