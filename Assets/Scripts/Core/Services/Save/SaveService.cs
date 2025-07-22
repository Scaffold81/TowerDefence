using Game.Common;
using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Service for saving and loading player data.
    /// Uses PlayerPrefs for data storage with JSON serialization support.
    /// </summary>
    public class SaveService : ISaveService
    {
        private SaveSystem saveSystem;

        [Inject]
        private void Construct()
        {
            Debug.Log("[SaveService] Initializing save service...");
            
            saveSystem = new SaveSystem();
            
            Debug.Log("[SaveService] Save service initialized successfully!");
        }

        public void Save(string key, string value)
        {
            Debug.Log($"[SaveService] Saving data with key: {key}");
            saveSystem.SaveData(key, value);
        }

        public string Load(string key)
        {
            Debug.Log($"[SaveService] Loading data with key: {key}");
            var result = saveSystem.LoadData(key);
            if (result != null)
            {
                Debug.Log($"[SaveService] ✓ Data loaded successfully for key: {key}");
            }
            else
            {
                Debug.LogWarning($"[SaveService] ✗ No data found for key: {key}");
            }
            return result;
        }

        public T LoadJson<T>(string key)
        {
            Debug.Log($"[SaveService] Loading JSON data with key: {key} as type: {typeof(T).Name}");
            var result = saveSystem.LoadJSON<T>(key);
            if (result != null)
            {
                Debug.Log($"[SaveService] ✓ JSON data loaded successfully for key: {key}");
            }
            else
            {
                Debug.LogWarning($"[SaveService] ✗ No JSON data found for key: {key}");
            }
            return result;
        }
       
        public void SaveJson(string key, object data)
        {
            Debug.Log($"[SaveService] Saving JSON data with key: {key}, type: {data?.GetType().Name ?? "null"}");
            saveSystem.SaveJSON(key, data);
            Debug.Log($"[SaveService] ✓ JSON data saved successfully for key: {key}");
        }
    }
}