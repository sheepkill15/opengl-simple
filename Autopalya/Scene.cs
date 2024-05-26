using System.Numerics;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Autopalya;

public class Scene
{
    private const string ModelMatrixVariableName = "uModel";
    private const string NormalMatrixVariableName = "uNormal";
    private const string ViewMatrixVariableName = "uView";
    private const string ProjectionMatrixVariableName = "uProjection";
    private const string ViewPosVariableName = "viewPos";
    private const string LightSpaceMatrixVariableName = "lightSpaceMatrix";
    private const string ShadowMapVariableName = "shadowMap";
    
    public readonly CameraDescriptor Camera = new();
    public readonly List<ModelObject> Objects = new();
    public readonly List<Light> Lights = new();

    public ModelObject? SkyBox;
    
    private static uint _lightSsbo;

    private static Shader? _shadowShader;

    public void Init(GL gl, Shader shader)
    {
        SetProjectionMatrix(gl, shader);
        SetupLightSsbo(gl, shader);
        _shadowShader ??= Shader.LoadShadersFromResource(gl, "ShadowVertexShader.vert", "ShadowFragmentShader.frag");
        gl.UseProgram(_shadowShader.Program);
        foreach (var light in Lights)
        {
            light.InitShadowMap(gl);
        }
    }
    
    public unsafe void Draw(GL gl, Shader shader)
    {
        gl.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
        int[] oldViewport = new int[4];
        gl.GetInteger(GetPName.Viewport, (Span<int>)oldViewport);
        Lights[0].ShadowMap.BindForWriting(gl);
        _shadowShader!.Use(gl);
        var lightSpaceMatrix = Lights[0].GetLightSpaceMatrix();
        gl.UniformMatrix4(_shadowShader.GetUniformLocation(gl, ViewMatrixVariableName), 1, false, (float*)&lightSpaceMatrix);
        // gl.Clear(ClearBufferMask.DepthBufferBit);
        // // Program.CheckError();
        // // // Program.CheckError();
        // foreach (var obj in Objects)
        // {
        //     obj.Draw(gl, _shadowShader, false);
        // }
        // gl.Clear(ClearBufferMask.DepthBufferBit);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        // Program.CheckError();
        shader.Use(gl);
        // Program.CheckError();
        gl.UniformMatrix4(shader.GetUniformLocation(gl, LightSpaceMatrixVariableName), 1, false, (float*)&lightSpaceMatrix);
        // Program.CheckError();
        gl.Viewport(oldViewport[0], oldViewport[1], (uint)oldViewport[2], (uint)oldViewport[3]);
        // Program.CheckError();
        //
        Lights[0].ShadowMap.BindForReading(gl, TextureUnit.Texture15);
        // Program.CheckError();
        gl.Uniform1(shader.GetUniformLocation(gl, ShadowMapVariableName), 15);
        // Program.CheckError();
        //
        lock (TextureLoader.LoadedTextures)
        {
            var textureCount = TextureLoader.LoadedTextures.Count;
            for (int i = 0; i < textureCount; i++)
            {
                var (mat, type, res) = TextureLoader.LoadedTextures[i];
                mat.AddTexture(gl, type, res);
            }
            TextureLoader.LoadedTextures.RemoveRange(0, textureCount);
        }
        gl.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
        SetViewerPosition(gl, shader);
        SetViewMatrix(gl, shader);
        SkyBox?.Draw(gl, shader);
        foreach (var obj in Objects)
        {
            obj.Draw(gl, shader);
        }
    }

    private void SetupLightSsbo(GL gl, Shader shader)
    {
        shader.Use(gl);
        List<float> lightData = new();
        foreach (var light in Lights)
        {
            lightData.Add(light.Color.X);
            lightData.Add(light.Color.Y);
            lightData.Add(light.Color.Z);
            lightData.Add(0f);
            lightData.Add(light.Position.X);
            lightData.Add(light.Position.Y);
            lightData.Add(light.Position.Z);
            lightData.Add(0f);
            lightData.Add(light.Direction.X);
            lightData.Add(light.Direction.Y);
            lightData.Add(light.Direction.Z);
            lightData.Add((float)Math.Cos(light.InnerCutOff));
            lightData.Add((float)Math.Cos(light.OuterCutOff));
            lightData.Add(light.Intensity);
            lightData.Add(light.AttenuationFalloff);
            lightData.Add(0f);
        }

        shader.Use(gl);
        _lightSsbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ShaderStorageBuffer, _lightSsbo);
        gl.BufferData(BufferTargetARB.ShaderStorageBuffer, (ReadOnlySpan<float>)lightData.ToArray().AsSpan(),
            BufferUsageARB.StaticDraw);
        gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 3, _lightSsbo);
        gl.BindBuffer(BufferTargetARB.ShaderStorageBuffer, 0);
        gl.GetInteger(GLEnum.ShaderStorageBufferBinding, 3, out var boundBuffer);
        if (boundBuffer != _lightSsbo)
        {
            Console.WriteLine("SSBO not bound correctly.");
        }
        Program.CheckError();
    }
    
    private unsafe void SetProjectionMatrix(GL gl, Shader shader)
    {
        shader.Use(gl);
        var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(Camera.Fov, 1280f / 720f, 0.1f, 1000);
        gl.UniformMatrix4(shader.GetUniformLocation(gl, ProjectionMatrixVariableName), 1, false, (float*)&projectionMatrix);
        Program.CheckError();
    }
    
    private void SetViewerPosition(GL gl, Shader shader)
    {
        shader.Use(gl);
        gl.Uniform3(shader.GetUniformLocation(gl, ViewPosVariableName), Camera.Position.X, Camera.Position.Y, Camera.Position.Z);
        Program.CheckError();
    }
    private unsafe void SetViewMatrix(GL gl, Shader shader)
    {
        shader.Use(gl);
        var viewMatrix =
            Matrix4X4.CreateTranslation(Camera.Position) *
            Matrix4X4.CreateRotationY(Camera.Rotation.X) *
            Matrix4X4.CreateRotationX(Camera.Rotation.Y);
        
        gl.UniformMatrix4(shader.GetUniformLocation(gl, ViewMatrixVariableName), 1, false, (float*)&viewMatrix);
        Program.CheckError();
    }

    public static unsafe void SetModelMatrix(GL gl, Shader shader, Matrix4X4<float> modelMatrix, bool setNormal = true)
    {
        shader.Use(gl);

        gl.UniformMatrix4(shader.GetUniformLocation(gl, ModelMatrixVariableName), 1, false, (float*)&modelMatrix);
        Program.CheckError();

        if (!setNormal) return;

        var modelMatrixWithoutTranslation =
            new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4)
            {
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 1
            };

        Matrix4X4.Invert(modelMatrixWithoutTranslation, out var modelInvers);
        var normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
        
        gl.UniformMatrix3(shader.GetUniformLocation(gl, NormalMatrixVariableName), 1, false, (float*)&normalMatrix);
        Program.CheckError();
    }
    public void Dispose(GL gl)
    {
        foreach (var modelObject in Objects)
        {
            modelObject.Dispose();
        }
        foreach (var light in Lights)
        {
            light.Dispose(gl);
        }
        gl.DeleteBuffer(_lightSsbo);
    }
}