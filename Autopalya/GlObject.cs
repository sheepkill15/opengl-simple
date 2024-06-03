using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Autopalya;

public class GlObject(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, Material? material, GL gl, float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
{
    public uint Vao { get; } = vao;
    public uint Vertices { get; } = vertices;
    public uint Colors { get; } = colors;
    public uint Indices { get; } = indeces;
    public uint IndexArrayLength { get; } = indexArrayLength;

    private bool _disposed = false;
    
    public Material Material { get; } = material ?? Material.BaseMaterial;

    public readonly Box3D<float> BoundingBox = new()
    {
        Min = new Vector3D<float>(minX, minY, minZ),
        Max = new Vector3D<float>(maxX, maxY, maxZ)
    };

    internal void ReleaseGlObject()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        // always unbound the vertex buffer first, so no halfway results are displayed by accident
        gl.DeleteBuffer(Vertices);
        gl.DeleteBuffer(Colors);
        gl.DeleteBuffer(Indices);
        gl.DeleteVertexArray(Vao);
        material?.Dispose(gl);
    }
}