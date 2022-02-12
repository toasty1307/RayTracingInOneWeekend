using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Serilog;

namespace OutputAnImage;

internal class Program
{
    public const int ImageHeight = 4096;
    public const int ImageWidth = ImageHeight;

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
#pragma warning disable CA1416
        var image = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb);
        
        for (var i = 0; i < ImageHeight; i++)
        {
            Console.SetCursorPosition(0, top);
            Log.Information("{Percent}% Done", Math.Floor((float) i / ImageHeight * 100));
            for (var j = 0; j < ImageWidth; j++)
            {
                // i like it this way
                // var r = (double) i / (ImageHeight - 1);
                // var g = (double) j / (ImageWidth - 1);
                // var b = (r + g) / 2;
                
                // same image as the book
                var r =     (double) j / (ImageHeight - 1);
                var g = 1 - (double) i / (ImageWidth - 1);
                var b = 0.65;
                
                var ir = Math.Floor(r * 255);
                var ig = Math.Floor(g * 255);
                var ib = Math.Floor(b * 255);
                var color = Color.FromArgb(255, (int) ir, (int) ig, (int) ib);
                image.SetPixel(j, i, color);
            }
        }
        image.Save("image.png", ImageFormat.Png);
#pragma warning restore CA1416
        Console.SetCursorPosition(0, top);
        Log.Information("{Percent}% Done", 100);
        Log.Information("Done! Opening file...");
        Process.Start(new ProcessStartInfo("image.png") { UseShellExecute = true });
    }
}
