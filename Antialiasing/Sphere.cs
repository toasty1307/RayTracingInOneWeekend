using System.Numerics;
using Common;

namespace Antialiasing;

public class Sphere : Hittable
{
    public readonly Vector3 Center;
    public readonly float Radius;

    public Sphere(Vector3 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    public override bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
    {
        var oc = ray.Origin - Center;
        var a = ray.Direction.LengthSquared();
        var halfB = Vector3.Dot(oc, ray.Direction);
        var c = oc.LengthSquared() - Radius * Radius;
        
        var discriminant = halfB * halfB - a * c;
        if (discriminant < 0) return false;

        var sqrtD = Math.Sqrt(discriminant);
        var root = (-halfB - sqrtD) / a;
        if (root < tMin || tMax < root)
        {
            root = (-halfB + sqrtD) / a;
            if (root < tMin || tMax < root) return false;
        }

        var p = ray.PointAt((float) root);
        
        rec = new HitRecord
        {
            T = root,
            Point = p,
        };
        
        rec.SetFaceNormal(ray, (p - Center) / Radius);
        return true;
    }
}