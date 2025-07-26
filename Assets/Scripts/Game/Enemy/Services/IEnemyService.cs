using UnityEngine;

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
        /// Уничтожить всех врагов
        /// </summary>
        void ClearAllEnemies();
        
        /// <summary>
        /// События
        /// </summary>
        System.Action<Enemy> OnEnemySpawned { get; set; }
        System.Action<Enemy> OnEnemyDied { get; set; }
        System.Action<Enemy> OnEnemyReachedBase { get; set; }
    }
}