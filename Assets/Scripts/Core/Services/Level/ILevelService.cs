using Game.Configs;
using Game.Path;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс сервиса управления уровнями.
    /// </summary>
    public interface ILevelService
    {
        /// <summary>
        /// События уровня.
        /// </summary>
        System.Action<string> OnLevelSetupStarted { get; set; }
        System.Action<string, LevelMap> OnLevelSetupCompleted { get; set; }
        System.Action<string> OnLevelUnloaded { get; set; }
        
        /// Получить конфигурацию уровня.
        /// </summary>
        LevelConfig GetLevelConfig(string levelId);
        
        /// <summary>
        /// Получить визуальную конфигурацию уровня.
        /// </summary>
        LevelVisualConfig GetVisualConfig(string levelId);
        
        /// <summary>
        /// Загрузить уровень.
        /// </summary>
        GameObject LoadLevel(string levelId);
        
        /// <summary>
        /// Установить префаб уровня по ID.
        /// </summary>
        void SetupLevel(string levelId);
        
        /// <summary>
        /// Получить текущий экземпляр уровня.
        /// </summary>
        GameObject GetCurrentLevelInstance();
        
        /// <summary>
        /// Получить ID текущего уровня.
        /// </summary>
        string GetCurrentLevelId();
    }
}
