using System.Globalization;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Autopalya;

public class SceneReader
{
    public static Scene ReadSceneFromResource(GL gl, string resourceName)
    {
        var fullResourceName = "Autopalya.Resources." + resourceName;
        var newScene = new Scene();
        using var objStream = typeof(SceneReader).Assembly.GetManifestResourceStream(fullResourceName);
        using var objReader = new StreamReader(objStream!);
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
                case "m":
                    var model = ObjectResourceReader.CreateObjectFromResource(gl, lineData[0]);
                    newScene.Objects.Add(model);
                    if (lineData.Length == 1)
                    {
                        break;
                    }
                    foreach (var setting in lineData[1..])
                    {
                        var keyvalue = setting.Split('=');
                        switch (keyvalue[0])
                        {
                            case "p":
                                var pos = keyvalue[1].Split(',');
                                var newPos = new Vector3D<float>
                                {
                                    X = float.Parse(pos[0], CultureInfo.InvariantCulture),
                                    Y = float.Parse(pos[1], CultureInfo.InvariantCulture),
                                    Z = float.Parse(pos[2], CultureInfo.InvariantCulture)
                                };
                                model.SetPosition(newPos);
                                break;
                            case "s":
                                var scale = keyvalue[1].Split(',');
                                var newScale = new Vector3D<float>
                                {
                                    X = float.Parse(scale[0], CultureInfo.InvariantCulture),
                                    Y = float.Parse(scale[1], CultureInfo.InvariantCulture),
                                    Z = float.Parse(scale[2], CultureInfo.InvariantCulture)
                                };
                                model.SetScale(newScale);
                                break;
                            case "r":
                                var rot = keyvalue[1].Split(',');
                                var newRot = new Vector3D<float>
                                {
                                    X = float.Parse(rot[0], CultureInfo.InvariantCulture),
                                    Y = float.Parse(rot[1], CultureInfo.InvariantCulture),
                                    Z = float.Parse(rot[2], CultureInfo.InvariantCulture)
                                };
                                model.SetRotation(newRot);
                                break;
                        }
                    }
                    break;
                case "l":
                    Light light = new Light();
                    foreach (var setting in lineData)
                    {
                        var keyvalue = setting.Split('=');
                        switch (keyvalue[0])
                        {
                            case "c":
                                var rgb = keyvalue[1].Split(',');
                                light.Color.X = float.Parse(rgb[0], CultureInfo.InvariantCulture);
                                light.Color.Y = float.Parse(rgb[1], CultureInfo.InvariantCulture);
                                light.Color.Z = float.Parse(rgb[2], CultureInfo.InvariantCulture);
                                break;
                            case "p":
                                var pos = keyvalue[1].Split(',');
                                light.Position.X = float.Parse(pos[0], CultureInfo.InvariantCulture);
                                light.Position.Y = float.Parse(pos[1], CultureInfo.InvariantCulture);
                                light.Position.Z = float.Parse(pos[2], CultureInfo.InvariantCulture);
                                break;
                            case "d":
                                var dir = keyvalue[1].Split(',');
                                light.Direction.X = float.Parse(dir[0], CultureInfo.InvariantCulture);
                                light.Direction.Y = float.Parse(dir[1], CultureInfo.InvariantCulture);
                                light.Direction.Z = float.Parse(dir[2], CultureInfo.InvariantCulture);
                                break;
                            case "ic":
                                var value = float.Parse(keyvalue[1], CultureInfo.InvariantCulture);
                                light.InnerCutOff = value;
                                break;
                            case "oc":
                                var value2 = float.Parse(keyvalue[1], CultureInfo.InvariantCulture);
                                light.OuterCutOff = value2;
                                break;
                            case "i":
                                var intensity = float.Parse(keyvalue[1], CultureInfo.InvariantCulture);
                                light.Intensity = intensity;
                                break;
                            case "f":
                                var falloff = float.Parse(keyvalue[1], CultureInfo.InvariantCulture);
                                light.AttenuationFalloff = falloff;
                                break;
                        }
                    }
                    newScene.Lights.Add(light);
                    break;
                case "s":
                    var texturePath = lineData[0];
                    Material skyboxMat = new Material
                    {
                        Name = "Skybox",
                        SpecularColor = Vector3D<float>.Zero,
                        AmbientColor = Vector3D<float>.One * 33
                    };
                    TextureLoader.LoadTexture(skyboxMat, Material.TextureType.Diffuse, texturePath);
                    GlObject newObj = GlCube.CreateInteriorCube(gl, skyboxMat);
                    ModelObject skybox = new ModelObject(texturePath);
                    skybox.SetScale(Vector3D<float>.One * 400);
                    skybox.AddModel("skybox", newObj);
                    newScene.SkyBox = skybox;
                    break;
                case "c":
                    foreach (var setting in lineData)
                    {
                        var keyvalue = setting.Split('=');
                        switch (keyvalue[0])
                        {
                            case "p":
                                var pos = keyvalue[1].Split(',');
                                var newPos = new Vector3D<float>
                                {
                                    X = float.Parse(pos[0], CultureInfo.InvariantCulture),
                                    Y = float.Parse(pos[1], CultureInfo.InvariantCulture),
                                    Z = float.Parse(pos[2], CultureInfo.InvariantCulture)
                                };
                                newScene.Camera.Position = newPos;
                                break;
                            case "d":
                                var dir = keyvalue[1].Split(',');
                                var newDir = new Vector3D<float>
                                {
                                    X = float.Parse(dir[0], CultureInfo.InvariantCulture),
                                    Y = float.Parse(dir[1], CultureInfo.InvariantCulture),
                                    Z = float.Parse(dir[2], CultureInfo.InvariantCulture)
                                };
                                newScene.Camera.Rotation = newDir;
                                break;
                            case "f":
                                var fov = float.Parse(keyvalue[1], CultureInfo.InvariantCulture);
                                newScene.Camera.Fov = fov;
                                break;
                            case "ms":
                                var ms = float.Parse(keyvalue[1], CultureInfo.InvariantCulture);
                                newScene.Camera.MoveSpeed = ms;
                                break;
                            case "rs":
                                var rs = float.Parse(keyvalue[1], CultureInfo.InvariantCulture);
                                newScene.Camera.RotateSpeed = rs;
                                break;
                        }
                    }
                    break;
            }
        }

        return newScene;
    }
}