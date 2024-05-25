using System.Globalization;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Autopalya;

internal class ObjectResourceReader
{
    public static ModelObject CreateObjectFromResource(GL gl, string resourceName)
    {
        var objNormals = new List<float[]>();
        var objFaces = new List<int[,]>();
        var objUVs = new List<float[]>();
        var vertexTransformations = new List<ObjVertexTransformationData>();
        var hasNormals = false;
        Dictionary<string, Material> loadedMaterials = new();
        Material? lastMaterial = null;
        ModelObject createdObject = new(resourceName);
        string currentGroup = string.Empty;
        var fullResourceName = "Autopalya.Resources." + resourceName;
        
        using (var objStream = typeof(ObjectResourceReader).Assembly.GetManifestResourceStream(fullResourceName))
        using (var objReader = new StreamReader(objStream!))
        {
            while (!objReader.EndOfStream)
            {
                var line = objReader.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (line.Length <= 1)
                {
                    continue;
                }

                var lineClassifier = line[..line.IndexOf(' ')];
                var lineData = line[line.IndexOf(' ')..].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                switch (lineClassifier)
                {
                    case "v":
                        var vertex = new float[3];
                        for (var i = 0; i < vertex.Length; ++i)
                            vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                        vertexTransformations.Add(new ObjVertexTransformationData(
                            new Vector3D<float>(vertex[0], vertex[1], vertex[2]),
                            Vector3D<float>.Zero,
                            Vector2D<float>.Zero,
                            0
                        ));
                        break;
                    case "f":
                        var face = new int[lineData.Length, 3];
                        int index = 0;
                        foreach (var t in lineData)
                        {
                            face[index, 0] = -1;
                            face[index, 1] = -1;
                            face[index, 2] = -1;
                            var faceData = t.Split("/");

                            for (var j = 0; j < faceData.Length; ++j)
                            {
                                if (string.IsNullOrWhiteSpace(faceData[j]))
                                {
                                    face[index, j] = -1;
                                }
                                else
                                {
                                    face[index, j] = int.Parse(faceData[j], CultureInfo.InvariantCulture);
                                }
                            }

                            index++;
                        }

                        for (int i = 1; i < face.GetLength(0) - 1; i++)
                        {
                            var newFace = new[,]
                            {
                                {face[0, 0], face[0, 1], face[0, 2]},
                                {face[i, 0], face[i, 1], face[i, 2]},
                                {face[i+1, 0], face[i+1, 1], face[i+1, 2]}
                            };
                            objFaces.Add(newFace);
                        }
                        break;
                    case "vn":
                        var normal = new float[3];
                        for (var i = 0; i < normal.Length; ++i)
                            normal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                        hasNormals = true;
                        objNormals.Add(normal);
                        break;
                    case "vt":
                        var uv = new float[2];
                        for (var i = 0; i < uv.Length; i++)
                        {
                            uv[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                        }

                        objUVs.Add(uv);
                        break;
                    case "newmtl":
                        string name = lineData[0];
                        lastMaterial = new Material
                        {
                            Name = name
                        };
                        loadedMaterials.Add(name, lastMaterial);
                        break;
                    case "usemtl":
                        string matName = lineData[0];
                        if (loadedMaterials.TryGetValue(matName, out lastMaterial))
                        {
                        }

                        break;
                    case "Ka":
                        var ambient = new float[3];
                        for (var i = 0; i < ambient.Length; ++i)
                            ambient[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                        if (lastMaterial is not null)
                        {
                            lastMaterial.AmbientColor = new Vector3D<float>(ambient[0], ambient[1], ambient[2]);
                        }

                        break;
                    case "Kd":
                        var diffuse = new float[3];
                        for (var i = 0; i < diffuse.Length; ++i)
                            diffuse[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                        if (lastMaterial is not null)
                        {
                            lastMaterial.DiffuseColor = new Vector3D<float>(diffuse[0], diffuse[1], diffuse[2]);
                        }

                        break;
                    case "Ks":
                        var specular = new float[3];
                        for (var i = 0; i < specular.Length; ++i)
                            specular[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                        if (lastMaterial is not null)
                        {
                            lastMaterial.DiffuseColor = new Vector3D<float>(specular[0], specular[1], specular[2]);
                        }

                        break;
                    case "Ns":
                        var specularComponent = float.Parse(lineData[0], CultureInfo.InvariantCulture);
                        if (lastMaterial is not null)
                        {
                            lastMaterial.SpecularComponent = specularComponent;
                        }

                        break;
                    case "d":
                        var dissolve = float.Parse(lineData[0], CultureInfo.InvariantCulture);
                        if (lastMaterial is not null)
                        {
                            lastMaterial.Dissolve = dissolve;
                        }

                        break;
                    case "Tr":
                        var transparency = float.Parse(lineData[0], CultureInfo.InvariantCulture);
                        if (lastMaterial is not null)
                        {
                            lastMaterial.Dissolve = 1 - transparency;
                        }

                        break;
                    case "Tf":
                        var transmissionFilter = new float[3];
                        for (var i = 0; i < transmissionFilter.Length; ++i)
                            transmissionFilter[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                        if (lastMaterial is not null)
                        {
                            lastMaterial.DiffuseColor = new Vector3D<float>(transmissionFilter[0],
                                transmissionFilter[1], transmissionFilter[2]);
                        }

                        break;
                    case "Ni":
                        var opticalDensity = float.Parse(lineData[0], CultureInfo.InvariantCulture);
                        if (lastMaterial is not null)
                        {
                            lastMaterial.RefractionIndex = opticalDensity;
                        }

                        break;
                    case "map_Ka":
                        TextureLoader.LoadTexture(lastMaterial, Material.TextureType.Ambience, lineData[^1]);
                        break;
                    case "map_Kd":
                        TextureLoader.LoadTexture(lastMaterial, Material.TextureType.Diffuse, lineData[^1]);
                        break;
                    case "map_Ks":
                        TextureLoader.LoadTexture(lastMaterial, Material.TextureType.Specular, lineData[^1]);
                        break;
                    case "map_d":
                        TextureLoader.LoadTexture(lastMaterial, Material.TextureType.Alpha, lineData[^1]);
                        break;
                    case "map_bump" or "bump":
                        TextureLoader.LoadTexture(lastMaterial, Material.TextureType.Bump, lineData[^1]);
                        break;
                    case "disp":
                        TextureLoader.LoadTexture(lastMaterial, Material.TextureType.Displacement, lineData[^1]);
                        break;
                    case "decal":
                        TextureLoader.LoadTexture(lastMaterial, Material.TextureType.Decal, lineData[^1]);
                        break;
                    case "o":
                        string groupName = lineData[0];
                        if (string.IsNullOrEmpty(currentGroup))
                        {
                            currentGroup = groupName;
                            break;
                        }

                        var newObject = CreateObject(gl, hasNormals, objFaces, objNormals, objUVs,
                            vertexTransformations, lastMaterial);
                        
                        createdObject.AddModel(currentGroup, newObject);
                        objFaces.Clear();

                        currentGroup = groupName;
                        break;
                }
            }
        }

        if (objFaces.Count > 0)
        {
            var newObject = CreateObject(gl, hasNormals, objFaces, objNormals, objUVs, vertexTransformations,
                lastMaterial);
            createdObject.AddModel(currentGroup, newObject);
        }

        return createdObject;
    }

    private static readonly float[] VertexColors = {
                 1f, 1f, 1f, 1.0f
            };

    private static unsafe GlObject CreateObject(GL gl, bool hasNormals, List<int[,]> objFaces, IReadOnlyList<float[]> objNormals, IReadOnlyList<float[]> objUVs, List<ObjVertexTransformationData> vertexTransformations, Material? lastMaterial)
    {
        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;
        float minZ = float.PositiveInfinity;
        float maxZ = float.NegativeInfinity;
        
        foreach (var vertex in vertexTransformations)
        {
            minX = Math.Min(minX, vertex.Coordinates.X);
            maxX = Math.Max(maxX, vertex.Coordinates.X);
            minY = Math.Min(minY, vertex.Coordinates.Y);
            maxY = Math.Max(maxY, vertex.Coordinates.Y);
            minZ = Math.Min(minZ, vertex.Coordinates.Z);
            maxZ = Math.Max(maxZ, vertex.Coordinates.Z);
        }
        
        if (!hasNormals)
        {
            foreach (var objFace in objFaces)
            {
                var a = vertexTransformations[objFace[0, 0] - 1];
                var b = vertexTransformations[objFace[1, 0] - 1];
                var c = vertexTransformations[objFace[2, 0] - 1];

                var normal =
                    Vector3D.Normalize(Vector3D.Cross(b.Coordinates - a.Coordinates, c.Coordinates - a.Coordinates));

                a.UpdateNormalWithContributionFromAFace(normal);
                b.UpdateNormalWithContributionFromAFace(normal);
                c.UpdateNormalWithContributionFromAFace(normal);
            }
        }

        var glIndexArray = new List<uint>();
        var glVertices = new List<float>();
        var glColors = new List<float>();
        foreach (var objFace in objFaces)
        {
            // glIndexArray.Add((uint)(objFace[0, 0] - 1));
            // glIndexArray.Add((uint)(objFace[1, 0] - 1));
            // glIndexArray.Add((uint)(objFace[2, 0] - 1));
            for (int i = 0; i < objFace.GetLength(0); i++)
            {
                int uvIndex = objFace[i, 1];
                int normalIndex = objFace[i, 2];
                if (normalIndex > 0)
                {
                    Vector3D<float> normal = new Vector3D<float>
                    {
                        X = objNormals[normalIndex - 1][0],
                        Y = objNormals[normalIndex - 1][1],
                        Z = objNormals[normalIndex - 1][2],
                    };
                    vertexTransformations[objFace[i, 0] - 1].Normal = normal;
                }
                else if (hasNormals)
                {
                    Vector3D<float> normal = new Vector3D<float>
                    {
                        X = objNormals[objFace[i, 0] - 1][0],
                        Y = objNormals[objFace[i, 0] - 1][1],
                        Z = objNormals[objFace[i, 0] - 1][2],
                    };
                    vertexTransformations[objFace[i, 0] - 1].UpdateNormalWithContributionFromAFace(normal);
                }

                if (uvIndex > 0)
                {
                    Vector2D<float> uv = new Vector2D<float>
                    {
                        X = objUVs[uvIndex - 1][0],
                        Y = 1-objUVs[uvIndex - 1][1],
                    };
                    vertexTransformations[objFace[i, 0] - 1].UVs = uv;
                }
                else if (objUVs.Count > 0 && objUVs.Count > objFace[i, 0] - 1)
                {
                    Vector2D<float> uv = new Vector2D<float>
                    {
                        X = objUVs[objFace[i, 0] - 1][0],
                        Y = objUVs[objFace[i, 0] - 1][1],
                    };
                    vertexTransformations[objFace[i, 0] - 1].UVs = uv;
                }
            }

            var edge1 = vertexTransformations[objFace[1, 0] - 1].Coordinates -
                        vertexTransformations[objFace[0, 0] - 1].Coordinates;
            var edge2 = vertexTransformations[objFace[2, 0] - 1].Coordinates -
                        vertexTransformations[objFace[0, 0] - 1].Coordinates;
            var deltaUv1 = vertexTransformations[objFace[1, 0] - 1].UVs -
                           vertexTransformations[objFace[0, 0] - 1].UVs;
            var deltaUv2 = vertexTransformations[objFace[2, 0] - 1].UVs -
                        vertexTransformations[objFace[0, 0] - 1].UVs;
            float f = 1.0f / (deltaUv1.X * deltaUv2.Y - deltaUv2.X * deltaUv1.Y);
            var tangent = new Vector3D<float>
            {
                X = f * (deltaUv2.Y * edge1.X - deltaUv1.Y * edge2.X),
                Y = f * (deltaUv2.Y * edge1.Y - deltaUv1.Y * edge2.Y),
                Z = f * (deltaUv2.Y * edge1.Z - deltaUv1.Y * edge2.Z)
            };
            for (int i = 0; i < 3; i++)
            {
                vertexTransformations[objFace[i, 0] - 1].Tangent = tangent;
            }

            for (int i = 0; i < 3; i++)
            {
                var vertexTransformation = vertexTransformations[objFace[i, 0] - 1];
                glVertices.Add(vertexTransformation.Coordinates.X);
                glVertices.Add(vertexTransformation.Coordinates.Y);
                glVertices.Add(vertexTransformation.Coordinates.Z);
                glVertices.Add(vertexTransformation.Normal.X);
                glVertices.Add(vertexTransformation.Normal.Y);
                glVertices.Add(vertexTransformation.Normal.Z);
                glVertices.Add(vertexTransformation.UVs.X);
                glVertices.Add(vertexTransformation.UVs.Y);
                glVertices.Add(vertexTransformation.Tangent.X);
                glVertices.Add(vertexTransformation.Tangent.Y);
                glVertices.Add(vertexTransformation.Tangent.Z);

                glColors.AddRange(VertexColors);
                glIndexArray.Add((uint)glIndexArray.Count);
            }
        }

        // foreach (var vertexTransformation in vertexTransformations)
        // {
        //     glVertices.Add(vertexTransformation.Coordinates.X);
        //     glVertices.Add(vertexTransformation.Coordinates.Y);
        //     glVertices.Add(vertexTransformation.Coordinates.Z);
        //     glVertices.Add(vertexTransformation.Normal.X);
        //     glVertices.Add(vertexTransformation.Normal.Y);
        //     glVertices.Add(vertexTransformation.Normal.Z);
        //     glVertices.Add(vertexTransformation.UVs.X);
        //     glVertices.Add(vertexTransformation.UVs.Y);
        //     glVertices.Add(vertexTransformation.Tangent.X);
        //     glVertices.Add(vertexTransformation.Tangent.Y);
        //     glVertices.Add(vertexTransformation.Tangent.Z);
        //     glVertices.Add(vertexTransformation.Bitangent.X);
        //     glVertices.Add(vertexTransformation.Bitangent.Y);
        //     glVertices.Add(vertexTransformation.Bitangent.Z);
        //
        //     glColors.AddRange(VertexColors);
        // }


        var vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        const uint offsetPos = 0;
        const uint offsetNormals = offsetPos + 3 * sizeof(float);
        const uint offsetUVs = offsetNormals + 3 * sizeof(float);
        const uint offsetTangent = offsetUVs + 2 * sizeof(float);
        const uint vertexSize = offsetTangent + 3 * sizeof(float);

        var vertices = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertices);
        gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(),
            BufferUsageARB.StaticDraw);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, vertexSize, (void*)offsetNormals);
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetUVs);
        gl.EnableVertexAttribArray(3);
        gl.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTangent);
        gl.EnableVertexAttribArray(4);

        var colors = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, colors);
        gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(),
            BufferUsageARB.StaticDraw);
        gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
        gl.EnableVertexAttribArray(1);


        var indices = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, indices);
        gl.BufferData(BufferTargetARB.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndexArray.ToArray().AsSpan(),
            BufferUsageARB.StaticDraw);

        // make sure to unbind array buffer
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        var indexArrayLength = (uint)glIndexArray.Count;

        gl.BindVertexArray(0);

        return new GlObject(vao, vertices, colors, indices, indexArrayLength, lastMaterial, gl, minX, maxX, minY, maxY, minZ, maxZ);
    }
}