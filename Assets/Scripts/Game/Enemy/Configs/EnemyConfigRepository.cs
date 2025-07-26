using Game.Enemy;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Configs.Enemy
{
    /// <summary>
    /// Репозиторий конфигураций врагов
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfigRepository", menuName = "Game/Configs/Enemy Config Repository")]
    public class EnemyConfigRepository : ScriptableObject
    {
        [Header("Enemy Configs")]
        [SerializeField] private List<EnemyConfig> enemyConfigs = new();
        
        [Header("Visual Configs")]
        [SerializeField] private List<EnemyVisualConfig> enemyVisualConfigs = new();
        
        public IReadOnlyList<EnemyConfig> EnemyConfigs => enemyConfigs;
        public IReadOnlyList<EnemyVisualConfig> EnemyVisualConfigs => enemyVisualConfigs;
        
        /// <summary>
        /// Получить конфигурацию врага по типу
        /// </summary>
        public EnemyConfig GetEnemyConfig(EnemyType enemyType)
        {
            return enemyConfigs.FirstOrDefault(config => config.enemyType == enemyType);
        }
        
        /// <summary>
        /// Получить визуальную конфигурацию врага по типу
        /// </summary>
        public EnemyVisualConfig GetEnemyVisualConfig(EnemyType enemyType)
        {
            return enemyVisualConfigs.Find(config => config.enemyType == enemyType);
        }
        
        /// <summary>
        /// Получить конфигурацию врага по ID (имени)
        /// </summary>
        public EnemyConfig GetEnemyConfigById(string enemyId)
        {
            return enemyConfigs.Find(config => config.name == enemyId);
        }
        
        /// <summary>
        /// Получить все конфигурации определенной категории
        /// </summary>
        public List<EnemyConfig> GetEnemyConfigsByCategory(EnemyCategory category)
        {
            return enemyConfigs.FindAll(config => config.category == category);
        }
        
        /// <summary>
        /// Проверить, есть ли конфигурация для типа врага
        /// </summary>
        public bool HasEnemyConfig(EnemyType enemyType)
        {
            return GetEnemyConfig(enemyType) != null;
        }
        
        /// <summary>
        /// Проверить, есть ли визуальная конфигурация для типа врага
        /// </summary>
        public bool HasEnemyVisualConfig(EnemyType enemyType)
        {
            return GetEnemyVisualConfig(enemyType) != null;
        }
        
        private void OnValidate()
        {
            // Проверяем на дублирующиеся типы врагов
            ValidateDuplicateEnemyTypes();
            ValidateDuplicateVisualTypes();
        }
        
        private void ValidateDuplicateEnemyTypes()
        {
            var usedTypes = new HashSet<EnemyType>();
            
            foreach (var config in enemyConfigs)
            {
                if (config != null && !usedTypes.Add(config.enemyType))
                {
                    Debug.LogError($"Duplicate enemy type {config.enemyType} found in {name}!");
                }
            }
        }
        
        private void ValidateDuplicateVisualTypes()
        {
            var usedTypes = new HashSet<EnemyType>();
            
            foreach (var config in enemyVisualConfigs)
            {
                if (config != null && !usedTypes.Add(config.enemyType))
                {
                    Debug.LogError($"Duplicate visual config for enemy type {config.enemyType} found in {name}!");
                }
            }
        }
    }
}