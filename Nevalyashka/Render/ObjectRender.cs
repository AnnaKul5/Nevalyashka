using Nevalyashka.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nevalyashka.Render
{
    internal class ObjectRender
    {
        private int VertexArrayObject = GL.GenVertexArray();
        private int ElementBufferObject = GL.GenBuffer();
        private int VertexBufferObject = GL.GenBuffer();

        private int IndicesLenght;
        Shader Shader;
        Texture Diffuse, Specular;   //диффузная, спекулярная


        public ObjectRender(float[] Vertices, uint[] Indices, Shader shader, Texture dff, Texture spcl)
        {
            IndicesLenght = Indices.Length;
            this.Shader = shader;
            this.Diffuse = dff;
            this.Specular = spcl;
            this.Bind();
            this.ShaderAttribute();

            //Создаётся связь c буффером
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

            //инициализируем буфферное
            GL.NamedBufferStorage(
               VertexBufferObject,
               Vertices.Length * sizeof(float),        // размер, необходимый для этого буффера
               Vertices,                           // данные, которые нужно положить в буферр
               BufferStorageFlags.MapWriteBit);    // на этом этапе мы будем записывать только в буфер

            //настройки vao
            GL.EnableVertexArrayAttrib(VertexArrayObject, 0);

            GL.VertexArrayVertexBuffer(VertexArrayObject, 0, VertexBufferObject, IntPtr.Zero, 8 * sizeof(float));

            //Связываем буфер, и посылаем туда данные индексов (для дальнейшего их использования, чтобы соединять точки)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.DynamicDraw);
        }


        //Связываение
        public void Bind()
        {
            GL.BindVertexArray(VertexArrayObject);
        }

        public void Render()
        {
            GL.DrawElements(PrimitiveType.Triangles, IndicesLenght, DrawElementsType.UnsignedInt, 0);
            
        }

        public void ShaderAttribute()
        {
            this.Bind();

            //собираем координты, посылаем туда, задаём инструкции как их читать
            var positionLocation = Shader.GetAttribLocation("aPos​");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            //собираем координты, посылаем туда, задаём инструкции как их читать
            var normalLocation = Shader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            //собираем координты, посылаем туда, задаём инструкции как их читать
            var texCoordLocation = Shader.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

        }

        public void ApplyTexture()
        {
            Diffuse.Use(TextureUnit.Texture0);
            Specular.Use(TextureUnit.Texture1);
            
        }
         
        public void UpdateShaderModel(Matrix4 model)
        {
            Shader.SetMatrix4("model", model);
        }


    }
}
