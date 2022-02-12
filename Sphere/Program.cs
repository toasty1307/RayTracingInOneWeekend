using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using Common;
using Serilog;

namespace Sphere;

internal class Program
{
    public const double AspectRatio = 16.0 / 9.0;
    public const int ImageWidth = 1920;
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
        if (HitSphere(Vector3.UnitZ * -1, 0.5, ray))
            return Color.Red;
        var unitDirection = Vector3.Normalize(ray.Direction);
        var t = unitDirection.Y / 2 + 0.5f;
        var colorVector = (1 - t) * Vector3.One + t * new Vector3(0.5f, 0.7f, 1);
        return Color.FromArgb(255, (int) (colorVector.X * 255), (int) (colorVector.Y * 255), (int) (colorVector.Z * 255));
    }

    public bool HitSphere(Vector3 center, double radius, Ray ray)
    {
        var direction = ray.Origin - center;
        var a = Vector3.Dot(ray.Direction, ray.Direction);
        var b = 2 * Vector3.Dot(direction, ray.Direction);
        var c = Vector3.Dot(direction, direction) - radius * radius;
        var discriminant = b * b - 4 * a * c; // never thought I'd use this
        return discriminant > 0;
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
        
        /*
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
        */

        for (var j = ImageHeight - 1; j >= 0; j--)
        {
            Console.SetCursorPosition(0, top);
            Log.Information("{Percent}% Done", Math.Floor(100 - (float) j / ImageHeight * 100));
            for (var i = 0; i < ImageWidth; i++)
            {
                var u = (double) i / (ImageWidth - 1);
                var v = (double) j / (ImageWidth - 1);
                var ray = new Ray(origin, lowerLeftCorner + horizontal * (float) u + vertical * (float) v - origin);
                var color = RayColor(ray);
                image.SetPixel(i, j, color);
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
