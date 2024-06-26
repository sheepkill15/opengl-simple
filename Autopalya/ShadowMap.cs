﻿using Silk.NET.OpenGL;

namespace Autopalya;

public class ShadowMap
{
    public const int Width = 2048;
    public const int Height = 2048;

    private uint _fbo;
    private uint _shadowMap;

    public unsafe void Init(GL gl)
    {
        _fbo = gl.GenFramebuffer();
        _shadowMap = gl.GenTexture();
        Console.WriteLine($"ShadowMap id: {_shadowMap} for FBO {_fbo}");
        gl.BindTexture(TextureTarget.Texture2D, _shadowMap);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.DepthComponent, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        float[] border = [1.0f, 1.0f, 1.0f, 1.0f];
        gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureBorderColor, (ReadOnlySpan<float>)border);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D, _shadowMap, 0);
        gl.DrawBuffer(DrawBufferMode.None);
        gl.ReadBuffer(ReadBufferMode.None);
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
        gl.BindTexture(TextureTarget.Texture2D, _shadowMap);
    }

    public void Dispose(GL gl)
    {
        gl.DeleteTexture(_shadowMap);
        gl.DeleteFramebuffer(_fbo);
    }
    
}