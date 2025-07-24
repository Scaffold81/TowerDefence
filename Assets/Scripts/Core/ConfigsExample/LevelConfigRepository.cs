using System.Collections.Generic;
using UnityEngine;

namespace Game.Configs
{
    /// <summary>
    /// Репозиторий конфигураций уровней.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfigRepository", menuName = "Game/Configs/Level Config Repository")]
    public class LevelConfigRepository : ScriptableObject
    {
        [Header("Level Configs")]
        [SerializeField] private List<LevelConfig> levelConfigs = new List<LevelConfig>();
        
        [Header("Visual Configs")]
        [SerializeField] private List<LevelVisualConfig> visualConfigs = new List<LevelVisualConfig>();
        
        public IReadOnlyList<LevelConfig> LevelConfigs => levelConfigs;
        public IReadOnlyList<LevelVisualConfig> VisualConfigs => visualConfigs;
        
        /// <summary>
        /// Получить конфигурацию уровня по ID.
        /// </summary>
        public LevelConfig GetLevelConfig(string levelId)
        {
            return levelConfigs.Find(config => config.levelId == levelId);
        }
        
        /// <summary>
        /// Получить визуальную конфигурацию уровня по ID.
        /// </summary>
        public LevelVisualConfig GetVisualConfig(string levelId)
        {
            return visualConfigs.Find(config => config.levelId == levelId);
        }
    }
}
