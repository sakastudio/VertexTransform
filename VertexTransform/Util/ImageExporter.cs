using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VertexTransform.Data;

namespace VertexTransform.Util;

public class ImageExporter
{
    public static void ExportImage(Rgba32[,] pixels, string path)
    {
        var width = pixels.GetLength(0);
        var height = pixels.GetLength(1);
        
        using var image = new Image<Rgba32>(width,height);

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++) 
            {
                image[x, y] = pixels[x, y];
            }
        }

        //export
        image.Save(path);
        
        Console.WriteLine("画像を出力しました :" + Path.GetFullPath(path));
    }
}