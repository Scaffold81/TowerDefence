using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using Game.Services;
using Game.Configs.Enemy;
using Game.Path;
using Game.Wave;

namespace Game.Enemy.Services
{
    /// <summary>
    /// Сервис управления врагами
    /// </summary>
    public class EnemyService : IEnemyService, IInitializable, IDisposable
    {
        // Инжектируемые сервисы
        [Inject] private IPoolService poolService;
        [Inject] private IGameFactory gameFactory;
        [Inject] private EnemyConfigRepository enemyConfigRepository;

        private List<Enemy> aliveEnemies = new();
        private Dictionary<int, List<Enemy>> enemiesByWave = new();
        private LevelMap currentLevelMap;
        private System.Threading.CancellationTokenSource cancellationTokenSource;
        private int currentWaveNumber = 0; // Текущий номер волны для спавна
        private bool isDisposed = false;
        
        // События
        public System.Action<Enemy> OnEnemySpawned { get; set; }
        public System.Action<Enemy> OnEnemyDied { get; set; }
        public System.Action<Enemy> OnEnemyReachedBase { get; set; }
        public System.Action<int> OnWaveCompleted { get; set; }
        
        public void Initialize()
        {
            Debug.Log("[EnemyService] Initializing enemy service...");
            
            // Инициализируем cancellation token
            cancellationTokenSource = new System.Threading.CancellationTokenSource();
            
            // Проверяем репозиторий конфигураций
            if (enemyConfigRepository == null)
            {
                Debug.LogError("[EnemyService] EnemyConfigRepository not found! Make sure it's in Resources/Configs/");
            }
            else
            {
                Debug.Log("[EnemyService] EnemyConfigRepository loaded successfully");
            }
            
            // Находим карту уровня
            currentLevelMap = UnityEngine.Object.FindFirstObjectByType<LevelMap>();
            
            if (currentLevelMap == null)
            {
                Debug.LogWarning("[EnemyService] LevelMap not found in scene!");
            }
            else
            {
                Debug.Log($"[EnemyService] LevelMap found with {currentLevelMap.Waypoints.Count} waypoints");
            }
            
            Debug.Log("[EnemyService] Enemy service initialized");
            
            // Запускаем периодическую очистку
            StartPeriodicCleanup().Forget();
        }
        
        /// <summary>
        /// Создать врага определенного типа
        /// </summary>
        public Enemy SpawnEnemy(EnemyType enemyType, Vector3 position)
        {
            return SpawnEnemyInternal(enemyType, position, null, null, -1);
        }
        
        /// <summary>
        /// Создать врага с модифицированными характеристиками
        /// </summary>
        public Enemy SpawnEnemyWithModifiers(EnemyType enemyType, Vector3 position, EnemyGroupConfig groupConfig, WaveModifiers waveModifiers)
        {
            // Используем текущий номер волны
            return SpawnEnemyInternal(enemyType, position, groupConfig, waveModifiers, currentWaveNumber);
        }
        
        /// <summary>
        /// Создать врага в точке спавна
        /// </summary>
        public Enemy SpawnEnemyAtSpawnPoint(EnemyType enemyType)
        {
            if (currentLevelMap == null || currentLevelMap.SpawnPoint == null)
            {
                Debug.LogError("[EnemyService] No spawn point available!");
                return null;
            }
            
            Vector3 spawnPosition = currentLevelMap.SpawnPoint.GetRandomSpawnPosition();
            return SpawnEnemy(enemyType, spawnPosition);
        }
        
        /// <summary>
        /// Установить текущий номер волны для спавна
        /// </summary>
        public void SetCurrentWaveNumber(int waveNumber)
        {
            currentWaveNumber = waveNumber;
            Debug.Log($"[EnemyService] Current wave number set to: {waveNumber}");
        }
        
