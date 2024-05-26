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
    public ShadowMap ShadowMap { get; } = new();

    public void InitShadowMap(GL gl)
    {
        ShadowMap.Init(gl);
    }

    public Matrix4X4<float> GetLightSpaceMatrix()
    {
        var projection = Matrix4X4.CreatePerspectiveFieldOfView(OuterCutOff, 1f, 0.1f, 20f);
        var view = projection * Matrix4X4.CreateLookAt(Position, Position + Direction, Vector3D<float>.UnitY);
        return view;
    }

    public void Dispose(GL gl)
    {
        ShadowMap.Dispose(gl);
    }
}