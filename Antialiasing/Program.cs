using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using Common;
using Serilog;

namespace Antialiasing;

internal class Program
{
    public const double AspectRatio = 16.0 / 9.0;
    public const int ImageWidth = 1024;
    public const int ImageHeight = (int) (ImageWidth / AspectRatio);
    
    public const float ViewportHeight = 2.0f;

    public const int SampleSize = 100;

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

    public Vector3 RayColor(Ray ray, Hittable world)
    {
        var rec = new HitRecord();
        if (world.Hit(ray, 0, double.PositiveInfinity, ref rec))
        {
            var colorVec = (rec.Normal + Vector3.One) / 2;
            return colorVec;
        }
        var unitDirection = Vector3.Normalize(ray.Direction);
        var t = unitDirection.Y / 2 + 0.5f;
        var colorVector = (1 - t) * Vector3.One + t * new Vector3(0.5f, 0.7f, 1);
        return colorVector;
    }
    
    private void ActualMain()
    {
        Log.Information("Starting");
        var top = Console.GetCursorPosition().Top;
        // wondows
#pragma warning disable CA1416
        var image = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb);

        var camera = new Camera();

        var random = new Random();
        
        var world = new HittableList(new List<Hittable>
        {
            new Sphere(new Vector3(0, 0, -1), 0.5f),
            new Sphere(new Vector3(0, -100.5f, -1), 100),
        });
        
        for (var j = ImageHeight - 1; j >= 0; j--)
        {
            Console.SetCursorPosition(0, top);
            Log.Information("{Percent}% Done", Math.Floor(100 - (float) j / ImageHeight * 100));
            for (var i = 0; i < ImageWidth; i++)
            {
                var colorVector = Vector3.Zero;
                for (var k = 0; k < SampleSize; k++)
                {
                    var u = (i + random.NextDouble()) / (ImageWidth - 1);
                    var v = (j + random.NextDouble()) / (ImageHeight - 1);
                    var ray = camera.GetRay((float) u, (float) v);
                    colorVector += RayColor(ray, world);
                }
                WriteColor(image, colorVector, i, j);
            }
        }

        image.RotateFlip(RotateFlipType.Rotate180FlipX);
        image.Save("image.png", ImageFormat.Png);
        
        Console.SetCursorPosition(0, top);
        Log.Information("{Percent}% Done", 100);
        Log.Information("Done! Opening file...");
        Process.Start(new ProcessStartInfo("image.png") { UseShellExecute = true });
    }

    public Color VecToColor(Vector3 vector3)
    {
        return Color.FromArgb(255, (int) (vector3.X * 255), (int) (vector3.Y * 255), (int) (vector3.Z * 255));
    }

    public void WriteColor(Bitmap map, Vector3 color, int x, int y)
    {
        const float scale = 1.0f / SampleSize;
        color *= scale;
        map.SetPixel(x, y, VecToColor(color));
    }
#pragma warning restore CA1416

}
