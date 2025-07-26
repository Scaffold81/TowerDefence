using System.Collections.Generic;
using UnityEngine;
using Game.Enemy;

namespace Game.Wave
{
    /// <summary>
    /// Конфигурация одной волны врагов.
    /// </summary>
    [System.Serializable]
    public class WaveConfig
    {
        [Header("Wave Info")]
        [Tooltip("Номер волны (для отображения игроку)")]
        public int waveNumber = 1;
        
        [Tooltip("Задержка перед стартом волны (в секундах)")]
        [Range(0f, 30f)]
        public float delayBeforeWave = 5f;
        
        [Tooltip("Базовый интервал между спавном врагов (в секундах)")]
        [Range(0.1f, 5f)]
        public float timeBetweenSpawns = 1f;

        [Header("Enemy Groups")]
        [Tooltip("Группы врагов в этой волне")]
        public List<EnemyGroupConfig> enemyGroups = new List<EnemyGroupConfig>();

        [Header("Wave Rewards")]
        [Tooltip("Награда золотом за прохождение волны")]
        [Range(0, 1000)]
        public int goldReward = 100;
        
        [Tooltip("Награда опытом за прохождение волны")]
        [Range(0, 500)]
        public int experienceReward = 50;

        [Header("Wave Modifiers")]
        [Tooltip("Глобальные модификаторы для всех врагов в волне")]
        public WaveModifiers modifiers = new WaveModifiers();

        /// <summary>
        /// Получить общее количество врагов в волне.
        /// </summary>
        public int GetTotalEnemyCount()
        {
            int total = 0;
            foreach (var group in enemyGroups)
            {
                total += group.count;
            }
            return total;
        }

        /// <summary>
        /// Получить общую продолжительность волны (приблизительно).
        /// </summary>
        public float GetEstimatedDuration()
        {
            float duration = delayBeforeWave;
            
            foreach (var group in enemyGroups)
            {
                duration += group.spawnDelay;
                duration += (group.count - 1) * group.intervalBetweenEnemies;
            }
            
            return duration;
        }

        /// <summary>
        /// Валидация конфигурации волны.
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (enemyGroups == null || enemyGroups.Count == 0)
            {
                errorMessage = $"Wave {waveNumber}: No enemy groups defined";
                return false;
            }

            for (int i = 0; i < enemyGroups.Count; i++)
            {
                if (!enemyGroups[i].IsValid(out string groupError))
                {
                    errorMessage = $"Wave {waveNumber}, Group {i}: {groupError}";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Создать копию конфигурации волны.
        /// </summary>
        public WaveConfig Clone()
        {
            var clone = new WaveConfig
            {
                waveNumber = waveNumber,
                delayBeforeWave = delayBeforeWave,
                timeBetweenSpawns = timeBetweenSpawns,
                goldReward = goldReward,
                experienceReward = experienceReward,
                modifiers = modifiers.Clone(),
                enemyGroups = new List<EnemyGroupConfig>()
            };

            foreach (var group in enemyGroups)
            {
                clone.enemyGroups.Add(group.Clone());
            }

            return clone;
        }
    }
}
