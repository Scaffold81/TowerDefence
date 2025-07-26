using Game.Path;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Упрощенный интерфейс сервиса управления камерой
    /// </summary>
    public interface ICameraService
    {
        /// <summary>
        /// Позиционировать камеру для уровня
        /// </summary>
        void PositionCameraForLevel(LevelMap levelMap);
        
        /// <summary>
        /// Позиционировать камеру в конкретную точку
        /// </summary>
        void PositionCamera(Vector3 center, float height = 20f, float angle = 45f);
        
        /// <summary>
        /// Получить текущую камеру
        /// </summary>
        Camera GetCurrentCamera();
    }
}