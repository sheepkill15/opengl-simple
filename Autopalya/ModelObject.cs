using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Autopalya;

public class ModelObject
{
    private readonly Dictionary<string, GlObject> _submodels = new();
    public readonly string OriginFile;

    public Box3D<float> BoundingBox { get; private set; }
    public Matrix4X4<float> OwnModelMatrix { get; private set; } = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _originalModelMatrix = Matrix4X4<float>.Identity;

    public Vector3D<float> Position {get; private set;} = Vector3D<float>.Zero;
    public Vector3D<float> Scale {get; private set;} = Vector3D<float>.One;
    public Vector3D<float> Rotation {get; private set;} = Vector3D<float>.Zero;

    public ModelObject(string originFile)
    {
        this.OriginFile = originFile;
    }

    public unsafe void Draw(GL gl, Shader shader, bool setNormal = true)
    {
        shader.Use(gl);
        Scene.SetModelMatrix(gl, shader, OwnModelMatrix, setNormal);
        foreach (var (_, model) in _submodels)
        {
            gl.BindVertexArray(model.Vao);
            model.Material.Bind(gl, shader);
            gl.DrawElements(GLEnum.Triangles, model.IndexArrayLength, GLEnum.UnsignedInt, null);
            gl.BindVertexArray(0);
        }
    }

    public void Dispose()
    {
        foreach (var (_, model) in _submodels)
        {
            model.ReleaseGlObject();
        }
    }

    private void RecalculateModelMatrix()
    {
        OwnModelMatrix = Matrix4X4.CreateTranslation(Position) * Matrix4X4.CreateScale(Scale) *
                          Matrix4X4.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z)
                          * _originalModelMatrix;
    }

    public void AddModel(string name, GlObject obj)
    {
        if (!_submodels.TryAdd(name, obj))
        {
            return;
        }

        var max = new Vector3D<float>
        {
            X = Math.Max(BoundingBox.Max.X, obj.BoundingBox.Max.X),
            Y = Math.Max(BoundingBox.Max.Y, obj.BoundingBox.Max.Y),
            Z = Math.Max(BoundingBox.Max.Z, obj.BoundingBox.Max.Z),
        };
        var min = new Vector3D<float>
        {
            X = Math.Min(BoundingBox.Min.X, obj.BoundingBox.Min.X),
            Y = Math.Min(BoundingBox.Min.Y, obj.BoundingBox.Min.Y),
            Z = Math.Min(BoundingBox.Min.Z, obj.BoundingBox.Min.Z),
        };
        BoundingBox = new Box3D<float>
        {
            Max = max,
            Min = min
        };
        float biggestSize = Math.Max(BoundingBox.Size.X, Math.Max(BoundingBox.Size.Y, BoundingBox.Size.Z));
        _originalModelMatrix = Matrix4X4.CreateScale(1 / biggestSize) * Matrix4X4.CreateTranslation(-BoundingBox.Center / biggestSize);

        RecalculateModelMatrix();
    }

    public void SetPosition(Vector3D<float> pos)
    {
        Position = pos;
        RecalculateModelMatrix();
    }

    public void SetScale(Vector3D<float> scale)
    {
        Scale = scale;
        RecalculateModelMatrix();
    }

    public void SetRotation(Vector3D<float> rot)
    {
        Rotation = rot;
        RecalculateModelMatrix();
    }
    
}