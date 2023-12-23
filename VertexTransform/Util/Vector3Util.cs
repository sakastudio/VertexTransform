using System.Numerics;

namespace VertexTransform.Util;

public class Vector3Util
{
    public static Vector3 Normalize(Vector3 v)
    {
        var length = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        return new Vector3(v.X / length, v.Y / length, v.Z / length);
    }
}