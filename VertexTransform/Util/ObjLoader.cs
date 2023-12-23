using System.Numerics;
using ObjLoader.Loader.Loaders;
using VertexTransform.Data;

namespace VertexTransform.Util;

public class ObjLoader
{
    public static (Dictionary<int, Vertex> vertices, List<List<int>> faces) LoadVertex(string path)
    {
        var obj = LoadObj(path);

        var vertices = LoadVertices(obj);
        
        var faces = LoadFaces(obj);
        
        return (vertices,faces);
    }

    private static LoadResult LoadObj(string path)
    {
        Console.WriteLine("LoadObj FullPath:" + Path.GetFullPath(path));
        
        var objLoaderFactory = new ObjLoaderFactory();
        return objLoaderFactory.Create().Load(new FileStream(path, FileMode.Open));
    }
    
    private static Dictionary<int,Vertex> LoadVertices(LoadResult obj)
    {
        var vertices = new Dictionary<int,Vertex>();

        for (var i = 0; i < obj.Vertices.Count; i++)
        {
            var vertex = obj.Vertices[i];
            vertices.Add(i+1, new Vertex()
            {
                VertexIndex = i+1,
                ModelPosition = new Vector4 
                {
                    X = vertex.X,
                    Y = vertex.Y,
                    Z = vertex.Z,
                    W = 1
                },
            });
        }

        return vertices;
    }
    
    private static List<List<int>> LoadFaces(LoadResult obj)
    {
        var faces = new List<List<int>>();

        foreach (var group in obj.Groups)
        {
            foreach (var face in group.Faces)
            {
                var faceIndexList = new List<int>();
                for (var j = 0; j < face.Count; j++)
                {
                    faceIndexList.Add(face[j].VertexIndex);
                }
                faces.Add(faceIndexList);
            }
        }

        return faces;
    }
}