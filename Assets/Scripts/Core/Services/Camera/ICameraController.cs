using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс контроллера управления камерой
    /// </summary>
    public interface ICameraController
    {
        /// <summary>
        /// Позиционировать камеру в указанную точку с заданными параметрами
        /// </summary>
        /// <param name="position">Позиция камеры</param>
        /// <param name="lookAtTarget">Точка, на которую должна смотреть камера</param>
        void SetCameraTransform(Vector3 position, Vector3 lookAtTarget);
        
        /// <summary>
        /// Установить ортографическую проекцию с заданным размером
        /// </summary>
        /// <param name="orthographicSize">Размер ортографической проекции</param>
        void SetOrthographicProjection(float orthographicSize);
        
        /// <summary>
        /// Установить перспективную проекцию с заданным FOV
        /// </summary>
        /// <param name="fieldOfView">Поле зрения</param>
        void SetPerspectiveProjection(float fieldOfView);
        /// <summary>
        /// Применить изометрическую настройку камеры
        /// </summary>
        /// <param name="centerPoint">Центральная точка для обзора</param>
        /// <param name="height">Высота камеры</param>
        /// <param name="angle">Угол наклона</param>
        /// <param name="orthographicSize">Размер ортографической проекции (если используется)</param>
        void ApplyIsometricView(Vector3 centerPoint, float height, float angle, float orthographicSize = 10f);
        Camera GetCamera();
    }
}