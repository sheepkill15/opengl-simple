using StbImageSharp;

namespace Autopalya;

public static class TextureLoader
{
    public static readonly List<(Material, Material.TextureType, ImageResult, string)> LoadedTextures = new();

    private static readonly Dictionary<string, uint> GlTextures = new();

    public static void AddGlTexture(string path, uint bindPoint)
    {
        GlTextures.TryAdd(path, bindPoint);
    }

    public static uint GetGlTexture(string path)
    {
        return GlTextures.TryGetValue(path, out var val) ? val : 0;
    }

    public static void LoadTexture(Material mat, Material.TextureType type, string file)
    {
        var loc = GetGlTexture(file);
        if (loc > 0)
        {
            mat.AddLoadedTexture(type, loc);
            return;
        }
        Task.Run(() =>
        {
            var imageResult = ReadTextureImage(file);
            lock (LoadedTextures)
            {
                LoadedTextures.Add((mat, type, imageResult, file));
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