using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Упрощенный интерфейс контроллера камеры
    /// </summary>
    public interface ICameraController
    {
        /// <summary>
        /// Установить изометрический вид с автоматическими вычислениями
        /// </summary>
        void SetIsometricView(Vector3 centerPoint, float height, float angle, bool useOrthographic = true, float orthographicSize = 10f);
        
        /// <summary>
        /// Установить позицию и направление камеры напрямую
        /// </summary>
        void SetTransform(Vector3 position, Vector3 lookAtTarget);
        
        /// <summary>
        /// Переключить между ортографической и перспективной проекцией
        /// </summary>
        void SetProjection(bool orthographic, float size = 10f, float fov = 70f);
        
        /// <summary>
        /// Получить камеру
        /// </summary>
        Camera GetCamera();
    }

    /// <summary>
    /// Простая структура для параметров камеры
    /// </summary>
    public readonly struct CameraParams
    {
        public readonly Vector3 center;
        public readonly float height;
        public readonly float angle;
        public readonly bool useOrthographic;
        public readonly float size;

        public CameraParams(Vector3 center, float height = 20f, float angle = 45f, bool useOrthographic = true, float size = 10f)
        {
            this.center = center;
            this.height = height;
            this.angle = angle;
            this.useOrthographic = useOrthographic;
            this.size = size;
        }
    }
}