using System.Collections.Generic;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Сервис управления конфигурациями игры
    /// </summary>
    public class ConfigService : IConfigService
    {
        private readonly Dictionary<System.Type, ScriptableObject> _configs = new();
        private bool _isInitialized = false;
        
        public ConfigService()
        {
            LoadAllConfigs();
        }
        
        /// <summary>
        /// Получить конфигурацию по типу
        /// </summary>
        public T GetConfig<T>() where T : ScriptableObject
        {
            if (!_isInitialized)
            {
                LoadAllConfigs();
            }
            
            var type = typeof(T);
            
            if (_configs.TryGetValue(type, out var config))
            {
                return config as T;
            }
            
            Debug.LogWarning($"[ConfigService] Config of type {type.Name} not found. Available configs: {string.Join(", ", _configs.Keys)}");
            return null;
        }
        
        /// <summary>
        /// Получить конфигурацию по имени
        /// </summary>
        public T GetConfig<T>(string name) where T : ScriptableObject
        {
            var config = Resources.Load<T>($"Configs/{name}");
            
            if (config == null)
            {
                Debug.LogWarning($"[ConfigService] Config '{name}' of type {typeof(T).Name} not found in Resources/Configs/");
            }
            
            return config;
        }
        
        /// <summary>
        /// Загрузить все конфигурации из Resources
        /// </summary>
        public void LoadAllConfigs()
        {
            Debug.Log("[ConfigService] Loading all configurations...");
            
            _configs.Clear();
            
            // Загружаем все ScriptableObject из папки Resources/Configs
            var allConfigs = Resources.LoadAll<ScriptableObject>("Configs");
            
            Debug.Log($"[ConfigService] Found {allConfigs.Length} total configs in Resources/Configs/");
            
            foreach (var config in allConfigs)
            {
                var type = config.GetType();
                Debug.Log($"[ConfigService] Found config: {type.Name} - {config.name} (Type: {type.FullName})");
                
                if (!_configs.ContainsKey(type))
                {
                    _configs[type] = config;
                    Debug.Log($"[ConfigService] ✓ Loaded config: {type.Name} ({config.name})");
                }
                else
                {
                    Debug.LogWarning($"[ConfigService] Duplicate config type {type.Name} found: {config.name}");
                }
            }
            
            _isInitialized = true;
            Debug.Log($"[ConfigService] Successfully loaded {_configs.Count} configurations");
        }
        
        /// <summary>
        /// Проверить, есть ли конфигурация определенного типа
        /// </summary>
        public bool HasConfig<T>() where T : ScriptableObject
        {
            if (!_isInitialized)
            {
                LoadAllConfigs();
            }
            
            return _configs.ContainsKey(typeof(T));
        }
    }
}