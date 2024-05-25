using System.Drawing;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Autopalya;

internal static class Program
{
    private static IWindow _window = null!;

    private static IInputContext _inputContext = null!;

    private static GL _gl = null!;

    private static ImGuiController _controller = null!;


    private static Scene _scene = null!;
    private static Shader _mainShader = null!;


    private static void Main()
    {
        var windowOptions = WindowOptions.Default;
        windowOptions.Title = "Autopalya";
        windowOptions.Size = new Vector2D<int>(1280, 720);

        // on some systems there is no depth buffer by default, so we need to make sure one is created
        windowOptions.PreferredDepthBufferBits = 24;

        _window = Window.Create(windowOptions);

        _window.Load += Window_Load;
        _window.Update += Window_Update;
        _window.Render += Window_Render;
        _window.Closing += Window_Closing;

        _window.Run();
    }

    private static void Window_Load()
    {
        //Console.WriteLine("Load");

        // set up input handling
        _inputContext = _window.CreateInput();
        foreach (var keyboard in _inputContext.Keyboards)
        {
            keyboard.KeyDown += Keyboard_KeyDown;
            keyboard.KeyUp += KeyboardOnKeyUp;
        }

        _gl = _window.CreateOpenGL();

        _controller = new ImGuiController(_gl, _window, _inputContext);

        // Handle resizes
        _window.FramebufferResize += s =>
        {
            // Adjust the viewport to the new window size
            _gl.Viewport(s);
        };


        _gl.ClearColor(Color.Black);

        _scene = SceneReader.ReadSceneFromResource(_gl, "scenes.main.sc");

        //Gl.Enable(EnableCap.CullFace);

        _gl.Enable(EnableCap.DepthTest);
        _gl.DepthFunc(DepthFunction.Lequal);

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _mainShader = Shader.LoadShadersFromResource(_gl, "VertexShader.vert", "PBRFragment.frag");
        
        _scene.Init(_gl, _mainShader);

        _gl.UseProgram(0);
    }


    private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        switch (key)
        {
            case Key.Left:
                _scene.Camera.MoveLeft = true;
                break;
            case Key.Right:
                _scene.Camera.MoveRight = true;
                break;
            case Key.Down:
                _scene.Camera.MoveBackward = true;
                break;
            case Key.Up:
                _scene.Camera.MoveForward = true;
                break;
            case Key.W:
                _scene.Camera.RotateUp = true;
                break;
            case Key.S:
                _scene.Camera.RotateDown = true;
                break;
            case Key.A:
                _scene.Camera.RotateLeft = true;
                break;
            case Key.D:
                _scene.Camera.RotateRight = true;
                break;
        }
    }

    private static void KeyboardOnKeyUp(IKeyboard keyboard, Key key, int arg3)
    {
        switch (key)
        {
            case Key.Left:
                _scene.Camera.MoveLeft = false;
                break;
            case Key.Right:
                _scene.Camera.MoveRight = false;
                break;
            case Key.Down:
                _scene.Camera.MoveBackward = false;
                break;
            case Key.Up:
                _scene.Camera.MoveForward = false;
                break;
            case Key.W:
                _scene.Camera.RotateUp = false;
                break;
            case Key.S:
                _scene.Camera.RotateDown = false;
                break;
            case Key.A:
                _scene.Camera.RotateLeft = false;
                break;
            case Key.D:
                _scene.Camera.RotateRight = false;
                break;
            case Key.Space:
                break;
        }
    }


    private static void Window_Update(double deltaTime)
    {
        //Console.WriteLine($"Update after {deltaTime} [s].");
        // multithreaded
        // make sure it is threadsafe
        // NO GL calls

        _controller.Update((float)deltaTime);

        float moveX = _scene.Camera.MoveAxisX * (float)deltaTime * _scene.Camera.MoveSpeed;
        float moveY = _scene.Camera.MoveAxisY * (float)deltaTime * _scene.Camera.MoveSpeed;
        Vector3D<float> movement = moveX * _scene.Camera.Right + moveY * _scene.Camera.Forward;
        _scene.Camera.Move(movement);


        float rotateX = _scene.Camera.RotateAxisX * (float)deltaTime * _scene.Camera.RotateSpeed;
        float rotateY = _scene.Camera.RotateAxisY * (float)deltaTime * _scene.Camera.RotateSpeed;
        _scene.Camera.Rotate(rotateY, rotateX);
    }

    private static void Window_Render(double deltaTime)
    {
        //Console.WriteLine($"Render after {deltaTime} [s].");
        
        _scene.Draw(_gl, _mainShader);

        // DrawRevolvingCube();

        // DrawSkyBox();

        //ImGuiNET.ImGui.ShowDemoWindow();
        ImGui.Begin("Lighting properties",
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
        ImGui.End();


        _controller.Render();
    }
    


    private static void Window_Closing()
    {
        _scene.Dispose(_gl);
    }

    public static void CheckError()
    {
        var error = (ErrorCode)_gl.GetError();
        if (error != ErrorCode.NoError)
            throw new Exception("GL.GetError() returned " + error);
    }
}