using Game.Configs;
using Game.Path;
using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Сервис управления уровнями.
    /// </summary>
    public class LevelService : ILevelService
    {
        [Inject] private LevelConfigRepository levelConfigRepository;
        [Inject] private IGameFactory gameFactory;
        
        private GameObject currentLevelInstance;
        private string currentLevelId = "Lvl_01";
        
        // События
        public System.Action<string> OnLevelSetupStarted { get; set; }
        public System.Action<string, LevelMap> OnLevelSetupCompleted { get; set; }
        public System.Action<string> OnLevelUnloaded { get; set; }
        
        [Inject]
        public void Initialize()
        {
            Debug.Log("[LevelService] Initializing level service...");
            
            // Автоматически устанавливаем уровень по умолчанию
            SetupLevel();
        }
        
        /// <summary>
        /// Получить конфигурацию уровня.
        /// </summary>
        public LevelConfig GetLevelConfig(string levelId)
        {
            return levelConfigRepository.GetLevelConfig(levelId);
        }
        
        /// <summary>
        /// Получить визуальную конфигурацию уровня.
        /// </summary>
        public LevelVisualConfig GetVisualConfig(string levelId)
        {
            return levelConfigRepository.GetVisualConfig(levelId);
        }
        
        /// <summary>
        /// Загрузить уровень.
        /// </summary>
        public GameObject LoadLevel(string levelId)
        {
            var visualConfig = GetVisualConfig(levelId);
            if (visualConfig?.levelPrefab != null)
            {
                return gameFactory.Create(visualConfig.levelPrefab, Vector3.zero, null);
            }
            
            Debug.LogError($"Level prefab not found for levelId: {levelId}");
            return null;
        }
        
        /// <summary>
        /// Установить префаб уровня (по умолчанию Lvl_01).
        /// </summary>
        public void SetupLevel()
        {
            SetupLevel(currentLevelId);
        }
        
        /// <summary>
        /// Установить префаб уровня по ID.
        /// </summary>
        public void SetupLevel(string levelId)
        {
            OnLevelSetupStarted?.Invoke(levelId);
            
            // Удаляем предыдущий уровень если есть
            if (currentLevelInstance != null)
            {
                OnLevelUnloaded?.Invoke(currentLevelId);
                Object.Destroy(currentLevelInstance);
                currentLevelInstance = null;
            }
            
            // Загружаем новый уровень
            currentLevelInstance = LoadLevel(levelId);
            currentLevelId = levelId;
            
            if (currentLevelInstance != null)
            {
                // Находим LevelMap в корне префаба
                var levelMap = currentLevelInstance.GetComponent<LevelMap>();
                if (levelMap != null)
                {
                    OnLevelSetupCompleted?.Invoke(levelId, levelMap);
                    Debug.Log($"Level setup completed: {levelId}");
                }
                else
                {
                    Debug.LogError($"LevelMap component not found on level prefab: {levelId}");
                }
            }
            else
            {
                Debug.LogError($"Failed to setup level: {levelId}");
            }
        }
        
        /// <summary>
        /// Получить текущий экземпляр уровня.
        /// </summary>
        public GameObject GetCurrentLevelInstance()
        {
            return currentLevelInstance;
        }
        
        /// <summary>
        /// Получить ID текущего уровня.
        /// </summary>
        public string GetCurrentLevelId()
        {
            return currentLevelId;
        }
    }
}
