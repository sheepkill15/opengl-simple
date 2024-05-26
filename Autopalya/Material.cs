using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Autopalya;

public class Material
{
    public enum TextureType
    {
        Ambience,
        Diffuse,
        Specular,
        Alpha,
        Bump,
        Displacement,
        Decal
    }

    public string Name { get; init; } = "Base";
    public Vector3D<float> AmbientColor { get; set; } = new(1f, 1f, 1f);
    public Vector3D<float> DiffuseColor { get; set; } = new(1f, 1f, 1f);
    public Vector3D<float> SpecularColor { get; set; } = new(1f, 1f, 1f);
    public float SpecularComponent { get; set; } = 10;
    public float Dissolve { get; set; } = 1.0f;
    public Vector3D<float> TransmissionFilterColor { get; set; } = Vector3D<float>.Zero;
    public float RefractionIndex { get; set; }

    private readonly Dictionary<TextureType, uint> _textureMap = new();

    private static readonly string[] TextureUniformNames = Enum.GetNames<TextureType>().Select(x => x + "Texture").ToArray();
    private const string AmbientUniformName = "AmbientColor";
    private const string DiffuseUniformName = "DiffuseColor";
    private const string SpecularUniformName = "SpecularColor";
    private const string SComponentUniformName = "SpecularComponent";
    private const string DissolveUniformName = "Dissolve";
    private const string TransmissionFilterUniformName = "TransmissionFilterColor";
    private const string RefractionIndexUniformName = "RefractionIndex";

    public static readonly Material BaseMaterial = new();

    private bool _disposed;

    public void AddTexture(GL gl, TextureType type, ImageResult imageResult)
    {
        var texture = gl.GenTexture();
        if (_textureMap.TryGetValue(type, out var value))
        {
            gl.DeleteTexture(value);
        }

        var textureBytes = (ReadOnlySpan<byte>)imageResult.Data.AsSpan();
        // gl.ActiveTexture(TextureUnit.Texture0 + (int)type);
        // bind texture
        gl.BindTexture(TextureTarget.Texture2D, texture);
            
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)imageResult.Width,
            (uint)imageResult.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureBytes);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        gl.BindTexture(TextureTarget.Texture2D, 0);
        _textureMap.Add(type, texture);
        Console.WriteLine($"Got texture {Name}.{type} - {texture}");
    }

    public void Bind(GL gl, Shader shader)
    {
        shader.Use(gl);
        foreach (var textureType in Enum.GetValues<TextureType>())
        {
            var textureLoc = shader.GetUniformLocation(gl, TextureUniformNames[(int)textureType]);

            if (textureLoc <= 0)
            {
                continue;
            }
            gl.ActiveTexture(TextureUnit.Texture0 + (int)textureType);
            Program.CheckError();
            if (_textureMap.TryGetValue(textureType, out var value))
            {
                if (value <= 0) continue;
                gl.BindTexture(TextureTarget.Texture2D, value);
                Program.CheckError();
                gl.Uniform1(textureLoc, (int)textureType);
                Program.CheckError();
            }
            else
            {
                gl.BindTexture(TextureTarget.Texture2D, 0);
                gl.Uniform1(textureLoc, 0);
                Program.CheckError();
            }
        }

        var ambientLocation = shader.GetUniformLocation(gl, AmbientUniformName);

        if (ambientLocation > 0)
        {
            gl.Uniform3(ambientLocation, AmbientColor.X, AmbientColor.Y, AmbientColor.Z);
            Program.CheckError();
        }

        var diffuseLocation = shader.GetUniformLocation(gl, DiffuseUniformName);

        if (diffuseLocation > 0)
        {
            gl.Uniform3(diffuseLocation, DiffuseColor.X, DiffuseColor.Y, DiffuseColor.Z);
            Program.CheckError();
        }

        var specularLocation = shader.GetUniformLocation(gl, SpecularUniformName);

        if (specularLocation > 0)
        {
            gl.Uniform3(specularLocation, SpecularColor.X, SpecularColor.Y, SpecularColor.Z);
            Program.CheckError();
        }

        var sComponentLoc = shader.GetUniformLocation(gl, SComponentUniformName);

        if (sComponentLoc > 0)
        {
            gl.Uniform1(sComponentLoc, SpecularComponent);
            Program.CheckError();
        }

        var dissolveLoc = shader.GetUniformLocation(gl, DissolveUniformName);
        if (dissolveLoc > 0)
        {
            gl.Uniform1(dissolveLoc, Dissolve);
            Program.CheckError();
        }

        var refrIndexLoc = shader.GetUniformLocation(gl, RefractionIndexUniformName);
        if (refrIndexLoc > 0)
        {
            gl.Uniform1(refrIndexLoc, RefractionIndex);
            Program.CheckError();
        }

        var trFcLoc = shader.GetUniformLocation(gl, TransmissionFilterUniformName);
        if (trFcLoc > 0)
        {
            gl.Uniform3(trFcLoc, TransmissionFilterColor.X, TransmissionFilterColor.Y, TransmissionFilterColor.Z);
            Program.CheckError();
        }
        
    }

    public void Dispose(GL gl)
    {
        if (_disposed) return;
        foreach (var (key, value) in _textureMap)
        {
            gl.DeleteTexture(value);
        }

        _disposed = true;
    }
}