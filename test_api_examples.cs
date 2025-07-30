using FaceOFFx.Infrastructure.Services;
using FaceOFFx.Core.Domain.Transformations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Test that our API examples compile correctly
        
        // Example 1: FacialImageEncoder basic usage
        byte[] imageData = new byte[] { }; // Would be real image data
        
        // This should compile but will fail at runtime due to empty data
        try 
        {
            var result = await FacialImageEncoder.ProcessAsync(imageData);
            Console.WriteLine("Basic API compiles correctly");
        }
        catch { /* Expected with empty data */ }
        
        // Example 2: ProcessingOptions usage
        var customOptions = ProcessingOptions.PivBalanced with
        {
            MinFaceConfidence = 0.9f,
            RequireSingleFace = false,
            MaxRetries = 3,
            PreserveMetadata = true,
            RoiStartLevel = 2
        };
        Console.WriteLine("ProcessingOptions API compiles correctly");
        
        // Example 3: EncodingStrategy usage
        var targetSizeStrategy = EncodingStrategy.TargetSize(25000);
        var fixedRateStrategy = EncodingStrategy.FixedRate(1.2f);
        Console.WriteLine("EncodingStrategy API compiles correctly");
        
        // Example 4: Service interfaces
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("Test");
        
        try
        {
            var faceDetector = new RetinaFaceDetector(logger);
            var landmarkExtractor = new OnnxLandmarkExtractor(logger);
            var encoder = new Jpeg2000EncoderService(logger);
            Console.WriteLine("Service interfaces compile correctly");
            
            // Clean up
            if (faceDetector is IDisposable df) df.Dispose();
            if (landmarkExtractor is IDisposable le) le.Dispose();
        }
        catch { /* Expected without proper model files */ }
        
        Console.WriteLine("All API examples compile successfully!");
    }
}