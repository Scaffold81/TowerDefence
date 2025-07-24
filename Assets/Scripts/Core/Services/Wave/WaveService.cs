using Game.Configs;
using Game.Path;
using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Сервис управления волнами врагов.
    /// </summary>
    public class WaveService : IWaveService
    {
        [Inject] private ILevelService levelService;
        [Inject] private LevelConfigRepository levelConfigRepository;
        
        private LevelMap currentLevelMap;
        private LevelConfig currentLevelConfig;
        private string currentLevelId;
        private int currentWaveNumber = 0;
        private bool isWaveActive = false;
        
        [Inject]
        public void Initialize()
        {
            Debug.Log("[WaveService] Initializing wave service...");
            
            // Подписываемся на события уровня
            levelService.OnLevelSetupCompleted += OnLevelLoaded;
            levelService.OnLevelUnloaded += OnLevelUnloaded;
            
            Debug.Log("[WaveService] Wave service initialized");
        }
        
        /// <summary>
        /// Обработчик загрузки уровня.
        /// </summary>
        private void OnLevelLoaded(string levelId, LevelMap levelMap)
        {
            Debug.Log($"[WaveService] Level loaded: {levelId}");
            
            currentLevelMap = levelMap;
            currentLevelId = levelId;
            currentLevelConfig = levelConfigRepository.GetLevelConfig(levelId);
            currentWaveNumber = 0;
            isWaveActive = false;
            
            if (currentLevelConfig == null)
            {
                Debug.LogError($"[WaveService] Level config not found for: {levelId}");
                return;
            }
            
            // Проверяем наличие SpawnPoint
            var spawnPoint = levelMap.SpawnPoint;
            if (spawnPoint != null)
            {
                Debug.Log($"[WaveService] Spawn point found at: {spawnPoint.transform.position}");
                Debug.Log($"[WaveService] Level has {currentLevelConfig.waveCount} waves");
                
                // Автоматически запускаем волны
                StartWaves();
            }
            else
            {
                Debug.LogError($"[WaveService] No SpawnPoint found on level: {levelId}");
            }
        }
        
        /// <summary>
        /// Обработчик выгрузки уровня.
        /// </summary>
        private void OnLevelUnloaded(string levelId)
        {
            Debug.Log($"[WaveService] Level unloaded: {levelId}");
            
            StopWaves();
            currentLevelMap = null;
            currentLevelConfig = null;
            currentLevelId = null;
            currentWaveNumber = 0;
            isWaveActive = false;
        }
        
        /// <summary>
        /// Запустить волны врагов.
        /// </summary>
        public void StartWaves()
        {
            if (currentLevelConfig == null || currentLevelMap == null)
            {
                Debug.LogWarning("[WaveService] Cannot start waves - level not loaded");
                return;
            }
            
            Debug.Log($"[WaveService] Starting waves for level: {currentLevelId}");
            SpawnNextWave();
        }
        
        /// <summary>
        /// Остановить волны врагов.
        /// </summary>
        public void StopWaves()
        {
            Debug.Log("[WaveService] Stopping waves");
            isWaveActive = false;
        }
        
        /// <summary>
        /// Запустить следующую волну.
        /// </summary>
        public void SpawnNextWave()
        {
            if (currentLevelConfig == null || currentLevelMap?.SpawnPoint == null)
            {
                Debug.LogWarning("[WaveService] Cannot spawn wave - level or spawn point not available");
                return;
            }
            
            if (currentWaveNumber >= currentLevelConfig.waveCount)
            {
                Debug.Log("[WaveService] All waves completed!");
                isWaveActive = false;
                return;
            }
            
            currentWaveNumber++;
            isWaveActive = true;
            
            Debug.Log($"[WaveService] Spawning wave {currentWaveNumber}/{currentLevelConfig.waveCount}");
            
            // Здесь будет логика спавна врагов
            // Пока что просто симулируем спавн
            var spawnPosition = currentLevelMap.SpawnPoint.transform.position;
            Debug.Log($"[WaveService] Wave {currentWaveNumber} spawned at position: {spawnPosition}");
            
            // TODO: Реализовать фактический спавн врагов
            // - Получить конфигурацию волны
            // - Создать врагов через EnemyFactory
            // - Настроить путь движения через waypoints
            
            // Симулируем завершение волны через некоторое время
            SimulateWaveCompletion();
        }
        
        /// <summary>
        /// Получить номер текущей волны.
        /// </summary>
        public int GetCurrentWaveNumber()
        {
            return currentWaveNumber;
        }
        
        /// <summary>
        /// Проверить активна ли волна.
        /// </summary>
        public bool IsWaveActive()
        {
            return isWaveActive;
        }
        
        /// <summary>
        /// Симуляция завершения волны (временно).
        /// </summary>
        private void SimulateWaveCompletion()
        {
            // В реальной игре это будет вызываться когда все враги волны уничтожены
            // Пока что симулируем через 3 секунды
            Debug.Log($"[WaveService] Wave {currentWaveNumber} will complete in 3 seconds (simulated)");
            
            // TODO: Заменить на реальную логику отслеживания врагов
        }
        
        /// <summary>
        /// Вызывается когда волна завершена (все враги уничтожены).
        /// </summary>
        public void OnWaveCompleted()
        {
            Debug.Log($"[WaveService] Wave {currentWaveNumber} completed");
            isWaveActive = false;
            
            // Небольшая пауза перед следующей волной
            if (currentWaveNumber < currentLevelConfig.waveCount)
            {
                Debug.Log("[WaveService] Preparing next wave...");
                // TODO: Добавить задержку между волнами
                SpawnNextWave();
            }
            else
            {
                Debug.Log("[WaveService] Level completed - all waves finished!");
            }
        }
    }
}
