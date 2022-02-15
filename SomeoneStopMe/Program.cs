using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using Common;
using Serilog;
using SkiaSharp;

namespace SomeoneStopMe;

internal class Program
{
    public static readonly Random Random = new();
    private const double AspectRatio = 16.0 / 9.0;
    private const int ImageWidth = 15360;
    private const int Pieces = 1000 * 1000;
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

        var time = DateTime.Now;
        var world = RandomScene();
        var time2 = DateTime.Now;
        
        Log.Information("Time taken to generate world: {TimeTaken}", time2 - time);
        
        Log.Information("Created world with {Num} objects", world.Objects.Count);
        
        var lookFrom = new Vector3(13, 2, 3);
        var lookAt = new Vector3(0, 0, 0);
        
        var camera = new Camera(20, (float) AspectRatio, lookFrom, lookAt, Vector3.UnitY, 0.1f, 10);

        var rowsForOneThread = height / _threadCount;

        Log.Information("I cant wait 10secs everytime i go to debug, so 2 instead");

        var top = Console.GetCursorPosition().Top;

        for (var i = 2 - 1; i >= 0; i--)
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
                        var image = new SKBitmap(ImageWidth, ImageHeight);
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

                        using var stream = File.Create($"output{i1}.png");
                        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
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
                    var output = new SKBitmap(ImageWidth, ImageHeight);
                    using var canvas = new SKCanvas(output);
                    for (var i = 0; i < _threadCount; i++)
                    {
                        var file = $"output{i}.png";
                        using var se = File.Open(file, FileMode.Open);
                        var bitmap = SKBitmap.Decode(se);
                        canvas.DrawBitmap(bitmap, new SKPoint(0, 0));
                        Console.SetCursorPosition(0, top);
                        bitmap.Dispose();
                    }
                    canvas.RotateDegrees(180);
                    canvas.Save();
                    using var stream = File.Create($"part{w * divide + e}.png");
                    output.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
                    output.Dispose();
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

        var image = new SKBitmap(3840, 2160);
        var ee = new SKCanvas(image);
        Log.Information("Combining parts...");
        top = Console.GetCursorPosition().Top;
        for (var i = 0; i < Pieces; i++)
        {
            var file = $"part{i}.png";
            using var stream = File.Open(file, FileMode.Open);
            var bitmap = SKBitmap.Decode(stream);
            ee.DrawBitmap(bitmap, new SKPoint(0, 0));
            Log.Information("Combined {Files} files", i + 1);
            Console.SetCursorPosition(0, top);
            bitmap.Dispose();
        }
        Log.Information("Combined all files");

        ee.Save();
        using var milk = File.Create("0.png");
        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(milk);
        milk.Close();
        
        Log.Information("All parts combined and saved!");
        Process.Start(new ProcessStartInfo("0.png") { UseShellExecute = true });
        Console.ReadLine();
    }

    private SKBitmap MergeBitmaps(IEnumerable<SKBitmap> bitmaps)
    {
        var img = new SKBitmap(ImageWidth, ImageHeight);
        using var canvas = new SKCanvas(img);
        foreach (var bitmap in bitmaps)
        {
            canvas.DrawBitmap(bitmap, new SKPoint(0, 0));
        }

        canvas.Save();
        return img;
    }

    private SKColor VecToColor(Vector3 vector3)
    {
        return new SKColor((byte) (vector3.X * 255), (byte) (vector3.Y * 255), (byte) (vector3.Z * 255), 255);
    }

    private void WriteColor(SKBitmap map, Vector3 color, int x, int y)
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
