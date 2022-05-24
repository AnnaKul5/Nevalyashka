using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace Nevalyashka.Common
{
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertPath, string fragPath)
        {
            // Существует несколько различных типов шейдеров, но единственные два, которые вам нужны для базового рендеринга, -это шейдеры вершин и фрагментов.
            // Вершинный шейдер отвечает за перемещение по вершинам и загрузку этих данных в фрагментный шейдер.
            // Вершинный шейдер здесь не будет слишком важен, но позже он станет более важным.
            // Шейдер фрагментов отвечает за последующее преобразование вершин в "фрагменты", которые представляют все данные, необходимые OpenGL для рисования пикселя.
            // Фрагментный шейдер - это то, что мы будем использовать здесь чаще всего.

            // Загружаем шейдер вертексов и компилируем
            var shaderSource = File.ReadAllText(vertPath);

            // GL.CreateShader создаст пустой шейдер (очевидно). Перечисление ShaderType указывает, какой тип шейдера будет создан.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            // Теперь привяжите исходный код GLSL
            GL.ShaderSource(vertexShader, shaderSource);

            // И компилим
            CompileShader(vertexShader);

            // Делаем тоже самое для фрагментого шейдера
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            // Затем эти два шейдера должны быть объединены в шейдерную программу, которая затем может быть использована OpenGL.
            // Для этого создайте программу...
            Handle = GL.CreateProgram();

            // Прикрепите оба шейдера...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // А затем свяжите их вместе.
            LinkProgram(Handle);

            // Когда шейдерная программа связана, ей больше не нужны отдельные шейдеры, прикрепленные к ней; скомпилированный код копируется в шейдерную программу.
            // Отсоедините их, а затем удалите.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // Теперь шейдер готов к работе, но сначала мы собираемся кэшировать все однородные местоположения шейдера.
            // Запрос этого из шейдера выполняется очень медленно, поэтому мы делаем это один раз при инициализации и повторно используем эти значения позже.

            // Во-первых, мы должны получить количество активных униформ в шейдере.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Далее выделите словарь для хранения местоположений
            _uniformLocations = new Dictionary<string, int>();

            // Цикл по всей униформе
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // получить название этой униформы
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // получить местоположение
                var location = GL.GetUniformLocation(Handle, key);

                // а затем добавьте его в словарь
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Пробуем скомпилить шейдер
            GL.CompileShader(shader);

            // Проверяем на ошибки компилирования
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // Соединяем с программой
            GL.LinkProgram(program);

            // Проверка на ошибки с линквокй
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // Функция-оболочка, которая включает программу шейдеров.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // Источники шейдеров, поставляемые с этим проектом, используют жестко закодированные layout(location)-ы. Если вы хотите сделать это динамически,
        // вы можете опустить строки layout(location=X) в вершинном шейдере и использовать это в vertexAttribPointer вместо жестко закодированных значений.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // // Единые пояснения
        // // Униформа - это переменные, которые могут быть заданы пользовательским кодом, вместо того, чтобы считывать их из VBA.
        // // Вы используете Vbo для данных, связанных с вершинами, и uniforms почти для всего остального.

        // Настройка униформы
        // 1. Привяжите программу, на которую вы хотите установить униформу
        // 2. Получите справку о местонахождении униформы с помощью GL.GetUniformLocation.
        // 3. Используйте соответствующую функцию GL.Uniform* для установки униформы.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }
    }
}
