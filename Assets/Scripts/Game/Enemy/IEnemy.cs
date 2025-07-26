using UnityEngine;
using R3;
using Game.Path;
using Game.Configs.Enemy;

namespace Game.Enemy
{
    /// <summary>
    /// Базовый интерфейс для всех врагов
    /// </summary>
    public interface IEnemy
    {
        EnemyType EnemyType { get; }
        EnemyCategory Category { get; }
        
        ReactiveProperty<float> Health { get; }
        ReactiveProperty<float> MaxHealth { get; }
        ReactiveProperty<Vector3> Position { get; }
        ReactiveProperty<bool> IsAlive { get; }
        ReactiveProperty<bool> IsMoving { get; }
        
        /// <summary>
        /// Инициализация врага с конфигурацией
        /// </summary>
        void Initialize(EnemyConfig config, Vector3 spawnPosition);
        
        /// <summary>
        /// Запуск движения по пути
        /// </summary>
        void StartMovement(LevelMap levelMap);
        
        /// <summary>
        /// Остановка движения
        /// </summary>
        void StopMovement();
        
        /// <summary>
        /// Получение текущей позиции
        /// </summary>
        Vector3 GetPosition();
        
        /// <summary>
        /// Получение направления движения
        /// </summary>
        Vector3 GetMovementDirection();
    }
}