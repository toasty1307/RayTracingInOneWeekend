using System.Numerics;

namespace Common;

public class Material
{
    public virtual bool Scatter(Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered)
    {
        return false;
    }
    
    public static bool NearZero(Vector3 vector3)
    {
        var epsilon = 1e-8;
        return Math.Abs(vector3.X) < epsilon && Math.Abs(vector3.Y) < epsilon && Math.Abs(vector3.Z) < epsilon;
    }
    
    public static Vector3 Reflect(Vector3 vector3, Vector3 normal)
    {
        return vector3 - 2 * Vector3.Dot(vector3, normal) * normal;
    }

    public Vector3 Refract(Vector3 uv, Vector3 normal, double etaiOverEtat)
    {
        var cosTheta = Math.Min(Vector3.Dot(-uv, normal), 1.0);
        var rOutPerp = (float) etaiOverEtat * (uv + (float) cosTheta * normal); // these casts are gone kill me, i should prolly start using MathF
        var rOutParallel = (float)-Math.Sqrt(Math.Abs(1.0 - rOutPerp.LengthSquared())) * normal;
        return rOutPerp + rOutParallel;
    }
}