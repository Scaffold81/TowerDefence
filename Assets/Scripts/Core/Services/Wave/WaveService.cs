using Game.Configs;
using Game.Path;
using Game.Wave;
using Game.Enemy.Services;
using UnityEngine;
using Zenject;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace Game.Services
{
    /// <summary>
    /// Сервис управления волнами врагов.
    /// </summary>
    public class WaveService : IWaveService
    {
        [Inject] private ILevelService levelService;
        [Inject] private IEnemyService enemyService;
        [Inject] private LevelConfigRepository levelConfigRepository;
        
        private LevelMap currentLevelMap;
        private LevelConfig currentLevelConfig;
        private string currentLevelId;
        private int currentWaveIndex = -1; // Индекс текущей волны (0-based)
        private bool isWaveActive = false;
        private CancellationTokenSource waveTokenSource;
        
        [Inject]
        public void Initialize()
        {
            Debug.Log("[WaveService] Initializing wave service...");
            
            // Подписываемся на события уровня
            levelService.OnLevelSetupCompleted += OnLevelLoaded;
            levelService.OnLevelUnloaded += OnLevelUnloaded;
            
            // Подписываемся на события врагов
            if (enemyService != null)
            {
                enemyService.OnWaveCompleted += OnEnemyWaveCompleted;
                Debug.Log("[WaveService] Subscribed to enemy service events");
            }
            
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
            currentWaveIndex = -1;
            isWaveActive = false;
            
            // Устанавливаем карту в EnemyService
            enemyService?.SetLevelMap(levelMap);
            
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
                Debug.Log($"[WaveService] Level has {currentLevelConfig.WaveCount} waves");
                
                // Применяем автомасштабирование сложности если включено
                if (currentLevelConfig.useAutoScaling)
                {
                    currentLevelConfig.ApplyDifficultyScaling();
                    Debug.Log("[WaveService] Applied difficulty scaling to waves");
                }
                
                // Автоматически запускаем волны если включено
                if (currentLevelConfig.autoStartWaves)
                {
                    Debug.Log($"[WaveService] Auto-starting waves in {currentLevelConfig.initialWaveDelay} seconds...");
                    AutoStartWavesAsync().Forget();
                }
                else
                {
                    Debug.Log("[WaveService] Manual wave start mode - use StartWaves() to begin");
                }
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
            
            // Очищаем всех врагов
            enemyService?.ClearAllEnemies();
            
            // Отменяем все асинхронные операции
            if (waveTokenSource != null)
            {
                waveTokenSource.Cancel();
                waveTokenSource.Dispose();
                waveTokenSource = null;
            }
            
            currentLevelMap = null;
            currentLevelConfig = null;
            currentLevelId = null;
            currentWaveIndex = -1;
            isWaveActive = false;
        }
        
        /// <summary>
        /// Обработчик завершения волны врагов от EnemyService.
        /// </summary>
        private void OnEnemyWaveCompleted(int waveNumber)
        {
            if (waveNumber == currentWaveIndex)
            {
                Debug.Log($"[WaveService] Wave {waveNumber + 1} completed by EnemyService");
                
                var waveConfig = currentLevelConfig.GetWave(currentWaveIndex);
                if (waveConfig != null)
                {
                    OnWaveCompleted(waveConfig);
                }
            }
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
            
            if (currentLevelConfig.WaveCount == 0)
            {
                Debug.LogWarning("[WaveService] Cannot start waves - no waves configured");
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
            
            if (waveTokenSource != null)
            {
                waveTokenSource.Cancel();
                waveTokenSource.Dispose();
                waveTokenSource = null;
            }
            
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
            
            // Проверяем, есть ли еще волны
            if (currentWaveIndex + 1 >= currentLevelConfig.WaveCount)
            {
                Debug.Log("[WaveService] All waves completed!");
                isWaveActive = false;
                OnAllWavesCompleted();
                return;
            }
            
            currentWaveIndex++;
            isWaveActive = true;
            
            var waveConfig = currentLevelConfig.GetWave(currentWaveIndex);
            if (waveConfig == null)
            {
                Debug.LogError($"[WaveService] Wave config not found for index: {currentWaveIndex}");
                return;
            }
            
            // Устанавливаем номер текущей волны в EnemyService
            enemyService?.SetCurrentWaveNumber(currentWaveIndex);
            
            Debug.Log($"[WaveService] Starting wave {currentWaveIndex + 1}/{currentLevelConfig.WaveCount}: {waveConfig.waveNumber}");
            
            // Отменяем предыдущую волну и запускаем новую
            if (waveTokenSource != null)
            {
                waveTokenSource.Cancel();
                waveTokenSource.Dispose();
            }
            
            waveTokenSource = new CancellationTokenSource();
            SpawnWaveAsync(waveConfig, waveTokenSource.Token).Forget();
        }
        
        /// <summary>
        /// Асинхронный спавн волны с задержками и группами врагов.
        /// </summary>
        private async UniTask SpawnWaveAsync(WaveConfig waveConfig, CancellationToken cancellationToken)
        {
            try
            {
                Debug.Log($"[WaveService] Wave {waveConfig.waveNumber} - waiting {waveConfig.delayBeforeWave}s before start");
                
                // Задержка перед началом волны
                await UniTask.Delay((int)(waveConfig.delayBeforeWave * 1000), cancellationToken: cancellationToken);
                
                Debug.Log($"[WaveService] Wave {waveConfig.waveNumber} started - spawning {waveConfig.enemyGroups.Count} enemy groups");
                
                // Спавним каждую группу врагов
                foreach (var group in waveConfig.enemyGroups)
                {
                    await SpawnEnemyGroupAsync(group, waveConfig, cancellationToken);
                }
                
                Debug.Log($"[WaveService] Wave {waveConfig.waveNumber} - all groups spawned");
                
                // Теперь ждем завершения волны через EnemyService
                // isWaveActive будет установлен в false в OnWaveCompleted
                
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log($"[WaveService] Wave {waveConfig.waveNumber} was cancelled");
            }
        }
        
        /// <summary>
        /// Асинхронный спавн группы врагов.
        /// </summary>
        private async UniTask SpawnEnemyGroupAsync(EnemyGroupConfig groupConfig, WaveConfig waveConfig, CancellationToken cancellationToken)
        {
            Debug.Log($"[WaveService] Spawning group: {groupConfig.count}x {groupConfig.enemyType} (delay: {groupConfig.spawnDelay}s)");
            
            // Задержка перед спавном группы
            if (groupConfig.spawnDelay > 0)
            {
                await UniTask.Delay((int)(groupConfig.spawnDelay * 1000), cancellationToken: cancellationToken);
            }
            
            // Спавним врагов группы с интервалами
            for (int i = 0; i < groupConfig.count; i++)
            {
                SpawnSingleEnemy(groupConfig, waveConfig);
                
                // Интервал между врагами в группе (с учетом модификатора скорости спавна)
                if (i < groupConfig.count - 1) // Не ждем после последнего врага
                {
                    float interval = groupConfig.intervalBetweenEnemies;
                    interval *= waveConfig.modifiers.GetSpawnSpeedMultiplier();
                    await UniTask.Delay((int)(interval * 1000), cancellationToken: cancellationToken);
                }
            }
            
            Debug.Log($"[WaveService] Group {groupConfig.enemyType} completed");
        }
        
        /// <summary>
        /// Спавн одного врага с использованием EnemyService.
        /// </summary>
        private void SpawnSingleEnemy(EnemyGroupConfig groupConfig, WaveConfig waveConfig)
        {
            var spawnPosition = currentLevelMap.SpawnPoint.transform.position;
            
            Debug.Log($"[WaveService] Spawning {groupConfig.enemyType} at {spawnPosition} " +
                     $"(HP: x{groupConfig.healthMultiplier * waveConfig.modifiers.globalHealthMultiplier}, " +
                     $"Speed: x{groupConfig.speedMultiplier * waveConfig.modifiers.globalSpeedMultiplier}, " +
                     $"Damage: x{groupConfig.damageMultiplier * waveConfig.modifiers.globalDamageMultiplier})");
            
            // ✅ Интеграция с EnemyService - заменили TODO на фактический спавн врагов
            var spawnedEnemy = enemyService.SpawnEnemyWithModifiers(
                groupConfig.enemyType, 
                spawnPosition, 
                groupConfig, 
                waveConfig.modifiers
            );
            
            if (spawnedEnemy == null)
            {
                Debug.LogError($"[WaveService] Failed to spawn enemy: {groupConfig.enemyType}");
            }
        }
        
        /// <summary>
        /// Получить номер текущей волны (1-based).
        /// </summary>
        public int GetCurrentWaveNumber()
        {
            return currentWaveIndex + 1;
        }
        
        /// <summary>
        /// Проверить активна ли волна.
        /// </summary>
        public bool IsWaveActive()
        {
            return isWaveActive;
        }
        
        /// <summary>
        /// Получить текущую конфигурацию волны.
        /// </summary>
        public WaveConfig GetCurrentWaveConfig()
        {
            if (currentLevelConfig == null || currentWaveIndex < 0)
                return null;
                
            return currentLevelConfig.GetWave(currentWaveIndex);
        }
        
        /// <summary>
        /// Получить общее количество волн на уровне.
        /// </summary>
        public int GetTotalWaveCount()
        {
            return currentLevelConfig?.WaveCount ?? 0;
        }
        
        /// <summary>
        /// Проверить можно ли запустить следующую волну досрочно.
        /// </summary>
        public bool CanTriggerNextWaveEarly()
        {
            return currentLevelConfig != null && 
                   currentLevelConfig.allowNextWaveEarly && 
                   !isWaveActive && 
                   currentWaveIndex + 1 < currentLevelConfig.WaveCount;
        }
        
        /// <summary>
        /// Вызывается когда волна завершена.
        /// </summary>
        private void OnWaveCompleted(WaveConfig waveConfig)
        {
            Debug.Log($"[WaveService] Wave {waveConfig.waveNumber} completed! Rewards: {waveConfig.goldReward} gold, {waveConfig.experienceReward} exp");
            
            isWaveActive = false;
            
            // TODO: Выдать награды игроку
            // playerService.AddGold(waveConfig.goldReward * waveConfig.modifiers.goldBonusMultiplier);
            // playerService.AddExperience(waveConfig.experienceReward);
            
            // Пауза перед следующей волной
            if (currentWaveIndex + 1 < currentLevelConfig.WaveCount)
            {
                Debug.Log($"[WaveService] Next wave will start in {currentLevelConfig.globalWaveDelay} seconds");
                WaitForNextWaveAsync().Forget();
            }
        }
        
        /// <summary>
        /// Асинхронный автоматический запуск волн.
        /// </summary>
        private async UniTask AutoStartWavesAsync()
        {
            try
            {
                if (currentLevelConfig == null)
                {
                    Debug.LogError("[WaveService] Cannot auto-start waves - no level config");
                    return;
                }
                
                // Ожидаем начальную задержку
                await UniTask.Delay((int)(currentLevelConfig.initialWaveDelay * 1000));
                
                // Проверяем, что уровень все еще загружен
                if (currentLevelConfig != null && currentLevelMap != null)
                {
                    Debug.Log("[WaveService] Auto-starting waves now!");
                    StartWaves();
                }
                else
                {
                    Debug.LogWarning("[WaveService] Auto-start cancelled - level was unloaded");
                }
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("[WaveService] Auto-start waves was cancelled");
            }
        }
        
        /// <summary>
        /// Ожидание перед следующей волной.
        /// </summary>
        private async UniTask WaitForNextWaveAsync()
        {
            try
            {
                await UniTask.Delay((int)(currentLevelConfig.globalWaveDelay * 1000));
                
                // Если игрок не запустил волну досрочно, запускаем автоматически
                if (!isWaveActive && currentWaveIndex + 1 < currentLevelConfig.WaveCount)
                {
                    SpawnNextWave();
                }
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("[WaveService] Wait for next wave was cancelled");
            }
        }
        
        /// <summary>
        /// Вызывается когда все волны завершены.
        /// </summary>
        private void OnAllWavesCompleted()
        {
            Debug.Log($"[WaveService] Level {currentLevelId} completed! Total rewards: {currentLevelConfig.TotalGoldReward} gold, {currentLevelConfig.TotalExperienceReward} exp");
            
            // TODO: Вызвать событие завершения уровня
            // levelService.OnLevelCompleted?.Invoke(currentLevelId, currentLevelConfig);
        }
    }
}
