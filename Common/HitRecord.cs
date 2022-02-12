using System.Numerics;
using System.Runtime.CompilerServices;

namespace Common;

public struct HitRecord
{
    public Vector3 Point;
    public Vector3 Normal;
    public double T;
    public bool FrontFace;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // ig this is the same as `inline void`
    public void SetFaceNormal(Ray ray, Vector3 outwardsNormal)
    {
        FrontFace = Vector3.Dot(ray.Direction, outwardsNormal) < 0;
        Normal = FrontFace ? outwardsNormal : -outwardsNormal;
    }
}