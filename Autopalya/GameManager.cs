using System.Numerics;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Autopalya;

public class GameManager
{
    private static GameManager? _instance;

    private ModelObject _mainCar = null!;
    private ModelObject _npcCar = null!;
    private ModelObject _house = null!;
    private List<ModelObject> _roads = null!;

    private Scene _scene = null!;
    public RearViewMirror Mirror { get; private set; } = null!;

    private readonly List<ModelObject> _allNpcCars = new();
    private readonly List<ModelObject> _allHouses = new();

    private float _npcSpeed = 3.5f;
    private float _spawnRate = 2f;
    private int _hp = 3;
    private int _nearMisses;

    public static float Score;

    private float _maxZ;
    private float _timeOpposite;

    private GameManager()
    {
        
    }

    public void Init(GL gl)
    {
        _scene = SceneReader.ReadSceneFromResource(gl, "scenes.main.sc");
        _scene.Init(gl);
        _mainCar = _scene.Objects.Find(x => x.OriginFile.Contains("rat.rat.obj"))!;
        _npcCar = _scene.Objects.Find(x => x.OriginFile.Contains("chrysler"))!;
        _house = _scene.Objects.Find(x => x.OriginFile.Contains("city"))!;
        _roads = _scene.Objects.FindAll(x => x.OriginFile.Contains("road"));
        _maxZ = _mainCar.Position.Z;
        _scene.Objects.Remove(_house);
        _scene.Objects.Remove(_npcCar);
        Mirror = new RearViewMirror();
        Mirror.Init(gl);
        Task.Run(GenerateCars);
        Task.Run(GenerateHouses);
    }


    public static GameManager GetInstance()
    {
        return _instance ??= new GameManager();
    }

    public void Update(double deltaTime)
    {
        _scene.SkyBox?.SetPosition(_mainCar.Position);
        lock (_allNpcCars)
        {
            foreach (var car in _allNpcCars)
            {
                if (car.Rotation.X != 0) continue;
                car.SetPosition(car.Position - car.Forward * (float)deltaTime * _npcSpeed);
            }
        }

        foreach (var road in _roads)
        {
            road.SetPosition(road.Position with {Z = MathF.Floor(_mainCar.Position.Z)});
            // if (_mainCar.Position.Z - road.Position.Z >= 20)
            // {
            //     road.SetPosition(road.Position + new Vector3D<float>(0, 0, 10 * MathF.Sign(_mainCar.Rotation.Y)));
            // }
        }
        
        _scene.Camera.Update(deltaTime);
        
        var moveY = _scene.Camera.MoveAxisY * _scene.Camera.MoveSpeed * (float)deltaTime;
        var movement = moveY * _mainCar.Forward;
        if (_scene.Camera.FirstPerson)
        {
            _scene.Camera.Position = _mainCar.Position with {Y = _scene.Camera.Position.Y} + 0.1f * _mainCar.Right;
        }
        else
        {
            _scene.Camera.Position = _mainCar.Position with {Y = _scene.Camera.Position.Y} + 1f * _mainCar.Forward;
        }

        var newX = _mainCar.Position.X - movement.X;
        if (newX is < -2f or > 0.5f)
        {
            movement.X = 0;
        }
        _mainCar.SetPosition(_mainCar.Position - movement);
        
        var rotateX = _scene.Camera.RotateAxisX * (float)deltaTime * _scene.Camera.RotateSpeed * Math.Abs(_scene.Camera.MoveAxisY);
        if (moveY < 0)
        {
            rotateX = -rotateX;
        }
        _mainCar.SetRotation(_mainCar.Rotation + new Vector3D<float>(0, -rotateX, 0));
        var rotateY = _scene.Camera.RotateAxisY * (float)deltaTime * _scene.Camera.RotateSpeed;
        _scene.Camera.Position += -_mainCar.Up * rotateY;

        if (-_mainCar.Position.Z > _maxZ)
        {
            Score += -_mainCar.Position.Z - _maxZ;
            if (_mainCar.Position.X < -0.5f)
            {
                Score += -_mainCar.Position.Z - _maxZ;
                _timeOpposite += (float)deltaTime;
            }
            _maxZ = -_mainCar.Position.Z;
        }

        _npcSpeed += _npcSpeed * 0.003f * (float)deltaTime;
        _spawnRate -= _spawnRate * 0.003f * (float)deltaTime;

        var mainCarColl = _mainCar.CollisionBox;
        lock (_allNpcCars)
        {
            foreach (var npc in _allNpcCars)
            {
                if (npc.Rotation.X != 0) continue;
                if (npc.CollisionBox.Overlaps(mainCarColl))
                {
                    _hp -= 1;
                    npc.SetRotation(npc.Rotation with { X = MathF.PI });
                }
            }
        }

        if (_hp <= 0)
        {
            _mainCar.SetRotation(_mainCar.Rotation with { X = MathF.PI });
        }
    }

