using System.Diagnostics;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using VertexTransform.Data;
using VertexTransform.Util;

namespace VertexTransform;

public class VertexTransformer
{
    public static void Main()
    {
        //オブジェクトをロードする
        var (vertices, face) = Util.ObjLoader.LoadTeapot();
        
        const int width = 640;
        const int height = 360;
        
        var objectPos = new Vector3(0, 0, 0);
        var objectRotateDegree = new Vector3(0, 90, 0);
        var objectScale = new Vector3(1, 1, 1);
        
        var cameraPos = new Vector3(20, 10, 20);
        var cameraTarget = new Vector3(0, 0, 0);
        
        //座標変換
        var convertedVertex = ConvertVertex(vertices, 
            objectPos, objectRotateDegree, objectScale, 
            cameraPos, cameraTarget,
            new Vector2Int(width,height));
        
        
        // 出力画像に変換する
        var pixels = new Rgba32[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                pixels[i, j] = new Rgba32(0, 0, 0);
            }
        }
        
        var path = "step1.png";
        foreach (var vertex in convertedVertex.Values)
        {
            var point = vertex.ScreenPosition;
            var x = point.X;
            var y = point.Y;
            
            pixels[x, y] = new Rgba32(255, 255, 255);
        }

        
        //画像出力
        ImageExporter.ExportImage(pixels, path);
        
        
        //画像ファイルを開く
        using var ps1 = new Process();
        ps1.StartInfo.UseShellExecute = true;
        ps1.StartInfo.FileName = path;
        ps1.Start();
    }


    public static Dictionary<int, Vertex> ConvertVertex(Dictionary<int, Vertex> vertexDict,
        Vector3 objectPos, Vector3 objectRotateDegree, Vector3 objectScale,
        Vector3 cameraPos, Vector3 cameraTarget,
        Vector2Int screenSize)
    {
        //MVP変換をする
        {
            // モデル変換行列（オブジェクト座標系からワールド座標系へ変換する）
            //http://marupeke296.sakura.ne.jp/DXG_No39_WorldMatrixInformation.html
            var posMatrix = new Matrix4x4(
                1, 0, 0, objectPos.X,
                0, 1, 0, objectPos.Y,
                0, 0, 1, objectPos.Z,
                0, 0, 0, 1
            );

            var objectRoteRadius = new Vector3(
                objectRotateDegree.X * (MathF.PI / 180),
                objectRotateDegree.Y * (MathF.PI / 180),
                objectRotateDegree.Z * (MathF.PI / 180)
            );
            //回転行列をどうやって表すのか
            // https://rikei-tawamure.com/entry/2019/11/04/184049
            var rotXMatrix = new Matrix4x4(
                1, 0, 0, 0,
                0, cos(objectRoteRadius.X), -sin(objectRoteRadius.X), 0,
                0, sin(objectRoteRadius.X), cos(objectRoteRadius.X), 0,
                0, 0, 0, 1
            );
            var rotYMatrix = new Matrix4x4(
                cos(objectRoteRadius.Y), 0, sin(objectRoteRadius.Y), 0,
                0, 1, 0, 0,
                -sin(objectRoteRadius.Y), 0, cos(objectRoteRadius.Y), 0,
                0, 0, 0, 1
            );
            var rotZMatrix = new Matrix4x4(
                cos(objectRoteRadius.Z), -sin(objectRoteRadius.Z), 0, 0,
                sin(objectRoteRadius.Z), cos(objectRoteRadius.Z), 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );


            var scaleMatrix = new Matrix4x4(
                objectScale.X, 0, 0, 0,
                0, objectScale.Y, 0, 0,
                0, 0, objectScale.Z, 0,
                0, 0, 0, 1
            );

            var a = posMatrix * rotYMatrix;
            var b = a * rotXMatrix;
            var c = b * rotZMatrix;
            var modelMatrix = c * scaleMatrix;

            foreach (var teaPodPoint in vertexDict.Values)
            {
                teaPodPoint.WorldPosition = MatrixUtil.Multi(modelMatrix, teaPodPoint.ModelPosition);
            }
        }


        {
            // ビュー変換行列 （ワールド座標からカメラ座標への変換）
            // https://yttm-work.jp/gmpg/gmpg_0003.html
            //http://marupeke296.com/DXG_No72_ViewProjInfo.html
            var cameraUp = new Vector3(0, 1, 0);

            var forward = Vector3Util.Normalize(cameraTarget - cameraPos);
            var right = Vector3Util.Normalize(Vector3.Cross(cameraUp, forward));
            var up = Vector3.Cross(forward, right);

            var viewMatrix = new Matrix4x4(
                right.X, right.Y, right.Z, -Vector3.Dot(right, cameraPos),
                up.X, up.Y, up.Z, -Vector3.Dot(up, cameraPos),
                forward.X, forward.Y, forward.Z, -Vector3.Dot(forward, cameraPos),
                0, 0, 0, 1);

            foreach (var teaPodPoint in vertexDict.Values)
            {
                teaPodPoint.ViewPosition = MatrixUtil.Multi(viewMatrix, teaPodPoint.WorldPosition);
            }
        }


        {
            //プロジェクション座標変換行列（カメラ座標からクリップ座標への変換）
            //https://yttm-work.jp/gmpg/gmpg_0004.html
            //http://marupeke296.com/DXG_No70_perspective.html
            const float viewAngle = 100 * (MathF.PI / 180);
            const float cameraNear = 0.1f;
            const float cameraFar = 100;
            var aspectRate = (float)screenSize.X / screenSize.Y;


            var perspectiveMatrix = new Matrix4x4(
                1 / (float)Math.Tan(viewAngle / 2) / aspectRate, 0, 0, 0,
                0, 1 / (float)Math.Tan(viewAngle / 2), 0, 0,
                0, 0, 1 / (cameraFar - cameraNear) * cameraFar, 1,
                0, 0, -cameraNear / (cameraFar - cameraNear) * cameraFar, 0
            );

            foreach (var teaPodPoint in vertexDict.Values)
            {
                teaPodPoint.ClipPosition = MatrixUtil.Multi(perspectiveMatrix, teaPodPoint.ViewPosition);
                
                //正規化デバイス座標系変換
                teaPodPoint.ClipPosition.X /= teaPodPoint.ClipPosition.W;
                teaPodPoint.ClipPosition.Y /= teaPodPoint.ClipPosition.W;
                teaPodPoint.ClipPosition.Z /= teaPodPoint.ClipPosition.W;
                teaPodPoint.ClipPosition.W /= teaPodPoint.ClipPosition.W;
            }

        }
        {
            //スクリーン座標に変換
            foreach (var teaPodPoint in vertexDict.Values)
            {
                var x = (int)((teaPodPoint.ClipPosition.X + 1) * screenSize.X / 2);
                var y = (int)((teaPodPoint.ClipPosition.Y + 1) * screenSize.Y / 2);
                teaPodPoint.ScreenPosition = new Vector2Int(x, y);
            }
        }
        
   
        return vertexDict;
    }

    private static float sin(float radian)
    {
        return (float)Math.Sin(radian);
    }

    private static float cos(float radian)
    {
        return (float)Math.Cos(radian);
    }
}