using System.Numerics;
using Common;

namespace Dielectric;

public class Dielectric : Material
{
    public float RefractionIndex;

    public Dielectric(float refractionIndex)
    {
        RefractionIndex = refractionIndex;
    }

    public override bool Scatter(Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered)
    {
        attenuation = Vector3.One;
        var refractionRatio = record.FrontFace ? 1.0 / RefractionIndex : RefractionIndex;
        var unitDirection = Vector3.Normalize(ray.Direction);

        var cosTheta = Math.Min(Vector3.Dot(-unitDirection, record.Normal), 1.0f);
        var sinTheta = Math.Sqrt(1.0f - cosTheta * cosTheta);

        var cannotRefract = refractionRatio * sinTheta > 1.0f;

        var direction = cannotRefract || Reflectance(cosTheta, (float) refractionRatio) < Program.Random.NextDouble()
            ? Reflect(unitDirection, record.Normal) 
            : Refract(unitDirection, record.Normal, refractionRatio);
        
        
        scattered = new Ray(record.Point, direction);
        return true;
    }
    
    private static double Reflectance(float cosine, float refractionIndex)
    {
        var r0 = (1 - refractionIndex) / (1 + refractionIndex);
        r0 *= r0;
        return r0 + (1 - r0) * Math.Pow(1 - cosine, 5);
    }
}