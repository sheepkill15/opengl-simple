using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Autopalya;

public class Light
{
    public Vector3D<float> Color;
    public Vector3D<float> Position;
    public Vector3D<float> Direction;
    public float InnerCutOff;
    public float OuterCutOff;
    public float Intensity;
    public float AttenuationFalloff;

    public const int ShadowWidth = 1024;
    public const int ShadowHeight = 1024;
    private uint _shadowMapFbo;
    public uint ShadowMap { get; private set; }

    public unsafe void InitShadowMap(GL gl)
    {
        // Generate framebuffer
        _shadowMapFbo = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _shadowMapFbo);

        // Generate depth texture
        ShadowMap = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, ShadowMap);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.DepthComponent, ShadowWidth, ShadowHeight, 0, PixelFormat.DepthComponent, PixelType.Float, null);

        // Set texture parameters
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        // Set border color
        float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

        // Attach depth texture as FBO's depth buffer
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, ShadowMap, 0);
        gl.DrawBuffer(DrawBufferMode.None);
        gl.ReadBuffer(ReadBufferMode.None);

        // Check if framebuffer is complete
        if (gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            Console.WriteLine("Framebuffer not complete");
        }

        // Unbind framebuffer
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public Matrix4X4<float> GetLightSpaceMatrix()
    {
        var projection = Matrix4X4.CreatePerspectiveFieldOfView(OuterCutOff, 1f, 0.1f, 20f);
        var view = Matrix4X4.CreateLookAt(Position, Vector3D<float>.Zero, Vector3D<float>.UnitY);
        return projection * view;
    }

    public void BindFramebuffer(GL gl)
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _shadowMapFbo);
        gl.Viewport(0, 0, ShadowWidth, ShadowHeight);
        gl.Clear(ClearBufferMask.DepthBufferBit); // Clear depth buffer for shadow mapping
    }

    public void Dispose(GL gl)
    {
        gl.DeleteTexture(ShadowMap);
        gl.DeleteFramebuffer(_shadowMapFbo);
    }
}