        /// <summary>
        /// Внутренний метод создания врага с поддержкой модификаторов и волн
        /// </summary>
        private Enemy SpawnEnemyInternal(EnemyType enemyType, Vector3 position, EnemyGroupConfig groupConfig, WaveModifiers waveModifiers, int waveNumber)
        {
            // Получаем конфигурации
            var enemyConfig = enemyConfigRepository?.GetEnemyConfig(enemyType);
            var visualConfig = enemyConfigRepository?.GetEnemyVisualConfig(enemyType);
            
            Debug.Log($"[EnemyService] EnemyConfigRepository has {enemyConfigRepository?.EnemyConfigs?.Count ?? 0} configs and {enemyConfigRepository?.EnemyVisualConfigs?.Count ?? 0} visual configs");
            Debug.Log($"[EnemyService] Trying to spawn enemy type: {enemyType} (int value: {(int)enemyType})");
            
            if (enemyConfig == null)
            {
                Debug.LogError($"[EnemyService] No config found for enemy type: {enemyType}");
                return null;
            }
            
            if (visualConfig == null)
            {
                Debug.LogError($"[EnemyService] No visual config found for enemy type: {enemyType}");
                return null;
            }
            
            Debug.Log($"[EnemyService] Visual config found for {enemyType}. Prefab: {(visualConfig.enemyPrefab != null ? visualConfig.enemyPrefab.name : "NULL")}");
            
            if (visualConfig.enemyPrefab == null)
            {
                Debug.LogError($"[EnemyService] No prefab found in visual config for enemy type: {enemyType}");
                return null;
            }
            
            var enemyComponentOnPrefab = visualConfig.enemyPrefab.GetComponent<Enemy>();
            if (enemyComponentOnPrefab == null)
            {
                Debug.LogError($"[EnemyService] Prefab {visualConfig.enemyPrefab.name} does not have Enemy component!");
                return null;
            }
            
            Debug.Log($"[EnemyService] About to spawn {enemyType} using prefab {visualConfig.enemyPrefab.name} at {position}");
            
            // Создаем врага через пул
            var enemyComponent = poolService.Get(
                visualConfig.enemyPrefab.GetComponent<Enemy>(),
                position,
                Quaternion.identity
            );
            
            if (enemyComponent == null)
            {
                Debug.LogError($"[EnemyService] Failed to create enemy of type: {enemyType}");
                return null;
            }
            
            // Клонируем конфигурацию для применения модификаторов
            var modifiedConfig = enemyConfig;
            if (groupConfig != null && waveModifiers != null)
            {
                modifiedConfig = ApplyModifiers(enemyConfig, groupConfig, waveModifiers);
            }
            
            // Инициализируем врага
            enemyComponent.Initialize(modifiedConfig, position);
            
            // Добавляем в список живых врагов
            aliveEnemies.Add(enemyComponent);
            
            // Добавляем в список врагов по волнам
            if (waveNumber >= 0)
            {
                if (!enemiesByWave.ContainsKey(waveNumber))
                {
                    enemiesByWave[waveNumber] = new List<Enemy>();
                }
                enemiesByWave[waveNumber].Add(enemyComponent);
            }
            
            // Подписываемся на события врага
            SubscribeToEnemyEvents(enemyComponent, waveNumber);
            
            // Подписываемся на событие достижения конечной точки
            var movementComponent = enemyComponent.GetMovementComponent();
            if (movementComponent != null)
            {
                movementComponent.OnReachedEndPoint += () => HandleEnemyReachedBase(enemyComponent, waveNumber);
            }
            
            // Запускаем движение, если есть карта
            if (currentLevelMap != null)
            {
                enemyComponent.StartMovement(currentLevelMap);
            }
            
            // Вызываем событие
            OnEnemySpawned?.Invoke(enemyComponent);
            
            string modifiersInfo = "";
            if (groupConfig != null && waveModifiers != null)
            {
                modifiersInfo = $" (HP: x{groupConfig.healthMultiplier * waveModifiers.globalHealthMultiplier:F2}, " +
                               $"Speed: x{groupConfig.speedMultiplier * waveModifiers.globalSpeedMultiplier:F2}, " +
                               $"Damage: x{groupConfig.damageMultiplier * waveModifiers.globalDamageMultiplier:F2})";
            }
            
            Debug.Log($"[EnemyService] Spawned enemy: {enemyType} at {position} for wave {waveNumber}{modifiersInfo}");
            
            return enemyComponent;
        }
        
        /// <summary>
        /// Применить модификаторы к конфигурации врага
        /// </summary>
        private EnemyConfig ApplyModifiers(EnemyConfig originalConfig, EnemyGroupConfig groupConfig, WaveModifiers waveModifiers)
        {
            // Создаем копию конфигурации
            var modifiedConfig = ScriptableObject.CreateInstance<EnemyConfig>();
            
            // Копируем основные параметры
            modifiedConfig.enemyType = originalConfig.enemyType;
            modifiedConfig.category = originalConfig.category;
            
            // Применяем модификаторы здоровья
            modifiedConfig.maxHealth = Mathf.RoundToInt(
                originalConfig.maxHealth * 
                groupConfig.healthMultiplier * 
                waveModifiers.globalHealthMultiplier
            );
            
            // Применяем модификаторы скорости
            modifiedConfig.speed = originalConfig.speed * 
                                  groupConfig.speedMultiplier * 
                                  waveModifiers.globalSpeedMultiplier;
            
            // Применяем модификаторы урона
            modifiedConfig.damage = Mathf.RoundToInt(
                originalConfig.damage * 
                groupConfig.damageMultiplier * 
                waveModifiers.globalDamageMultiplier
            );
            
            // Копируем остальные параметры без изменений
            modifiedConfig.goldReward = originalConfig.goldReward;
            modifiedConfig.experienceReward = originalConfig.experienceReward;
            modifiedConfig.resistances = originalConfig.resistances;
            modifiedConfig.abilities = originalConfig.abilities;
            
            return modifiedConfig;
        }
        
