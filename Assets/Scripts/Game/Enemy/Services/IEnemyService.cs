using UnityEngine;
using Game.Wave;

namespace Game.Enemy.Services
{
    /// <summary>
    /// Интерфейс сервиса управления врагами
    /// </summary>
    public interface IEnemyService
    {
        /// <summary>
        /// Создать врага определенного типа
        /// </summary>
        Enemy SpawnEnemy(EnemyType enemyType, Vector3 position);
        
        /// <summary>
        /// Создать врага с модифицированными характеристиками
        /// </summary>
        Enemy SpawnEnemyWithModifiers(EnemyType enemyType, Vector3 position, EnemyGroupConfig groupConfig, WaveModifiers waveModifiers);
        
        /// <summary>
        /// Создать врага в точке спавна
        /// </summary>
        Enemy SpawnEnemyAtSpawnPoint(EnemyType enemyType);
        
        /// <summary>
        /// Получить количество живых врагов на поле
        /// </summary>
        int GetAliveEnemiesCount();
        
        /// <summary>
        /// Получить всех живых врагов
        /// </summary>
        Enemy[] GetAliveEnemies();
        
        /// <summary>
        /// Получить всех врагов определенной волны
        /// </summary>
        Enemy[] GetEnemiesByWave(int waveNumber);
        
        /// <summary>
        /// Уничтожить всех врагов
        /// </summary>
        void ClearAllEnemies();
        
        /// <summary>
        /// Проверить, завершена ли волна (все враги волны мертвы)
        /// </summary>
        bool IsWaveCompleted(int waveNumber);
        
        /// <summary>
        /// Установить карту уровня
        /// </summary>
        void SetLevelMap(Game.Path.LevelMap levelMap);
        
        /// <summary>
        /// Установить текущий номер волны для спавна
        /// </summary>
        void SetCurrentWaveNumber(int waveNumber);
        
        /// <summary>
        /// События
        /// </summary>
        System.Action<Enemy> OnEnemySpawned { get; set; }
        System.Action<Enemy> OnEnemyDied { get; set; }
        System.Action<Enemy> OnEnemyReachedBase { get; set; }
        System.Action<int> OnWaveCompleted { get; set; }
    }
}
