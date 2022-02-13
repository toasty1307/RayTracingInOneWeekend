using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using Common;
using Serilog;

namespace SomeoneStopMe;

internal class Program
{
    public static readonly Random Random = new();
    private const double AspectRatio = 16.0 / 9.0;
    private const int ImageWidth = 3840; // 3840
    private const int Pieces = 10000;
    private int _threadCount = -1; // -1 for max
    private const int ImageHeight = (int) (ImageWidth / AspectRatio); // 2160
    private const int MaxDepth = 50;
    private const int SpheresHalfRow = 11;

    private const int SampleSize = 500;

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

    public static Vector3 RandomInUnitSphere()
    {
        while (true)
        {
            var p = new Vector3((float) Random.NextDouble(), (float) Random.NextDouble(), (float) Random.NextDouble());
            if (p.LengthSquared() > 1) continue;
            return p;
        }
    }

    private Vector3 RayColor(Ray ray, Hittable world, int depth)
    {
        var rec = new HitRecord();
        
        if (depth <= 0) return Vector3.Zero;

        if (world.Hit(ray, 0.001, double.PositiveInfinity, ref rec))
        {
            var scattered = new Ray();
            var attenuation = new Vector3();
            if (rec.Material.Scatter(ray, ref rec, ref attenuation, ref scattered))
                return attenuation * RayColor(scattered, world, depth - 1); 
            return Vector3.Zero;
        }
        var unitDirection = Vector3.Normalize(ray.Direction);
        var t = unitDirection.Y / 2 + 0.5f;
        var colorVector = (1 - t) * Vector3.One + t * new Vector3(0.5f, 0.7f, 1);
        return colorVector;
    }
    
    private void ActualMain()
    {
        Log.Information("Starting...");
        var divide = (int)Math.Sqrt(Pieces);
        var height = ImageHeight / divide;
        ThreadPool.GetAvailableThreads(out var workerThreads, out _);
        if (_threadCount == -1)
            _threadCount = Math.Min(height, workerThreads);
        Log.Information("Using {ThreadCount} threads of out {WorkerThreads} worker threads", _threadCount, workerThreads);
        // wondows
#pragma warning disable CA1416

        var time = DateTime.Now;
        var world = RandomScene();
        var time2 = DateTime.Now;
        
        Log.Information("Time taken to generate world: {TimeTaken}", time2 - time);
        
        Log.Information("Created world with {Num} objects", world.Objects.Count);
        
        var lookFrom = new Vector3(13, 2, 3);
        var lookAt = new Vector3(0, 0, 0);
        
        var camera = new Camera(20, (float) AspectRatio, lookFrom, lookAt, Vector3.UnitY, 0.1f, 10);

        var rowsForOneThread = height / _threadCount;

        var top = Console.GetCursorPosition().Top;

        for (var i = 10 - 1; i >= 0; i--)
        {
            Console.SetCursorPosition(0, top);
            Log.Information("Will start {ThreadCount} threads in {Seconds} seconds. Your pc might die, last time to stop this!", _threadCount, i);
            Thread.Sleep(1000);
        }

        Log.Information("Initiating death sequence...");

        time = DateTime.Now;
        for (var w = 0; w < divide; w++)
        {
            height = ImageHeight / divide * (w + 1);
            for (var e = 0; e < divide; e++)
            {
                var threads = new List<Thread>();
                var start = height - 1;
                for (var i = 0; i < _threadCount; i++)
                {
                    var i1 = i;
                    var s = start;
                    start -= rowsForOneThread;
                    var width1 = ImageWidth / divide;
                    var e1 = e;
                    var thread = new Thread(() =>
                    {
                        var image = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb);
                        for (var j = s; j >= s + 1 - rowsForOneThread; j--)
                        {
                            for (var k = width1 * e1; k < width1 * (e1 + 1); k++)
                            {
                                var colorVector = Vector3.Zero;
                                for (var l = 0; l < SampleSize; l++)
                                {
                                    var u = (k + Random.NextDouble()) / (ImageWidth - 1);
                                    var v = (j + Random.NextDouble()) / (ImageHeight - 1);
                                    var ray = camera.GetRay((float) u, (float) v);
                                    colorVector += RayColor(ray, world, MaxDepth);
                                }

                                WriteColor(image, colorVector, k, j);
                            }
                        }

                        image.Save($"output{i1}.png");
                        image.Dispose();
                    })
                    {
                        IsBackground = false
                    };
                    threads.Add(thread);
                }

                Log.Information("Created all threads!");

                Log.Information("Starting threads...");
                threads.ForEach(thread => thread.Start());

                while (threads.Any(x => x.IsAlive)) { }

                Log.Information("Merging images...");

                try
                {
                    var bitmaps = new Bitmap[_threadCount];
                    for (var j = 0; j < _threadCount; j++)
                    {
                        bitmaps[j] = new Bitmap($"output{j}.png");
                    }

                    var finalImage = MergeBitmaps(bitmaps);
                    finalImage.RotateFlip(RotateFlipType.Rotate180FlipX);
                    finalImage.Save($"part{w * divide + e}.png");

                    for (var i = 0; i < _threadCount; i++)
                    {
                        bitmaps[i].Dispose();
                    }

                    Log.Information("Part {Part} combined and saved!", w * divide + e);
                }
                catch (OutOfMemoryException)
                {
                    Log.Information("Out of memory");
                }
            }
        }

