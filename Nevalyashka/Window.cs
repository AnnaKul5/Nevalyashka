using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using Nevalyashka.Object;
using Nevalyashka.Common;
using Nevalyashka.Render;
using LearnOpenTK;

namespace Nevalyashka
{
    public class Window : GameWindow
    {
        Vector3 LightPos = new Vector3(0.0f, 2.5f, -1.0f);
        Shader Shader;
        Texture DiffuseTail, SpecularTail;
        Texture DiffuseHead, SpecularHead;
        Texture DiffuseHand, SpecularHand;

        List<ObjectRender> ObjectRenderList = new List<ObjectRender>();

        double Time;
        int Side = 1;
        const double Degrees = 30;


        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        private void DefineShader(Shader Shader)
        {
            Shader.SetInt("material.diffuse", 0);
            Shader.SetInt("material.specular", 1);
            Shader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
            Shader.SetFloat("material.shininess", 100000.0f);
            Shader.SetVector3("light.position", LightPos);
            Shader.SetFloat("light.constant", 0.1f);
            Shader.SetFloat("light.linear", 0.09f);
            Shader.SetFloat("light.quadratic", 0.032f);
            Shader.SetVector3("light.ambient", new Vector3(0.2f));
            Shader.SetVector3("light.diffuse", new Vector3(0.5f));
            Shader.SetVector3("light.specular", new Vector3(1.0f));
            Shader.Use();
        }



        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.2f, 0.2f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            Sphere Head = new Sphere(0.21f, 0.0f, 0.55f, 0f);
            Sphere Body = new Sphere(0.4f, 0.0f, 0.0f, 0f);
            Sphere LeftHand = new Sphere(0.1f, 0.0f, 0.35f, -0.35f);
            Sphere RightHand = new Sphere(0.1f, 0.0f, 0.35f, 0.35f);

            Shader = new Shader("../../../Shaders/shader.vert", "../../../Shaders/lighting.frag");
            DefineShader(Shader);

            DiffuseHead = Texture.LoadFromFile("../../../Resources/body.jpg");
            SpecularHead = Texture.LoadFromFile("../../../Resources/body_specular.jpg");

            DiffuseTail = Texture.LoadFromFile("../../../Resources/head.jpg");
            SpecularTail = Texture.LoadFromFile("../../../Resources/head_specular.jpg");

            DiffuseHand = Texture.LoadFromFile("../../../Resources/hand.jpg");
            SpecularHand = Texture.LoadFromFile("../../../Resources/hand_specular.jpg");

            var HeadVert = Head.GetAll(); var HeadInd = Head.GetIndices();
            var BodyVert = Body.GetAll(); var BodyInd = Body.GetIndices();

            var LeftVert = LeftHand.GetAll(); var LeftInd = LeftHand.GetIndices();
            var RightVert = RightHand.GetAll(); var RightInd = RightHand.GetIndices();

            ObjectRenderList.Add(new ObjectRender(HeadVert, HeadInd, Shader, DiffuseTail, SpecularTail));
            ObjectRenderList.Add(new ObjectRender(BodyVert, BodyInd, Shader, DiffuseHead , SpecularHead));

            ObjectRenderList.Add(new ObjectRender(LeftVert, LeftInd, Shader, DiffuseHand, SpecularHand));
            ObjectRenderList.Add(new ObjectRender(RightVert, RightInd, Shader, DiffuseHand, SpecularHand));
        }

        protected override void OnRenderFrame(FrameEventArgs e)     //Класс аргументов события кадра. Характеристика - Time
        {
            base.OnRenderFrame(e);      
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Time += 30.0 * e.Time * Side;

            if (Math.Abs(Time) > Degrees) Side *= -1;

            var RotationMatrixZ = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(Time));
            var RotationMatrixY = Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(90));
            var TranslationMatrix = Matrix4.CreateTranslation(0, 0, (float)(Time / 100));

            var model = Matrix4.Identity * RotationMatrixZ * TranslationMatrix * RotationMatrixY;

            foreach (var Obj in ObjectRenderList)
            {
                Obj.Bind();
                Obj.ApplyTexture();
                Obj.UpdateShaderModel(model);
                Obj.ShaderAttribute();
                Obj.Render();
            }

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused) // Проверяем, в фокусе ли окно
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            
        }
    
    }
}

