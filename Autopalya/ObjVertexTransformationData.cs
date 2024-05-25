using Silk.NET.Maths;

namespace Autopalya;

internal class ObjVertexTransformationData(Vector3D<float> coordinates, Vector3D<float> initialNormal, Vector2D<float> uvs,
    int aggregatedFaceCount)
{
    public Vector3D<float> Coordinates = coordinates;
    public Vector2D<float> UVs = uvs;
    public Vector3D<float> Tangent = Vector3D<float>.Zero;

    public Vector3D<float> Normal { get; set; } = Vector3D.Normalize(initialNormal);

    internal void UpdateNormalWithContributionFromAFace(Vector3D<float> normal)
    {
        var newNormalToNormalize = aggregatedFaceCount == 0
            ? normal
            : (aggregatedFaceCount * Normal + normal) / (aggregatedFaceCount + 1);

        var newNormal = Vector3D.Normalize(newNormalToNormalize);
        Normal = newNormal;
        ++aggregatedFaceCount;
    }
}