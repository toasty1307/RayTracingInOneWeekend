using System.Diagnostics;
using Serilog;
using SkiaSharp;

namespace OutputAnImage;

internal class Program
{
    public const int ImageHeight = 2160;
    public const int ImageWidth = 3840;

    public static void Main()
    {
        new Program()
            .ActualMain();
    }
    
    private void ActualMain()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
        
        Log.Information("Starting");
        var top = Console.GetCursorPosition().Top;
        // wondows
        var image = new SKBitmap(ImageWidth, ImageHeight);
        
        for (var i = 0; i < ImageHeight; i++)
        {
            Console.SetCursorPosition(0, top);
            Log.Information("{Percent}% Done", Math.Floor((float) i / ImageHeight * 100));
            for (var j = 0; j < ImageWidth; j++)
            {
                var ir = (float)i / ImageHeight;
                var ig = (float)(ImageWidth - j) / ImageWidth;
                var ib = 0.65f;
                
                var color = new SKColor((byte) (ir * 255), (byte) (ig * 255), (byte) (ib * 255), 255);
                image.SetPixel(j, i, color);;
            }
        }

        var stream = File.Create("output.png");
        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
        stream.Close();
#pragma warning restore CA1416
        Console.SetCursorPosition(0, top);
        Log.Information("{Percent}% Done", 100);
        Log.Information("Done! Opening file...");
        Process.Start(new ProcessStartInfo("output.png") { UseShellExecute = true });
    }
}
