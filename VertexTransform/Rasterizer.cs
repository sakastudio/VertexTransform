using System.Diagnostics;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using VertexTransform.Data;
using VertexTransform.Util;

namespace VertexTransform;

public class Rasterizer
{
    public static void _Main()
    {
        //オブジェクトをロードする
        var (vertices, faces) = Util.ObjLoader.LoadTeapot();
        
        const int width = 320;
        const int height = 180;
        
        var objectPos = new Vector3(0, 0, 0);
        var objectRotateDegree = new Vector3(0, 90, 0);
        var objectScale = new Vector3(1, 1, 1);
        
        var cameraPos = new Vector3(20, 10, 20);
        var cameraTarget = new Vector3(0, 0, 0);
        
        //座標変換
        var convertedVertex = VertexTransformer.ConvertVertex(vertices, 
            objectPos, objectRotateDegree, objectScale, 
            cameraPos, cameraTarget,
            new Vector2Int(width,height));
        
        //ライトの設定
        var lightDirection = new Vector3(-1f, 1f, 0f);
        
        
        var pixels = new Rgba32[width, height];
        var zBuffer = new float[width, height];
        
        
        var path = "step2.png";


        for (int x = 0; x < width; x++)
        {
            Console.WriteLine($"x:{x}");
            for (int y = 0; y < height; y++)
            {
                pixels[x, y] = new Rgba32(0, 0, 0);
                zBuffer[x, y] = float.MaxValue;
                var rasterizeX = 1.0f / width + x * (2.0f / width) - 1.0f;
                var rasterizeY = 1.0f / height + y * (2.0f / height) - 1.0f;

                foreach (var face in faces)
                {
                    var bb = GetClippingBoundingBox(face,vertices);

                    if (rasterizeX < bb.Min.X || rasterizeX > bb.Max.X ||
                        rasterizeY < bb.Min.Y || rasterizeY > bb.Max.Y)
                    {
                        continue;
                    }
                    
                    var AVertex = vertices[face[0]];
                    var BVertex = vertices[face[1]];
                    var CVertex = vertices[face[2]];

                    var A = AVertex.ClipPosition;
                    var B = BVertex.ClipPosition;
                    var C = CVertex.ClipPosition;

                    // Edge関数の計算
                    var edgeA = (C.X - B.X) * (rasterizeY - B.Y) - (C.Y - B.Y) * (rasterizeX - B.X); // BC×BP
                    var edgeB = (A.X - C.X) * (rasterizeY - C.Y) - (A.Y - C.Y) * (rasterizeX - C.X); // CA×CP
                    var edgeC = (B.X - A.X) * (rasterizeY - A.Y) - (B.Y - A.Y) * (rasterizeX - A.X); // AB×AP

                    if (!(edgeA >= 0 & edgeB >= 0 & edgeC >= 0)) continue;
                    
                    float lambda_A, lambda_B, lambda_C;
                    var temp = edgeA + edgeB + edgeC;
                    lambda_A = edgeA/temp;
                    lambda_B = edgeB/temp;
                    lambda_C = edgeC/temp;
                    var depth = lambda_A * A.Z + lambda_B * B.Z + lambda_C * C.Z;
                    if (zBuffer[x, y] < depth)
                    {
                        //continue;
                    }
                    
                    
                    // z-バッファ法 (より手前にあるものを描画)
                    zBuffer[x, y] = depth;
                        
                    //ノーマル方向を計算する
                    var worldA = new Vector3(AVertex.WorldPosition.X, AVertex.WorldPosition.Y, AVertex.WorldPosition.Z);
                    var worldB = new Vector3(BVertex.WorldPosition.X, BVertex.WorldPosition.Y, BVertex.WorldPosition.Z);
                    var worldC = new Vector3(CVertex.WorldPosition.X, CVertex.WorldPosition.Y, CVertex.WorldPosition.Z);
                    var normal = Vector3.Cross(worldB - worldA, worldC - worldA);
                    normal = Vector3Util.Normalize(normal);
                        
                    //ライトとノーマルの内積を計算する
                    var color = 0.2f + 0.8f * MathF.Max(0, Vector3.Dot(normal, lightDirection));
                        
                    pixels[x, y] = new Rgba32(color, color, color);
                }
            }
        }
        
        ImageExporter.ExportImage(pixels, path);
        
        //画像ファイルを開く
        using var ps1 = new Process();
        ps1.StartInfo.UseShellExecute = true;
        ps1.StartInfo.FileName = path;
        ps1.Start();
    }

    private static BoundingBox GetClippingBoundingBox(List<int> faces,Dictionary<int,Vertex> vertices)
    {
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (var face in faces)
        {
            var vertex = vertices[face];
            var clipping = vertex.ClipPosition;
            
            min.X = Math.Min(min.X, clipping.X);
            min.Y = Math.Min(min.Y, clipping.Y);
            min.Z = Math.Min(min.Z, clipping.Z);
            
            max.X = Math.Max(max.X, clipping.X);
            max.Y = Math.Max(max.Y, clipping.Y);
            max.Z = Math.Max(max.Z, clipping.Z);
        }
        
        return new BoundingBox(min, max);
    }
}