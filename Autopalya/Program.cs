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

    private static Shader _mainShader = null!;

    private static ModelObject _mainCar = null!;


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


        //Gl.Enable(EnableCap.CullFace);

        _gl.Enable(EnableCap.DepthTest);
        _gl.DepthFunc(DepthFunction.Lequal);

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _mainShader = Shader.LoadShadersFromResource(_gl, "VertexShader.vert", "PBRFragment.frag");
        _mainShader.Use(_gl);
        
        GameManager.GetInstance().Init(_gl);
        
        _gl.UseProgram(0);
    }


    private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        switch (key)
        {
            case Key.Left:
                GameManager.MainCamera.MoveLeft = true;
                GameManager.MainCamera.RotateLeft = true;
                break;
            case Key.Right:
                GameManager.MainCamera.MoveRight = true;
                GameManager.MainCamera.RotateRight = true;
                break;
            case Key.Down:
                GameManager.MainCamera.MoveBackward = true;
                break;
            case Key.Up:
                GameManager.MainCamera.MoveForward = true;
                break;
            case Key.W:
                GameManager.MainCamera.RotateUp = true;
                break;
            case Key.S:
                GameManager.MainCamera.RotateDown = true;
                break;
            case Key.A:
                GameManager.MainCamera.RotateLeft = true;
                break;
            case Key.D:
                GameManager.MainCamera.RotateRight = true;
                break;
        }
    }

    private static void KeyboardOnKeyUp(IKeyboard keyboard, Key key, int arg3)
    {
        switch (key)
        {
            case Key.Left:
                GameManager.MainCamera.MoveLeft = false;
                GameManager.MainCamera.RotateLeft = false;
                break;
            case Key.Right:
                GameManager.MainCamera.MoveRight = false;
                GameManager.MainCamera.RotateRight = false;

                break;
            case Key.Down:
                GameManager.MainCamera.MoveBackward = false;
                break;
            case Key.Up:
                GameManager.MainCamera.MoveForward = false;
                break;
            case Key.W:
                GameManager.MainCamera.RotateUp = false;
                break;
            case Key.S:
                GameManager.MainCamera.RotateDown = false;
                break;
            case Key.A:
                GameManager.MainCamera.RotateLeft = false;
                break;
            case Key.D:
                GameManager.MainCamera.RotateRight = false;
                break;
            case Key.Space:
                GameManager.MainCamera.FirstPerson = !GameManager.MainCamera.FirstPerson;
                if (GameManager.MainCamera.FirstPerson)
                {
                    GameManager.MainCamera.Position = GameManager.MainCamera.Position with { Y = -0.1f };
                }
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
        GameManager.GetInstance().Update(deltaTime);
        // _scene.Camera.Rotate(rotateY, rotateX);
    }

    private static void Window_Render(double deltaTime)
    {
        //Console.WriteLine($"Render after {deltaTime} [s].");
        
        GameManager.GetInstance().Draw(_gl, _mainShader, deltaTime);

        // DrawRevolvingCube();

        // DrawSkyBox();

        //ImGuiNET.ImGui.ShowDemoWindow();


        _controller.Render();
    }
    


    private static void Window_Closing()
    {
        GameManager.GetInstance().Dispose(_gl);
    }

    public static void CheckError()
    {
        var error = (ErrorCode)_gl.GetError();
        if (error != ErrorCode.NoError)
            throw new Exception("GL.GetError() returned " + error);
    }
}