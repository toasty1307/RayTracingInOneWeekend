using System.Numerics;

namespace Common;

public class Camera
{
    private Vector3 _origin;
    private Vector3 _lowerLeftCorner;
    private Vector3 _horizontal;
    private Vector3 _vertical;

    public Camera()
    {
        var aspectRatio = 16.0f / 9.0f;
        var viewportHeight = 2.0f;
        var viewportWidth = aspectRatio * viewportHeight;
        var focalLength = 1.0f;
        
        _origin = new Vector3(0, 0, 0);
        _horizontal = new Vector3(viewportWidth, 0, 0);
        _vertical = new Vector3(0, viewportHeight, 0);
        _lowerLeftCorner = _origin - _horizontal / 2 - _vertical / 2 - new Vector3(0, 0, focalLength);
    }
    
    public Ray GetRay(float u, float v)
    {
        return new Ray(_origin, _lowerLeftCorner + u * _horizontal + v * _vertical - _origin);
    }
}