        time2 = DateTime.Now;
        Log.Information("Done! Time taken to render: {TimeTaken}", time2 - time);

        var pieces = new Bitmap[Pieces];
        for (var i = 0; i < Pieces; i++)
            pieces[i] = new Bitmap($"part{i}.png");
        
        var output = MergeBitmaps(pieces);
        output.Save($"0.png");
        
        for (var i = 0; i < Pieces; i++)
            pieces[i].Dispose();

        Log.Information("All parts combined and saved!");
        Process.Start(new ProcessStartInfo("0.png") { UseShellExecute = true });
        Console.ReadLine();
    }
    
    private Bitmap MergeBitmaps(IEnumerable<Bitmap> bitmaps) 
    {
        var result = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(result);
        foreach (var bitmap in bitmaps)
            g.DrawImage(bitmap, Point.Empty);
        return result;
    }

    private Color VecToColor(Vector3 vector3)
    {
        return Color.FromArgb(255, (int) (vector3.X * 255), (int) (vector3.Y * 255), (int) (vector3.Z * 255));
    }

    private void WriteColor(Bitmap map, Vector3 color, int x, int y)
    {
        const float scale = 1.0f / SampleSize;
        color.X = (float) Math.Sqrt(color.X * scale);
        color.Y = (float) Math.Sqrt(color.Y * scale);
        color.Z = (float) Math.Sqrt(color.Z * scale);
        map.SetPixel(x, y, VecToColor(color));
    }
#pragma warning restore CA1416

    private HittableList RandomScene()
    {
        var world = new HittableList();

        var groundMaterial = new Lambertian(Vector3.One * 0.5f);
        world.Add(new Sphere(new Vector3(0, -1000, 0), 1000, groundMaterial));
        
        for (var i = -SpheresHalfRow; i < SpheresHalfRow; i++)
        {
            for (var j = -SpheresHalfRow; j < SpheresHalfRow; j++)
            {
                var chooseMat = Random.NextDouble();
                var center = new Vector3((float) (i + 0.9*Random.NextDouble()), 0.2f, (float) (j + 0.9*Random.NextDouble()));

                if (!((center - new Vector3(4, 0.2f, 0)).Length() > 0.9)) continue;
                Material sphereMaterial = chooseMat switch
                {
                    < 0.8 => new Lambertian(new Vector3((float) (Random.NextDouble() * Random.NextDouble()),
                        (float) (Random.NextDouble() * Random.NextDouble()),
                        (float) (Random.NextDouble() * Random.NextDouble()))),
                    < 0.95 => new Metal(
                        new Vector3((float) (0.5f * (1 + Random.NextDouble())),
                            (float) (0.5f * (1 + Random.NextDouble())), (float) (0.5f * (1 + Random.NextDouble()))),
                        0.5f * Random.NextDouble()),
                    _ => new Dielectric(1.5f)
                };
                world.Add(new Sphere(center, 0.2f, sphereMaterial));
            }
        }

        var material1 = new Dielectric(1.5f);
        world.Add(new Sphere(new Vector3(0, 1, 0), 1, material1));
        
        var material2 = new Lambertian(new Vector3(0.4f, 0.2f, 0.1f));
        world.Add(new Sphere(new Vector3(-4, 1, 0), 1, material2));
        
        var material3 = new Metal(new Vector3(0.7f, 0.6f, 0.5f), 0.0f);
        world.Add(new Sphere(new Vector3(4, 1, 0), 1, material3));

        return world;
    }
}
