using UnityEngine;

namespace Game.Configs
{
    /// <summary>
    /// Визуальная конфигурация уровня (префаб уровня).
    /// </summary>
    [CreateAssetMenu(fileName = "LevelVisualConfig", menuName = "Game/Configs/Level Visual Config")]
    public class LevelVisualConfig : ScriptableObject
    {
        [Header("Level Info")]
        public string levelId;
        
        [Header("Visual")]
        public GameObject levelPrefab;
    }
}
