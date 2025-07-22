using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Interface for game configuration management service.
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// Get config by type.
        /// </summary>
        T GetConfig<T>() where T : ScriptableObject;
        
        /// <summary>
        /// Get config by name.
        /// </summary>
        T GetConfig<T>(string configName) where T : ScriptableObject;
        
        /// <summary>
        /// Check if config exists by type.
        /// </summary>
        bool HasConfig<T>() where T : ScriptableObject;
        
        /// <summary>
        /// Check if config exists by name.
        /// </summary>
        bool HasConfig(string configName);
        
        /// <summary>
        /// Load all configs from Resources/Configs.
        /// </summary>
        void LoadAllConfigs();
        
        /// <summary>
        /// Reload specific config.
        /// </summary>
        void ReloadConfig<T>() where T : ScriptableObject;
    }
}