    public void Draw(GL gl, Shader shader, double deltaTime)
    {
        _scene.Draw(gl, shader);
        
        ImGui.Begin("Camera",
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
        ImGui.Checkbox("First person camera", ref _scene.Camera.FirstPerson);
        var camFov = MainCamera.Fov * 180f / MathF.PI;
        if (ImGui.SliderFloat("Camera FOV", ref camFov, 60, 90))
        {
            MainCamera.Fov = camFov * MathF.PI / 180f;
        }
        ImGui.End();

        ImGui.Begin("Rearview mirror");
        ImGui.Image((nint)Mirror._mirrorMap, new Vector2(300, 150), new Vector2(1, 1), new Vector2(0, 0));
        ImGui.End();

        ImGui.Begin("Game statistics");
        ImGui.Text("Score: " + Score);
        ImGui.Text("Time spent on opposite lanes: " + _timeOpposite);
        ImGui.Text("NPC Speed: " + _npcSpeed);
        ImGui.Text("NPC SpawnRate: " + 1.0f / _spawnRate);
        ImGui.TextColored(new Vector4(0.75f, 0.45f, 0.54f, 1.0f), "HP: " + _hp);
        ImGui.End();
    }

    private async Task GenerateCars()
    {
        while (true)
        {
            var lane = Random.Shared.Next(100) / 25;
            var dir = lane < 2 ? -1 : 1;
            var pos = _npcCar.Position with
            {
                X = (2f - 0.75f) * lane / 1.75f - 1.85f, Z = _mainCar.Position.Z + dir * 20
            };
            var rot = new Vector3D<float>(0f, MathF.PI / 2f - dir * MathF.PI / 2, 0f);
            ModelObject? newCar = null;
            lock (_allNpcCars)
            {
                var tooFar = _allNpcCars.Find(x => (x.Position - _mainCar.Position).LengthSquared >= 20 * 20);
                if (tooFar is not null)
                {
                    newCar = tooFar;
                }
            }

            if (newCar is null)
            {
                newCar = ModelObject.CreateFrom(_npcCar);
                lock (_allNpcCars)
                {
                    _allNpcCars.Add(newCar);
                }
            }

            newCar.SetPosition(pos);
            newCar.SetRotation(rot);
            lock (_scene.Objects)
            {
                _scene.Objects.Add(newCar);
            }

            await Task.Delay((int)(_spawnRate * 1000));
        }
    }

    private async Task GenerateHouses()
    {
        for (var i = 0; i < 10; i++)
        {
            var lane = Random.Shared.Next(100) / 50;
            var dir = lane < 1 ? -1 : 1;
            var pos = _house.Position with { X = dir * 10, Z = -MathF.Sign(_mainCar.Forward.Z) * i * 15 };
            var newHouse = ModelObject.CreateFrom(_house);
            newHouse.SetPosition(pos);
            lock (_scene.Objects)
            {
                _scene.Objects.Add(newHouse);
            }
            _allHouses.Add(newHouse);
        }
        
        while (true)
        {
            ModelObject? redoHouse = null;
            lock (_allNpcCars)
            {
                var tooFar = _allHouses.Find(x => x.Position.Z - _mainCar.Position.Z >= 40);
                if (tooFar is not null)
                {
                    redoHouse = tooFar;
                }
            }

            redoHouse?.SetPosition(redoHouse.Position with { Z = _mainCar.Position.Z - MathF.Sign(_mainCar.Forward.Z) * (redoHouse.Position.Z -_mainCar.Position.Z) / 2f});
            await Task.Delay(500);
        }
    }

    public void Dispose(GL gl)
    {
        _scene.Dispose(gl);
        _npcCar.Dispose();
        _house.Dispose();
        Mirror.Dispose(gl);
    }

    public static CameraDescriptor MainCamera => GetInstance()._scene.Camera;
}