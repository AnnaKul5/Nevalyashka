using OpenTK.Mathematics;
using System;

namespace LearnOpenTK
{
    // ласс камера, с помощью которого можно перемещатьс€ в пространстве и смотреть по сторонам с помощью мыши
    public class Camera
    {
        // Ёти вектора отвечают за направлени€ движени€
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        // ¬ращение вокруг оси X (в радианах)
        private float _pitch;

        // ¬ращение вокруг оси Y (в радианах)
        private float _yaw = -MathHelper.PiOver2; // Ѕез этого камера будет смотреть на 90 градусов вправо (не пр€мо)

        //поле зрени€ камеры (в радианах)
        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        // ѕозици€ камеры
        public Vector3 Position { get; set; }

        // Ёто просто соотношение сторон видового экрана, используемое дл€ матрицы проекции.
        public float AspectRatio { private get; set; }

        public Vector3 Front => _front;

        public Vector3 Up => _up;

        public Vector3 Right => _right;

        // ћы преобразуем градусы в радианы
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // ћы фиксируем значение шага между -89 и 89, чтобы предотвратить переворачивание камеры вверх ногами,
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // ћы преобразуем градусы в радианы
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // ѕоле зрени€(FOV) - это вертикальный угол обзора камеры.
        // ћы преобразуем градусы в радианы
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        // ѕолучить матрицу просмотра с помощью функции LookAt
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        // ѕолучить матрицу проекциb
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        // Ёта функци€ будет обновл€ть вершины направлени€, использу€ некоторые математические методы
        private void UpdateVectors()
        {
            // ¬о-первых, передн€€ матрица вычисл€етс€ с использованием некоторой базовой тригонометрии.
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            //Ќам нужно убедитьс€, что все векторы нормализованы, так как в противном случае мы получили бы некоторые странные результаты.
            _front = Vector3.Normalize(_front);

            // ¬ычислите как правый, так и верхний вектор, использу€ перекрестное произведение.
            // ќбратите внимание, что мы вычисл€ем справа от глобального вверх; такое поведение может
            // не то, что вам нужно дл€ всех камер, так что имейте это в виду, если вам не нужна камера с частотой кадров в секунду.
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}