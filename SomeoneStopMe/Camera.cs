using System.Numerics;
using Common;

namespace SomeoneStopMe;

public class Camera
{
    private Vector3 _origin;
    private Vector3 _lowerLeftCorner;
    private Vector3 _horizontal;
    private Vector3 _vertical;
    private Vector3 _w;
    private Vector3 _u;
    private Vector3 _v;
    private float _lensRadius;

    public Camera(float vFOV, float aspectRatio, Vector3 lookFrom, Vector3 lookAt, Vector3 vUp, float aperture, float focusDist)
    {
        var theta = MathF.PI * vFOV / 180;
        var h = Math.Tan(theta / 2);
        var viewportHeight = 2 * h;
        var viewportWidth = aspectRatio * viewportHeight;
        
        _w = Vector3.Normalize(lookFrom - lookAt);
        _u = Vector3.Normalize(Vector3.Cross(vUp, _w));
        _v = Vector3.Cross(_w, _u);
        
        _origin = lookFrom;
        _horizontal = (float)viewportWidth * (focusDist * _u);
        _vertical = (float)viewportHeight * (_v * focusDist);
        _lowerLeftCorner = _origin - _horizontal / 2 - _vertical / 2 - _w * focusDist;
        _lensRadius = aperture / 2;
    }

    public Ray GetRay(float s, float t)
    {
        var rd = _lensRadius * RandomInUnitDisk();
        var offset = _u * rd.X + _v * rd.Y;
        return new Ray(_origin + offset, _lowerLeftCorner + s * _horizontal + t * _vertical - _origin - offset);
    }

    public Vector3 RandomInUnitDisk()
    {
        while (true)
        {
            var p = new Vector3(Program.Random.Next(-1000, 1000) / 1000f, Program.Random.Next(-1000, 1000) / 1000f, 0);
            if (p.LengthSquared() >= 1) continue;
            return p;
        }
    }
}