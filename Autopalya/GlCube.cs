using Silk.NET.OpenGL;

namespace Autopalya;

internal static class GlCube
{

    public static unsafe GlObject CreateInteriorCube(GL gl, Material mat)
    {
        uint vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        // counter clockwise is front facing
        float[] vertexArray = {
            // top face
            -0.5f, 0.5f, 0.5f, 0f, -1f, 0f, 1f/4f, 0f/3f,
            0.5f, 0.5f, 0.5f, 0f, -1f, 0f, 2f/4f, 0f/3f,
            0.5f, 0.5f, -0.5f, 0f, -1f, 0f, 2f/4f, 1f/3f,
            -0.5f, 0.5f, -0.5f, 0f, -1f, 0f, 1f/4f, 1f/3f,

            // front face
            -0.5f, 0.5f, 0.5f, 0f, 0f, -1f, 1, 1f/3f,
            -0.5f, -0.5f, 0.5f, 0f, 0f, -1f, 4f/4f, 2f/3f,
            0.5f, -0.5f, 0.5f, 0f, 0f, -1f, 3f/4f, 2f/3f,
            0.5f, 0.5f, 0.5f, 0f, 0f, -1f,  3f/4f, 1f/3f,

            // left face
            -0.5f, 0.5f, 0.5f, 1f, 0f, 0f, 0, 1f/3f,
            -0.5f, 0.5f, -0.5f, 1f, 0f, 0f,1f/4f, 1f/3f,
            -0.5f, -0.5f, -0.5f, 1f, 0f, 0f, 1f/4f, 2f/3f,
            -0.5f, -0.5f, 0.5f, 1f, 0f, 0f, 0f/4f, 2f/3f,

            // bottom face
            -0.5f, -0.5f, 0.5f, 0f, 1f, 0f, 1f/4f, 1f,
            0.5f, -0.5f, 0.5f,0f, 1f, 0f, 2f/4f, 1f,
            0.5f, -0.5f, -0.5f,0f, 1f, 0f, 2f/4f, 2f/3f,
            -0.5f, -0.5f, -0.5f,0f, 1f, 0f, 1f/4f, 2f/3f,

            // back face
            0.5f, 0.5f, -0.5f, 0f, 0f, 1f, 2f/4f, 1f/3f,
            -0.5f, 0.5f, -0.5f, 0f, 0f, 1f, 1f/4f, 1f/3f,
            -0.5f, -0.5f, -0.5f,0f, 0f, 1f, 1f/4f, 2f/3f,
            0.5f, -0.5f, -0.5f,0f, 0f, 1f, 2f/4f, 2f/3f,

            // right face
            0.5f, 0.5f, 0.5f, -1f, 0f, 0f, 3f/4f, 1f/3f,
            0.5f, 0.5f, -0.5f,-1f, 0f, 0f, 2f/4f, 1f/3f,
            0.5f, -0.5f, -0.5f, -1f, 0f, 0f, 2f/4f, 2f/3f,
            0.5f, -0.5f, 0.5f, -1f, 0f, 0f, 3f/4f, 2f/3f,
        };

        uint[] indexArray = {
            0, 2, 1,
            0, 3, 2,

            4, 6, 5,
            4, 7, 6,

            8, 10, 9,
            10, 8, 11,

            12, 13, 14,
            12, 14, 15,

            17, 19, 16,
            17, 18, 19,

            20, 21, 22,
            20, 22, 23
        };

        const uint offsetPos = 0;
        const uint offsetNormal = offsetPos + (3 * sizeof(float));
        const uint offsetTexture = offsetNormal + (3 * sizeof(float));
        const uint vertexSize = offsetTexture + (2 * sizeof(float));

        uint vertices = gl.GenBuffer();
        gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
        gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
        gl.EnableVertexAttribArray(0);

        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

        uint colors = gl.GenBuffer();
        //Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
        //Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
        //Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
        //Gl.EnableVertexAttribArray(1);
        
        gl.EnableVertexAttribArray(3);
        gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTexture);


        uint indices = gl.GenBuffer();
        gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
        gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

        // release array buffer
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        uint indexArrayLength = (uint)indexArray.Length;

        return new GlObject(vao, vertices, colors, indices, indexArrayLength, mat, gl, -0.5f, 0.5f, -0.5f, 0.5f, -0.5f, 0.5f);
    }
}