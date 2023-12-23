using System.Numerics;

namespace VertexTransform.Util;

public class MatrixUtil
{
    public static Vector4 Multi(Matrix4x4 a, Vector4 b)
    {
        return new Vector4(
            a.M11 * b.X + a.M12 * b.Y + a.M13 * b.Z + a.M14 * b.W,
            a.M21 * b.X + a.M22 * b.Y + a.M23 * b.Z + a.M24 * b.W,
            a.M31 * b.X + a.M32 * b.Y + a.M33 * b.Z + a.M34 * b.W,
            a.M41 * b.X + a.M42 * b.Y + a.M43 * b.Z + a.M44 * b.W
        );
    }
}