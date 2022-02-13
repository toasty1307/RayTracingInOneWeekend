using System.Numerics;

namespace Common;

public class Ray
{
    private readonly Vector3 _origin;
    private readonly Vector3 _direction;

    public Vector3 Origin
    {
        get => _origin;
        init => _origin = value;
    }

    public Vector3 Direction
    {
        get => _direction;
        init => _direction = value;
    }

    public Ray() { }
    
    public Ray(Vector3 origin, Vector3 direction)
    {
        _origin = origin;
        _direction = direction;
    }
    
    public Vector3 PointAt(float t)
    {
        return _origin + t * _direction;
    }
}