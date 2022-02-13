using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using Common;
using Serilog;

namespace DiffuseMaterials;

internal class Program
{
    private readonly Random _random = new();
    public const double AspectRatio = 16.0 / 9.0;
    public const int ImageWidth = 512;
    public const int ImageHeight = (int) (ImageWidth / AspectRatio);
    public const int MaxDepth = 50;
    
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

    public Vector3 RandomInHemisphere(Vector3 normal)
    {
        var inUnitSphere = RandomInUnitSphere();
        if (Vector3.Dot(inUnitSphere, normal) > 0)
            return inUnitSphere;
        return -inUnitSphere;
    }

    public Vector3 RandomInUnitSphere()
    {
        while (true)
        {
            var p = new Vector3((float) _random.NextDouble(), (float) _random.NextDouble(), (float) _random.NextDouble());
            if (p.LengthSquared() > 1) continue;
            return p;
        }
    }

    public Vector3 RayColor(Ray ray, Hittable world, int depth)
    {
        var rec = new HitRecord();
        
        if (depth <= 0) return Vector3.Zero;

        if (world.Hit(ray, 0.001, double.PositiveInfinity, ref rec))
        {
            var target = rec.Point + RandomInHemisphere(rec.Normal);
            return (float)0.5 * RayColor(new Ray(rec.Point, target - rec.Point), world, depth - 1);
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
                    var u = (i + _random.NextDouble()) / (ImageWidth - 1);
                    var v = (j + _random.NextDouble()) / (ImageHeight - 1);
                    var ray = camera.GetRay((float) u, (float) v);
                    colorVector += RayColor(ray, world, MaxDepth);
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
        color.X = (float) Math.Sqrt(color.X * scale);
        color.Y = (float) Math.Sqrt(color.Y * scale);
        color.Z = (float) Math.Sqrt(color.Z * scale);
        map.SetPixel(x, y, VecToColor(color));
    }
#pragma warning restore CA1416

}
