using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Autopalya;

public class ModelObject
{
    private Dictionary<string, GlObject> _submodels = new();
    public readonly string OriginFile;

    private float _boundingBoxScale = 1f;
    
    public Box3D<float> BoundingBox { get; private set; }

    public Box3D<float> CollisionBox => BoundingBox.GetTranslated(-BoundingBox.Center)
        .GetScaled(new Vector3D<float>(_boundingBoxScale) * Scale, Vector3D<float>.Zero).GetTranslated(Position);
    
    public Matrix4X4<float> OwnModelMatrix { get; private set; } = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _originalModelMatrix = Matrix4X4<float>.Identity;

    public Vector3D<float> Position {get; private set;} = Vector3D<float>.Zero;
    public Vector3D<float> Scale {get; private set;} = Vector3D<float>.One;
    public Vector3D<float> Rotation {get; private set;} = Vector3D<float>.Zero;
    
    public Vector3D<float> TextureScale { get; private set; } = Vector3D<float>.One;

    public ModelObject(string originFile)
    {
        this.OriginFile = originFile;
    }

    public static ModelObject CreateFrom(ModelObject other)
    {
        var newCar = new ModelObject(other.OriginFile)
        {
            _submodels = other._submodels,
            _boundingBoxScale = other._boundingBoxScale,
            _originalModelMatrix = other._originalModelMatrix,
            Scale = other.Scale,
            TextureScale = other.TextureScale,
            BoundingBox = other.BoundingBox
        };
        return newCar;
    }
    
    public unsafe void Draw(GL gl, Shader shader, bool onlyTransparent, bool setNormal = true)
    {
        shader.Use(gl);
        Scene.SetModelMatrix(gl, shader, OwnModelMatrix, setNormal);
        var texScaleLoc = shader.GetUniformLocation(gl, "TextureScale");
        if (texScaleLoc >= 0)
        {
            gl.Uniform2(texScaleLoc, TextureScale.X, TextureScale.Y);
        }
        foreach (var (_, model) in _submodels)
        {
            switch (onlyTransparent)
            {
                case true when Math.Abs(model.Material.Dissolve - 1.0f) < 0.001f && !model.Material.HasTexture(Material.TextureType.Alpha):
                case false when Math.Abs(model.Material.Dissolve - 1.0f) > 0.001f || model.Material.HasTexture(Material.TextureType.Alpha):
                    continue;
            }

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
        OwnModelMatrix = _originalModelMatrix * Matrix4X4.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * 
                        Matrix4X4.CreateScale(Scale) * Matrix4X4.CreateTranslation(new Vector3D<float>(-Position.X, Position.Y, -Position.Z));
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
        _boundingBoxScale = 1 / biggestSize;
        _originalModelMatrix = Matrix4X4.CreateScale(_boundingBoxScale) * Matrix4X4.CreateTranslation(-BoundingBox.Center / biggestSize);
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
    
    public void SetTextureScale(Vector3D<float> scale)
    {
        TextureScale = scale;
    }
    
    
    public Vector3D<float> Forward => new()
    {
        X = MathF.Sin(Rotation.Y) * MathF.Cos(Rotation.X),
        Y = MathF.Sin(Rotation.X),
        Z = MathF.Cos(Rotation.Y) * MathF.Cos(Rotation.X)
    };

    public Vector3D<float> Right => Vector3D.Cross(Forward, Up);
        
    public Vector3D<float> Up => Vector3D<float>.UnitY;
}