        /// <summary>
        /// Получить количество живых врагов на поле
        /// </summary>
        public int GetAliveEnemiesCount()
        {
            CleanupEnemiesList();
            return aliveEnemies.Count;
        }
        
        /// <summary>
        /// Получить всех живых врагов
        /// </summary>
        public Enemy[] GetAliveEnemies()
        {
            CleanupEnemiesList();
            return aliveEnemies.ToArray();
        }
        
        /// <summary>
        /// Получить всех врагов определенной волны
        /// </summary>
        public Enemy[] GetEnemiesByWave(int waveNumber)
        {
            if (!enemiesByWave.ContainsKey(waveNumber))
                return new Enemy[0];
            
            // Очищаем список от мертвых врагов
            enemiesByWave[waveNumber] = enemiesByWave[waveNumber].Where(enemy => 
                enemy != null && enemy.IsAliveSafe()
            ).ToList();
            
            return enemiesByWave[waveNumber].ToArray();
        }
        
        /// <summary>
        /// Проверить, завершена ли волна (все враги волны мертвы)
        /// </summary>
        public bool IsWaveCompleted(int waveNumber)
        {
            return GetEnemiesByWave(waveNumber).Length == 0;
        }
        
        /// <summary>
        /// Уничтожить всех врагов
        /// </summary>
        public void ClearAllEnemies()
        {
            Debug.Log("[EnemyService] Clearing all enemies");
            
            var enemiesToClear = aliveEnemies.ToArray();
            
            foreach (var enemy in enemiesToClear)
            {
                if (enemy != null)
                {
                    // Отписываемся от событий движения
                    var movementComponent = enemy.GetMovementComponent();
                    if (movementComponent != null)
                    {
                        movementComponent.OnReachedEndPoint = null;
                    }
                    
                    enemy.GetHealthComponent()?.Kill();
                }
            }
            
            aliveEnemies.Clear();
            enemiesByWave.Clear();
            currentWaveNumber = 0;
        }
        
        /// <summary>
        /// Установить карту уровня
        /// </summary>
        public void SetLevelMap(LevelMap levelMap)
        {
            currentLevelMap = levelMap;
            Debug.Log($"[EnemyService] Level map set: {levelMap?.name}");
        }
        
