using System.Numerics;
using System.Runtime.InteropServices;
using Common;

namespace Dielectric;

public class Camera
{
    private Vector3 _origin;
    private Vector3 _lowerLeftCorner;
    private Vector3 _horizontal;
    private Vector3 _vertical;

    public Camera(float vFOV, float aspectRatio, Vector3 lookFrom, Vector3 lookAt, Vector3 vUp)
    {
        var theta = MathF.PI * vFOV / 180;
        var h = Math.Tan(theta / 2);
        var viewportHeight = 2 * h;
        var viewportWidth = aspectRatio * viewportHeight;
        var focalLength = 1.0f;

        var w = Vector3.Normalize(lookFrom - lookAt);
        var u = Vector3.Normalize(Vector3.Cross(vUp, w));
        var v = Vector3.Cross(w, u);
        
        _origin = lookFrom;
        _horizontal = (float)viewportWidth * u;
        _vertical = (float)viewportHeight * v;
        _lowerLeftCorner = _origin - _horizontal / 2 - _vertical / 2 - w;
    }

    public Ray GetRay(float s, float t)
    {
        return new Ray(_origin, _lowerLeftCorner + s * _horizontal + t * _vertical - _origin);
    }
}