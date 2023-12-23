using System.Numerics;

namespace VertexTransform.Data;

public struct BoundingBox
{
    public Vector3 Min;
    public Vector3 Max;
    
    public BoundingBox(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }
}