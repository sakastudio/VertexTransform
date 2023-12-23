using System.Numerics;

namespace VertexTransform.Data;

public class Vertex
{
    public Vector4 ModelPosition;
    public Vector4 WorldPosition;
    public Vector4 ViewPosition;
    public Vector4 ClipPosition;
    public Vector2Int ScreenPosition; 
        
    public int VertexIndex;
}