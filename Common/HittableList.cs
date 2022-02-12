using System.Drawing;

namespace Common;

// prolly not needed but eh why not
public class HittableList : Hittable
{
    public readonly List<Hittable> Objects = new();

    public HittableList() { }
    
    public HittableList(List<Hittable> objects)
    {
        Objects = objects;
    }

    public void Clear() => Objects.Clear();
    public void Add(Hittable obj) => Objects.Add(obj);
    
    public override bool Hit(Ray r, double tMin, double tMax, ref HitRecord rec)
    {
        var tempRec = new HitRecord();
        var hitAnything = false;
        var closestSoFar = tMax;

        foreach (var obj in Objects)
        {
            if (!obj.Hit(r, tMin, closestSoFar, ref tempRec)) continue;
            hitAnything = true;
            closestSoFar = tempRec.T;
            rec = tempRec;
        }

        return hitAnything;
    }
}