using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс сервиса управления конфигурациями
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// Получить конфигурацию по типу
        /// </summary>
        T GetConfig<T>() where T : ScriptableObject;
        
        /// <summary>
        /// Получить конфигурацию по имени
        /// </summary>
        T GetConfig<T>(string name) where T : ScriptableObject;
        
        /// <summary>
        /// Загрузить все конфигурации из Resources
        /// </summary>
        void LoadAllConfigs();
        
        /// <summary>
        /// Проверить, есть ли конфигурация определенного типа
        /// </summary>
        bool HasConfig<T>() where T : ScriptableObject;
    }
}