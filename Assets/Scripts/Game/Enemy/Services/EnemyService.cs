using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using Game.Services;
using Game.Enemy.Configs;
using Game.Path;

namespace Game.Enemy.Services
{
    /// <summary>
    /// Сервис управления врагами
    /// </summary>
    public class EnemyService : MonoBehaviour, IEnemyService
    {
        // Инжектируемые сервисы
        [Inject] private IPoolService _poolService;
        [Inject] private IGameFactory _gameFactory;
        [Inject] private IConfigService _configService;
        
        private EnemyConfigRepository _enemyConfigRepository;
        private List<Enemy> _aliveEnemies = new();
        private LevelMap _currentLevelMap;
        private System.Threading.CancellationTokenSource _cancellationTokenSource;
        
        // События
        public System.Action<Enemy> OnEnemySpawned { get; set; }
        public System.Action<Enemy> OnEnemyDied { get; set; }
        public System.Action<Enemy> OnEnemyReachedBase { get; set; }
        
        private void Start()
        {
            // Инициализируем cancellation token
            _cancellationTokenSource = new System.Threading.CancellationTokenSource();
            
            // Получаем репозиторий конфигураций
            _enemyConfigRepository = _configService.GetConfig<EnemyConfigRepository>();
            
            if (_enemyConfigRepository == null)
            {
                Debug.LogError("EnemyConfigRepository not found! Make sure it's in Resources/Configs/");
            }
            
            // Находим карту уровня
            _currentLevelMap = FindFirstObjectByType<LevelMap>();
            
            if (_currentLevelMap == null)
            {
                Debug.LogWarning("LevelMap not found in scene!");
            }
        }
        
        /// <summary>
        /// Создать врага определенного типа
        /// </summary>
        public Enemy SpawnEnemy(EnemyType enemyType, Vector3 position)
        {
            // Получаем конфигурации
            var enemyConfig = _enemyConfigRepository?.GetEnemyConfig(enemyType);
            var visualConfig = _enemyConfigRepository?.GetEnemyVisualConfig(enemyType);
            
            if (enemyConfig == null)
            {
                Debug.LogError($"No config found for enemy type: {enemyType}");
                return null;
            }
            
            if (visualConfig == null)
            {
                Debug.LogError($"No visual config found for enemy type: {enemyType}");
                return null;
            }
            
            if (visualConfig.enemyPrefab == null)
            {
                Debug.LogError($"No prefab found in visual config for enemy type: {enemyType}");
                return null;
            }
            
            // Создаем врага через пул
            var enemyComponent = _poolService.Get(
                visualConfig.enemyPrefab.GetComponent<Enemy>(),
                position,
                Quaternion.identity
            );
            
            if (enemyComponent == null)
            {
                Debug.LogError($"Failed to create enemy of type: {enemyType}");
                return null;
            }
            
            // Инициализируем врага
            enemyComponent.Initialize(enemyConfig, position);
            
            // Добавляем в список живых врагов
            _aliveEnemies.Add(enemyComponent);
            
            // Подписываемся на события врага
            SubscribeToEnemyEvents(enemyComponent);
            
            // Запускаем движение, если есть карта
            if (_currentLevelMap != null)
            {
                enemyComponent.StartMovement(_currentLevelMap);
            }
            
            // Вызываем событие
            OnEnemySpawned?.Invoke(enemyComponent);
            
            Debug.Log($"Spawned enemy: {enemyType} at {position}");
            
            return enemyComponent;
        }
        
        /// <summary>
        /// Создать врага в точке спавна
        /// </summary>
        public Enemy SpawnEnemyAtSpawnPoint(EnemyType enemyType)
        {
            if (_currentLevelMap == null || _currentLevelMap.SpawnPoint == null)
            {
                Debug.LogError("No spawn point available!");
                return null;
            }
            
            Vector3 spawnPosition = _currentLevelMap.SpawnPoint.GetRandomSpawnPosition();
            return SpawnEnemy(enemyType, spawnPosition);
        }
        
        /// <summary>
        /// Получить количество живых врагов на поле
        /// </summary>
        public int GetAliveEnemiesCount()
        {
            // Очищаем список от null и мертвых врагов
            CleanupEnemiesList();
            return _aliveEnemies.Count;
        }
        
        /// <summary>
        /// Получить всех живых врагов
        /// </summary>
        public Enemy[] GetAliveEnemies()
        {
            CleanupEnemiesList();
            return _aliveEnemies.ToArray();
        }
        
        /// <summary>
        /// Уничтожить всех врагов
        /// </summary>
        public void ClearAllEnemies()
        {
            var enemiesToClear = _aliveEnemies.ToArray();
            
            foreach (var enemy in enemiesToClear)
            {
                if (enemy != null)
                {
                    enemy.GetHealthComponent()?.Kill();
                }
            }
            
            _aliveEnemies.Clear();
        }
        
        /// <summary>
        /// Подписка на события врага
        /// </summary>
        private void SubscribeToEnemyEvents(Enemy enemy)
        {
            if (enemy?.IsAlive != null)
            {
                // Используем UniTask для мониторинга
                MonitorEnemyHealth(enemy).Forget();
            }
        }
        
        /// <summary>
        /// Мониторинг здоровья врага
        /// </summary>
        private async UniTaskVoid MonitorEnemyHealth(Enemy enemy)
        {
            bool wasAlive = true;
            var cancellationToken = _cancellationTokenSource.Token;
            
            try
            {
                while (enemy != null && enemy.gameObject != null && !cancellationToken.IsCancellationRequested)
                {
                    bool isCurrentlyAlive = enemy.IsAlive.Value;
                    
                    if (wasAlive && !isCurrentlyAlive)
                    {
                        HandleEnemyDied(enemy);
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
        }
        
        /// <summary>
        /// Обработка смерти врага
        /// </summary>
        private void HandleEnemyDied(Enemy enemy)
        {
            if (enemy == null) return;
            
            // Убираем из списка живых
            _aliveEnemies.Remove(enemy);
            
            // Вызываем событие
            OnEnemyDied?.Invoke(enemy);
            
            Debug.Log($"Enemy {enemy.EnemyType} died");
        }
        
        /// <summary>
        /// Очистка списка врагов от null и мертвых
        /// </summary>
        private void CleanupEnemiesList()
        {
            _aliveEnemies = _aliveEnemies.Where(enemy => 
                enemy != null && 
                enemy.gameObject != null && 
                enemy.IsAlive.Value
            ).ToList();
        }
        
        /// <summary>
        /// Установить карту уровня
        /// </summary>
        public void SetLevelMap(LevelMap levelMap)
        {
            _currentLevelMap = levelMap;
        }
        
        private void Update()
        {
            // Периодически очищаем список врагов
            if (Time.frameCount % 60 == 0) // Каждую секунду при 60 FPS
            {
                CleanupEnemiesList();
            }
        }
        
        private void OnDestroy()
        {
            // Отменяем все асинхронные операции
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}