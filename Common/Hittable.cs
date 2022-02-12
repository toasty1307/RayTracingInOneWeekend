namespace Common;

public class Hittable
{
    public virtual bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
    {
        return false;
    }
}