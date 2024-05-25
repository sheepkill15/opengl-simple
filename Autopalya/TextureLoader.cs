using StbImageSharp;

namespace Autopalya;

public static class TextureLoader
{
    public static readonly List<(Material, Material.TextureType, ImageResult)> LoadedTextures = new();

    public static void LoadTexture(Material mat, Material.TextureType type, string file)
    {
        Task.Run(() =>
        {
            var imageResult = ReadTextureImage(file);
            lock (LoadedTextures)
            {
                LoadedTextures.Add((mat, type, imageResult));
            }
            Console.WriteLine($"Loaded texture {file}");
        });
    }
    
    private static ImageResult ReadTextureImage(string textureResource)
    {
        using var textureStream
            = typeof(ObjectResourceReader).Assembly.GetManifestResourceStream("Autopalya.Resources." + textureResource.Replace('/', '.'));
        
        var result = ImageResult.FromStream(textureStream, ColorComponents.RedGreenBlueAlpha);

        return result;
    }
}