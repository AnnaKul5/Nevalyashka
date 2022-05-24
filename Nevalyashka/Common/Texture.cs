using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Nevalyashka.Common
{
    // Вспомогательный класс, очень похожий на Shader, предназначенный для упрощения загрузки текстур.
    public class Texture
    {
        public readonly int Handle;

        public static Texture LoadFromFile(string path)
        {
            // Генерируем дескриптор
            int handle = GL.GenTexture();

            // Привязываем дескриптор
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            // Загружаем изображение

            using (var image = new Bitmap(path))
            {
                // Наш Bitmap загружается из верхнего левого пикселя, тогда как OpenGL загружается из нижнего левого пикселя, в результате чего текстура переворачивается по вертикали.
                // Это исправит ситуацию, заставив текстуру отображаться правильно.
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                 // Во - первых, мы получаем наши пиксели из загруженного растрового изображения.
                 // Аргументы:
                 // Требуемая площадь в пикселях. Как правило, вы хотите оставить его от (0,0) до (ширина, высота), но вы можете
                 // использовать другие прямоугольники для получения сегментов текстур, полезных для таких вещей, как таблицы спрайтов.
                 // Режим блокировки. В основном, как вы хотите использовать пиксели. Поскольку мы передаем их в OpenGL,
                 // нам нужен только ReadOnly.
                 // Далее следует формат пикселей, в котором мы хотим, чтобы наши пиксели были. В этом случае ARGB будет достаточно.
                 // Мы должны полностью определить имя, потому что OpenTK также имеет перечисление с именем PixelFormat.

                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // Теперь, когда наши пиксели подготовлены, пришло время сгенерировать текстуру. Мы делаем это с помощью GL.TexImage2D.
                // Аргументы:
                // Тип текстуры, которую мы генерируем. Существуют различные типы текстур, но сейчас нам нужна только Texture2D.
                //   Уровень проработанности деталей. Мы можем использовать это, чтобы начать с меньшего мип-мапа (если хотим), но нам это не нужно, поэтому оставьте значение 0.
                // Целевой формат пикселей. Это формат, в котором OpenGL будет хранить наше изображение.
                // Ширина изображения
                // Высота изображения.
                // Граница изображения. Это всегда должно быть 0; это устаревший параметр, от которого Khronos так и не избавился.
                // Формат пикселей, описанный выше. Поскольку ранее мы загрузили пиксели как ARGB, нам нужно использовать BGRA.
                // Тип данных пикселей.
                // И, наконец, сами пиксели.

                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);
            }

            // Теперь, когда наша текстура загружена, мы можем установить несколько параметров, влияющих на то, как изображение будет отображаться при рендеринге.

            // Во-первых, мы устанавливаем минимальные и магнитные фильтры. Они используются, когда текстура уменьшается и увеличивается соответственно.
            // Здесь мы используем Linear для обоих. Это означает, что OpenGL попытается смешать пиксели, а это означает, что текстуры, масштабированные слишком далеко, будут выглядеть размытыми.
            // Вы также можете использовать (среди других опций) Nearest, который просто захватывает ближайший пиксель, из-за чего текстура выглядит пиксельной, если масштаб слишком большой.
            // ПРИМЕЧАНИЕ. Настройки по умолчанию для обоих — LinearMipmap. Если вы оставите их по умолчанию, но не будете генерировать MIP-карты,
            // ваше изображение вообще не будет отображаться (вместо этого обычно получается чистый черный цвет).

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Теперь установим режим переноса. S — для оси X, а T — для оси Y.
            // Мы устанавливаем значение Repeat, чтобы текстуры повторялись при переносе. Здесь не показано, так как координаты текстуры точно совпадают

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Далее генерируем mipmaps.
            // Mipmaps — это уменьшенные копии текстуры в уменьшенном масштабе. Каждый уровень mipmaps в два раза меньше предыдущего.
            // Сгенерированные mipmaps уменьшаются до одного пикселя.
            // OpenGL будет автоматически переключаться между mipmaps, когда объект окажется достаточно далеко.
            // Это предотвращает эффекты муара, а также экономит пропускную способность текстуры.
            // Здесь вы можете увидеть и прочитать об эффекте морье https://en.wikipedia.org/wiki/Moir%C3%A9_pattern
            // Вот пример мипов в действии https://en.wikipedia.org/wiki/File:Mipmap_Aliasing_Comparison.png

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(handle);
        }

        public Texture(int glHandle)
        {
            Handle = glHandle;
        }

        // Активировать текстуру
        // Можно связать несколько текстур, если вашему шейдеру нужно больше одной.
        // Если вы хотите это сделать, используйте GL.ActiveTexture, чтобы указать, к какому слоту привязывается GL.BindTexture.
        // Стандарт OpenGL требует, чтобы их было не менее 16, но их может быть больше в зависимости от вашей видеокарты.
        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
