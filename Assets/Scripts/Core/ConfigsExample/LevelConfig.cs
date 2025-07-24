using System.Collections.Generic;
using UnityEngine;

namespace Game.Configs
{
    /// <summary>
    /// Конфигурация игровой логики уровня (волны врагов).
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Configs/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level Info")]
        public string levelId;
        public string levelName;
        
        [Header("Enemy Waves")]
        public int waveCount = 5;
    }
}
