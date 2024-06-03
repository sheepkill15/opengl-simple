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
    public Matrix4X4<float> GetViewMatrix()
    {
        return Matrix4X4.CreateLookAt(Position, Position + Direction, Vector3D<float>.UnitY);
        // return Matrix4X4.CreateLookAt(Vector3D<float>.Zero, Direction, Vector3D<float>.UnitY);
    }
    
    public Matrix4X4<float> GetProjectionMatrix()
    {
        return Matrix4X4.CreateOrthographic(10f, 10f, 1f, 10f);
        // return Matrix4X4.CreatePerspectiveFieldOfView(OuterCutOff, 1f, 0.1f, 10f);
    }

    public void Dispose(GL gl)
    {
        ShadowMap.Dispose(gl);
    }

}