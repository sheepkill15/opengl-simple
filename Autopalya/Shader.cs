using Silk.NET.OpenGL;

namespace Autopalya;

public class Shader
{ 
    public uint Program { get; }

    public readonly Dictionary<string, int> UniformLocations = new();
    
    public Shader(GL gl, string vertex, string fragment)
    {
        Program = LinkProgram(gl, vertex, fragment);
    }

    public static Shader LoadShadersFromResource(GL gl, string vertexShaderResource, string fragmentShaderResource)
    {
        return new Shader(gl, ReadShader(vertexShaderResource), ReadShader(fragmentShaderResource));
    }

    public static uint LinkProgram(GL gl, string vertexShaderSource, string fragmentShaderSource)
    {
        uint vshader = gl.CreateShader(ShaderType.VertexShader);
        uint fshader = gl.CreateShader(ShaderType.FragmentShader);

        gl.ShaderSource(vshader, vertexShaderSource);
        gl.CompileShader(vshader);
        gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + gl.GetShaderInfoLog(vshader));

        gl.ShaderSource(fshader, fragmentShaderSource);
        gl.CompileShader(fshader);

        uint program = gl.CreateProgram();
        gl.AttachShader(program, vshader);
        gl.AttachShader(program, fshader);
        gl.LinkProgram(program);
        gl.GetProgram(program, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            Console.WriteLine($"Error linking shader {gl.GetProgramInfoLog(program)}");
        }
        gl.DetachShader(program, vshader);
        gl.DetachShader(program, fshader);
        gl.DeleteShader(vshader);
        gl.DeleteShader(fshader);
        return program;
    }

    public void Use(GL gl)
    {
        gl.UseProgram(Program);
    }

    public int GetUniformLocation(GL gl, string uniformName)
    {
        if (UniformLocations.TryGetValue(uniformName, out var location))
        {
            return location;
        }

        int loc = gl.GetUniformLocation(Program, uniformName);
        UniformLocations.Add(uniformName, loc);
        return loc;
    }
    
    private static string ReadShader(string shaderFileName)
    {
        using var shaderStream =
            typeof(Program).Assembly.GetManifestResourceStream("Autopalya.Shaders." + shaderFileName);
        using var shaderReader = new StreamReader(shaderStream!);
        return shaderReader.ReadToEnd();
    }

}