        /// <summary>
        /// Подписка на события врага
        /// </summary>
        private void SubscribeToEnemyEvents(Enemy enemy, int waveNumber)
        {
            if (enemy != null && enemy.gameObject != null && enemy.gameObject.activeInHierarchy)
            {
                // Проверяем, что IsAlive инициализирован и не disposed
                if (enemy.IsAlive != null && !enemy.IsAlive.IsDisposed)
                {
                    // Проверяем, что HealthComponent существует
                    var healthComponent = enemy.GetHealthComponent();
                    if (healthComponent != null)
                    {
                        // Используем UniTask для мониторинга
                        MonitorEnemyHealth(enemy, waveNumber).Forget();
                    }
                    else
                    {
                        Debug.LogWarning($"[EnemyService] Enemy {enemy.name} HealthComponent is null! Skipping health monitoring.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[EnemyService] Enemy {enemy.name} IsAlive property is null or disposed! Skipping health monitoring.");
                }
            }
            else
            {
                Debug.LogWarning($"[EnemyService] Enemy is null, destroyed or inactive! Skipping event subscription.");
            }
        }
        
        /// <summary>
        /// Мониторинг здоровья врага
        /// </summary>
        private async UniTaskVoid MonitorEnemyHealth(Enemy enemy, int waveNumber)
        {
            // Проверка на начальное состояние
            if (enemy == null)
            {
                Debug.LogError("[EnemyService] MonitorEnemyHealth called with null enemy!");
                return;
            }
            
            if (isDisposed)
            {
                Debug.LogWarning("[EnemyService] MonitorEnemyHealth called but service is disposed");
                return;
            }
            
            if (cancellationTokenSource == null || cancellationTokenSource.Token.IsCancellationRequested)
            {
                return; // Молчаливо выходим, это нормально при dispose
            }
            
            bool wasAlive = true;
            var cancellationToken = cancellationTokenSource.Token;
            
            try
            {
                while (enemy != null && enemy.gameObject != null && !cancellationToken.IsCancellationRequested)
                {
                    // Проверяем, что враг все еще активен и не начал уничтожение
                    if (!enemy.gameObject.activeInHierarchy)
                    {
                        Debug.Log($"[EnemyService] Enemy {enemy.name} became inactive, stopping monitoring");
                        return;
                    }
                    
                    // Проверяем, что IsAlive все еще существует и не disposed
                    if (enemy.IsAlive == null || enemy.IsAlive.IsDisposed)
                    {
                        Debug.LogWarning($"[EnemyService] Enemy {enemy.name} IsAlive became null or disposed during monitoring!");
                        return;
                    }
                    
                    // Проверяем, что HealthComponent все еще существует
                    var healthComponent = enemy.GetHealthComponent();
                    if (healthComponent == null)
                    {
                        Debug.LogWarning($"[EnemyService] Enemy {enemy.name} HealthComponent became null during monitoring!");
                        return;
                    }
                    
                    bool isCurrentlyAlive;
                    try
                    {
                        isCurrentlyAlive = enemy.IsAliveSafe();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[EnemyService] Error checking IsAliveSafe for enemy {enemy?.name}: {ex.Message}");
                        return;
                    }
                    
                    // Если метод вернул false, значит враг больше не валиден
                    if (!isCurrentlyAlive && !wasAlive)
                    {
                        // Враг уже не валиден, прекращаем мониторинг
                        return;
                    }
                    
                    if (wasAlive && !isCurrentlyAlive)
                    {
                        HandleEnemyDied(enemy, waveNumber);
                        return;
                    }
                    
                    wasAlive = isCurrentlyAlive;
                    await UniTask.Delay(100, cancellationToken: cancellationToken);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Нормальное завершение при отмене
                return;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EnemyService] Unexpected error in MonitorEnemyHealth: {ex.Message}");
                Debug.LogException(ex);
            }
        }
        
        /// <summary>
        /// Обработка достижения врагом базы
        /// </summary>
        private void HandleEnemyReachedBase(Enemy enemy, int waveNumber)
        {
            if (enemy == null) return;
            
            Debug.Log($"[EnemyService] Enemy {enemy.EnemyType} reached player base!");
            
            // Убираем из списка живых
            aliveEnemies.Remove(enemy);
            
            // Убираем из списка волны
            if (waveNumber >= 0 && enemiesByWave.ContainsKey(waveNumber))
            {
                enemiesByWave[waveNumber].Remove(enemy);
                
                // Проверяем, завершена ли волна
                if (IsWaveCompleted(waveNumber))
                {
                    Debug.Log($"[EnemyService] Wave {waveNumber} completed after enemy reached base!");
                    OnWaveCompleted?.Invoke(waveNumber);
                }
            }
            
            // Вызываем событие
            OnEnemyReachedBase?.Invoke(enemy);
            
            Debug.Log($"[EnemyService] Enemy {enemy.EnemyType} handled for reaching base (will be returned to pool by Enemy class)");
        }
        /// <summary>
        /// Обработка смерти врага
        /// </summary>
        private void HandleEnemyDied(Enemy enemy, int waveNumber)
        {
            if (enemy == null) return;
            
            // Убираем из списка живых
            aliveEnemies.Remove(enemy);
            
            // Убираем из списка волны
            if (waveNumber >= 0 && enemiesByWave.ContainsKey(waveNumber))
            {
                enemiesByWave[waveNumber].Remove(enemy);
                
                // Проверяем, завершена ли волна
                if (IsWaveCompleted(waveNumber))
                {
                    Debug.Log($"[EnemyService] Wave {waveNumber} completed!");
                    OnWaveCompleted?.Invoke(waveNumber);
                }
            }
            
            // Вызываем событие
            OnEnemyDied?.Invoke(enemy);
            
            Debug.Log($"[EnemyService] Enemy {enemy.EnemyType} died");
        }
        
        /// <summary>
        /// Очистка списка врагов от null и мертвых
        /// </summary>
        private void CleanupEnemiesList()
        {
            if (isDisposed) return;
            
            aliveEnemies = aliveEnemies.Where(enemy => 
                enemy != null && enemy.IsAliveSafe()
            ).ToList();
        }
        
        /// <summary>
        /// Периодическая очистка списка врагов
        /// </summary>
        private async UniTaskVoid StartPeriodicCleanup()
        {
            try
            {
                while (!isDisposed && !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await UniTask.Delay(1000, cancellationToken: cancellationTokenSource.Token); // Каждую секунду
                    CleanupEnemiesList();
                }
            }
            catch (System.OperationCanceledException)
            {
                // Нормальное завершение
            }
        }
        
        public void Dispose()
        {
            if (isDisposed) return;
            
            Debug.Log("[EnemyService] Disposing enemy service");
            
            isDisposed = true;
            
            // Отменяем все асинхронные операции
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}
