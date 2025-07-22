using UnityEngine;

namespace Game.Services
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Configs/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game Settings")]
        public string gameVersion = "1.0.0";
        public int targetFrameRate = 60;
        public bool enableCheats = false;
        
        [Header("Scene Settings")]
        public float sceneTransitionDuration = 1f;
        public string startSceneName = "MainMenu";
        
        [Header("Save Settings")]
        public bool autoSave = true;
        public float autoSaveInterval = 30f;
        public int maxSaveSlots = 3;
        
        [Header("Debug Settings")]
        public bool enableDebugConsole = false;
        public bool showFPS = false;
        public bool enableProfiling = false;
        
        [Header("Performance Settings")]
        public int poolPrewarmCount = 50;
        public float objectCullingDistance = 100f;
        public int maxActiveEnemies = 200;
        
        [Header("Localization")]
        public SystemLanguage defaultLanguage = SystemLanguage.English;
        
        public void ValidateSettings()
        {
            targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 120);
            autoSaveInterval = Mathf.Clamp(autoSaveInterval, 10f, 300f);
            maxSaveSlots = Mathf.Clamp(maxSaveSlots, 1, 10);
            poolPrewarmCount = Mathf.Clamp(poolPrewarmCount, 10, 1000);
        }
        
        private void OnValidate()
        {
            ValidateSettings();
        }
    }
}