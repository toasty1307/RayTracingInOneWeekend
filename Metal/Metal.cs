using System.Numerics;
using Common;

namespace Metal;

public class Metal : Material
{
    public double F;
    public Vector3 Albedo;
    
    public Metal(Vector3 albedo, double f)
    {
        F = f < 1 ? f : 1;
        Albedo = albedo;
    }

    public override bool Scatter(Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered)
    {
        var reflected = Reflect(Vector3.Normalize(ray.Direction), record.Normal);
        scattered = new Ray(record.Point, reflected + (float)F * Program.RandomInUnitSphere());
        attenuation = Albedo;
        return Vector3.Dot(scattered.Direction, record.Normal) > 0;
    }
}