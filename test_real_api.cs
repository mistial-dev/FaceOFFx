using FaceOFFx.Infrastructure.Services;
using FaceOFFx.Core.Domain.Transformations;

class RealApiTest
{
    static async Task Main()
    {
        // Test with actual image file
        var testImagePath = "tests/sample_images/generic_guy.jpg";
        
        if (!File.Exists(testImagePath))
        {
            Console.WriteLine($"Test image not found: {testImagePath}");
            return;
        }
        
        try
        {
            // Load actual image data
            byte[] imageData = await File.ReadAllBytesAsync(testImagePath);
            Console.WriteLine($"Loaded test image: {imageData.Length:N0} bytes");
            
            // Test FacialImageEncoder API as documented
            var result = await FacialImageEncoder.ProcessAsync(imageData);
            
            Console.WriteLine("✓ API example works correctly:");
            Console.WriteLine($"  Output size: {result.Metadata.FileSize:N0} bytes");
            Console.WriteLine($"  Face confidence: {result.Metadata.FaceConfidence:P1}");
            Console.WriteLine($"  Rotation applied: {result.Metadata.RotationApplied:F1}°");
            Console.WriteLine($"  Processing time: {result.Metadata.ProcessingTime.TotalMilliseconds:F0}ms");
            Console.WriteLine($"  Compression rate: {result.Metadata.CompressionRate:F2} bpp");
                
                // Test preset comparison as documented
                var presets = new[]
                {
                    ("PIV Min", ProcessingOptions.PivMin),
                    ("PIV Balanced", ProcessingOptions.PivBalanced),
                    ("PIV High", ProcessingOptions.PivHigh)
                };
                
            Console.WriteLine("\n✓ Preset comparison:");
            foreach (var (name, preset) in presets)
            {
                var presetResult = await FacialImageEncoder.ProcessAsync(imageData, preset);
                var meta = presetResult.Metadata;
                Console.WriteLine($"  {name,-12} | {meta.FileSize,6:N0} bytes | {meta.CompressionRate,4:F2} bpp");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Exception during API test: {ex.Message}");
        }
    }
}