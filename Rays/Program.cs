using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using Common;
using Serilog;

namespace Rays;

internal class Program
{
    public const double AspectRatio = 16.0 / 9.0;
    public const int ImageWidth = 400;
    public const int ImageHeight = (int) (ImageWidth / AspectRatio);
    
    public const float ViewportHeight = 2.0f;
    public const double ViewportWidth = AspectRatio * ViewportHeight;
    public const float FocalLength = 1.0f;

    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
        
        new Program()
            .ActualMain();
    }

    public Color RayColor(Ray ray)
    {
        var unitDirection = ray.Direction / ray.Direction.Length();
        var t = unitDirection.Y / 2 + 0.5f;
        var colorVector = (1 - t) * Vector3.One + t * new Vector3(0.5f, 0.7f, 1);
        return Color.FromArgb(255, (int) (colorVector.X * 255), (int) (colorVector.Y * 255), (int) (colorVector.Z * 255));
    }
    
    private void ActualMain()
    {
        Log.Information("Starting");
        var top = Console.GetCursorPosition().Top;
        // wondows
#pragma warning disable CA1416
        var image = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb);
        
        var origin = Vector3.Zero;
        var horizontal = Vector3.UnitX * (float) ViewportWidth;
        var vertical = Vector3.UnitY * ViewportHeight;
        var lowerLeftCorner = origin - horizontal / 2 - vertical / 2 - Vector3.UnitZ * FocalLength;
        
        for (var i = 0; i < ImageHeight; i++)
        {
            Console.SetCursorPosition(0, top);
            Log.Information("{Percent}% Done", Math.Floor((float) i / ImageHeight * 100));
            for (var j = 0; j < ImageWidth; j++)
            {
                var u =     (double) j / (ImageHeight - 1);
                var v = 1 - (double) i / (ImageWidth - 1);
                var r = new Ray(origin, lowerLeftCorner + horizontal * (float) u + vertical * (float) v - origin);
                var color = RayColor(r); 
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
