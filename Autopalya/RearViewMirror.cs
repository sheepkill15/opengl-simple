using Silk.NET.OpenGL;

namespace Autopalya;

public class RearViewMirror
{
    
    public const int Width = 1024;
    public const int Height = 512;

    private uint _fbo;
    public uint _mirrorMap;
    private uint _depthMap;

    public unsafe void Init(GL gl)
    {
        _fbo = gl.GenFramebuffer();
        _mirrorMap = gl.GenTexture();
        _depthMap = gl.GenTexture();
        Console.WriteLine($"MirrorMap id: {_mirrorMap} for FBO {_fbo}");
        gl.BindTexture(TextureTarget.Texture2D, _mirrorMap);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        float[] border = [1.0f, 1.0f, 1.0f, 1.0f];
        gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureBorderColor, (ReadOnlySpan<float>)border);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, _mirrorMap, 0);
        gl.BindTexture(TextureTarget.Texture2D, _depthMap);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.DepthComponent, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureBorderColor, (ReadOnlySpan<float>)border);
        
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D, _depthMap, 0);
        
        var status = gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != GLEnum.FramebufferComplete)
        {
            Console.WriteLine("Failed to create framebuffer");
        }
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void BindForWriting(GL gl)
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        gl.Viewport(0, 0, Width, Height);
    }

    public void BindForReading(GL gl, TextureUnit unit)
    {
        gl.ActiveTexture(unit);
        gl.BindTexture(TextureTarget.Texture2D, _mirrorMap);
    }

    public void Dispose(GL gl)
    {
        gl.DeleteTexture(_mirrorMap);
        gl.DeleteTexture(_depthMap);
        gl.DeleteFramebuffer(_fbo);
    }
    
}