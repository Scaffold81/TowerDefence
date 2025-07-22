using UnityEngine;
using Newtonsoft.Json;
using System;

namespace Game.Common
{
    public class SaveSystem
    {
        public  bool HasKeyData(string saveSlot)
        {
            return PlayerPrefs.HasKey(saveSlot);
        }

        public  void SaveData(string saveSlot, string data)
        {
            PlayerPrefs.SetString(saveSlot, data);
        }

        public string LoadDataOrDefault(string key, string defaultValue)
        {
            string loadedData = LoadData(key);
            return string.IsNullOrEmpty(loadedData) ? defaultValue : loadedData;
        }

        public string LoadData(string saveSlot)
        {
            if (PlayerPrefs.HasKey(saveSlot))
            {
                return PlayerPrefs.GetString(saveSlot);
            }
            else
            {
               // Debug.LogError(saveSlot + " " + "PlayerPrefs.HasKey value is not valid");
                return null;
            }
        }

        public void DeleteData(string saveSlot)
        {
            PlayerPrefs.DeleteKey(saveSlot);
        }

        public  string SaveJSON(string saveSlot, object data)
        {
            string jsonData = JsonConvert.SerializeObject(data);
           
            PlayerPrefs.SetString(saveSlot, jsonData);
            PlayerPrefs.Save();

            return jsonData;
        }

        public  T LoadJSON<T>(string saveSlot)
        {
            string jsonData = PlayerPrefs.GetString(saveSlot);
            
            if (!string.IsNullOrEmpty(jsonData))
            {
                T data = JsonConvert.DeserializeObject<T>(jsonData);
                return data;
            }
            else
            {
               // Debug.LogError(saveSlot + " data not found in PlayerPrefs");
                return default(T);
            }
        }
    }
}
