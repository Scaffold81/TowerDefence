using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Game configuration management service.
    /// Automatically loads all ScriptableObject configs from Resources/Configs.
    /// </summary>
    public class ConfigService : IConfigService
    {
        private readonly Dictionary<string, ScriptableObject> configs = new Dictionary<string, ScriptableObject>();

        [Inject]
        private void Construct()
        {
            Debug.Log("[ConfigService] Initializing configuration service...");
            
            LoadAllConfigs();
            
            Debug.Log("[ConfigService] Configuration service initialized successfully!");
        }

        public void LoadAllConfigs()
        {
            Debug.Log("[ConfigService] Loading all configs from Resources/Configs...");
            
            // Load all configs from Resources/Configs
            LoadAllConfigsFromResources();
            
            Debug.Log($"[ConfigService] Successfully loaded {configs.Count} configuration files");
        }

        public T GetConfig<T>() where T : ScriptableObject
        {
            string configName = typeof(T).Name;
            return GetConfig<T>(configName);
        }

        public T GetConfig<T>(string configName) where T : ScriptableObject
        {
            if (configs.TryGetValue(configName, out var config))
            {
                Debug.Log($"[ConfigService] Retrieved config: {configName}");
                return config as T;
            }

            Debug.LogWarning($"[ConfigService] Config '{configName}' not found! Available configs: {string.Join(", ", configs.Keys)}");
            return null;
        }

        public bool HasConfig<T>() where T : ScriptableObject
        {
            string configName = typeof(T).Name;
            return HasConfig(configName);
        }

        public bool HasConfig(string configName)
        {
            return configs.ContainsKey(configName);
        }

        public void ReloadConfig<T>() where T : ScriptableObject
        {
            string configName = typeof(T).Name;
            Debug.Log($"[ConfigService] Reloading config: {configName}");
            
            LoadConfig<T>(configName);
        }

        private void LoadAllConfigsFromResources()
        {
            // Automatically find all ScriptableObjects in Resources/Configs
            var configAssets = Resources.LoadAll<ScriptableObject>("Configs");
            
            if (configAssets.Length == 0)
            {
                Debug.LogWarning("[ConfigService] No config files found in Resources/Configs folder!");
                return;
            }
            
            foreach (var config in configAssets)
            {
                string configName = config.GetType().Name;
                configs[configName] = config;
                Debug.Log($"[ConfigService] ✓ Auto-loaded config: {configName} ({config.GetType()})");
            }
        }

        private void LoadConfig<T>(string configName) where T : ScriptableObject
        {
            var config = Resources.Load<T>($"Configs/{configName}");
            if (config != null)
            {
                configs[configName] = config;
                Debug.Log($"[ConfigService] ✓ Manually loaded config: {configName}");
            }
            else
            {
                Debug.LogError($"[ConfigService] ✗ Failed to load config: {configName} from Resources/Configs/");
            }
        }
    }
}