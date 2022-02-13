using System.Numerics;
using Common;

namespace SomeoneStopMe;

public class Lambertian : Material
{
    public Vector3 Albedo;
    
    public Lambertian(Vector3 albedo)
    {
        Albedo = albedo;
    }

    public override bool Scatter(Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered)
    {
        var scatterDirection = record.Normal + Vector3.Normalize(Program.RandomInUnitSphere());
        if (NearZero(scatterDirection))
            scatterDirection = record.Normal;
        scattered = new Ray(record.Point, scatterDirection);
        attenuation = Albedo;
        return true;
    }
}