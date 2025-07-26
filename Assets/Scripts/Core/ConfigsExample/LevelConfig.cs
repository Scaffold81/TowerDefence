using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Wave;

namespace Game.Configs
{
    /// <summary>
    /// Конфигурация игровой логики уровня (волны врагов).
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Configs/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level Info")]
        [Tooltip("Уникальный идентификатор уровня")]
        public string levelId;
        
        [Tooltip("Отображаемое название уровня")]
        public string levelName;
        
        [Tooltip("Описание уровня для игрока")]
        [TextArea(2, 4)]
        public string levelDescription;

        [Header("Enemy Waves")]
        [Tooltip("Конфигурации всех волн врагов на уровне")]
        public List<WaveConfig> waves = new List<WaveConfig>();

        [Header("Wave Settings")]
        [Tooltip("Глобальная задержка между волнами (в секундах)")]
        [Range(5f, 60f)]
        public float globalWaveDelay = 10f;
        
        [Tooltip("Можно ли запустить следующую волну досрочно")]
        public bool allowNextWaveEarly = true;
        
        [Tooltip("Через сколько секунд появляется кнопка 'Следующая волна'")]
        [Range(1f, 30f)]
        public float earlyWaveButtonDelay = 3f;
        
        [Tooltip("Автоматически запускать волны при старте уровня")]
        public bool autoStartWaves = true;
        
        [Tooltip("Задержка перед началом первой волны (в секундах)")]
        [Range(0f, 30f)]
        public float initialWaveDelay = 5f;

        [Header("Level Rewards")]
        [Tooltip("Базовая награда золотом за прохождение уровня")]
        [Range(0, 5000)]
        public int baseLevelGoldReward = 500;
        
        [Tooltip("Базовая награда опытом за прохождение уровня")]
        [Range(0, 2000)]
        public int baseLevelExperienceReward = 200;

        [Header("Difficulty Settings")]
        [Tooltip("Автоматически масштабировать сложность волн")]
        public bool useAutoScaling = true;
        
        [Tooltip("Коэффициент роста сложности между волнами")]
        [Range(1.0f, 2.0f)]
        public float difficultyScaling = 1.1f;

        #region Properties

        /// <summary>
        /// Получить количество волн.
        /// </summary>
        public int WaveCount => waves?.Count ?? 0;

        /// <summary>
        /// Получить общее количество врагов на уровне.
        /// </summary>
        public int TotalEnemyCount
        {
            get
            {
                if (waves == null) return 0;
                return waves.Sum(wave => wave.GetTotalEnemyCount());
            }
        }

        /// <summary>
        /// Получить общую награду золотом за уровень.
        /// </summary>
        public int TotalGoldReward
        {
            get
            {
                int waveGold = waves?.Sum(wave => wave.goldReward) ?? 0;
                return baseLevelGoldReward + waveGold;
            }
        }

        /// <summary>
        /// Получить общую награду опытом за уровень.
        /// </summary>
        public int TotalExperienceReward
        {
            get
            {
                int waveExperience = waves?.Sum(wave => wave.experienceReward) ?? 0;
                return baseLevelExperienceReward + waveExperience;
            }
        }

        /// <summary>
        /// Получить приблизительную продолжительность уровня.
        /// </summary>
        public float EstimatedDuration
        {
            get
            {
                if (waves == null || waves.Count == 0) return 0f;
                
                float duration = 0f;
                for (int i = 0; i < waves.Count; i++)
                {
                    duration += waves[i].GetEstimatedDuration();
                    if (i < waves.Count - 1) // Не добавляем задержку после последней волны
                    {
                        duration += globalWaveDelay;
                    }
                }
                return duration;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Получить конфигурацию волны по номеру.
        /// </summary>
        public WaveConfig GetWave(int waveIndex)
        {
            if (waves == null || waveIndex < 0 || waveIndex >= waves.Count)
                return null;
            return waves[waveIndex];
        }

        /// <summary>
        /// Добавить новую волну.
        /// </summary>
        public void AddWave(WaveConfig wave)
        {
            if (waves == null)
                waves = new List<WaveConfig>();
            
            wave.waveNumber = waves.Count + 1;
            waves.Add(wave);
        }

        /// <summary>
        /// Удалить волну по индексу.
        /// </summary>
        public bool RemoveWave(int waveIndex)
        {
            if (waves == null || waveIndex < 0 || waveIndex >= waves.Count)
                return false;
                
            waves.RemoveAt(waveIndex);
            
            // Обновляем номера волн
            for (int i = 0; i < waves.Count; i++)
            {
                waves[i].waveNumber = i + 1;
            }
            
            return true;
        }

        /// <summary>
        /// Валидация конфигурации уровня.
        /// </summary>
        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrEmpty(levelId))
                errors.Add("Level ID cannot be empty");

            if (string.IsNullOrEmpty(levelName))
                errors.Add("Level name cannot be empty");

            if (waves == null || waves.Count == 0)
            {
                errors.Add("Level must have at least one wave");
                return false;
            }

            for (int i = 0; i < waves.Count; i++)
            {
                if (!waves[i].IsValid(out string waveError))
                {
                    errors.Add($"Wave {i + 1}: {waveError}");
                }
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// Применить автомасштабирование сложности к волнам.
        /// </summary>
        public void ApplyDifficultyScaling()
        {
            if (!useAutoScaling || waves == null) return;

            for (int i = 0; i < waves.Count; i++)
            {
                waves[i].modifiers.ApplyDifficultyScaling(i + 1, difficultyScaling);
            }
        }

        /// <summary>
        /// Создать копию конфигурации уровня.
        /// </summary>
        public LevelConfig Clone()
        {
            var clone = CreateInstance<LevelConfig>();
            clone.levelId = levelId + "_copy";
            clone.levelName = levelName + " (Copy)";
            clone.levelDescription = levelDescription;
            clone.globalWaveDelay = globalWaveDelay;
            clone.allowNextWaveEarly = allowNextWaveEarly;
            clone.earlyWaveButtonDelay = earlyWaveButtonDelay;
            clone.autoStartWaves = autoStartWaves;
            clone.initialWaveDelay = initialWaveDelay;
            clone.baseLevelGoldReward = baseLevelGoldReward;
            clone.baseLevelExperienceReward = baseLevelExperienceReward;
            clone.useAutoScaling = useAutoScaling;
            clone.difficultyScaling = difficultyScaling;
            
            clone.waves = new List<WaveConfig>();
            if (waves != null)
            {
                foreach (var wave in waves)
                {
                    clone.waves.Add(wave.Clone());
                }
            }
            
            return clone;
        }

        #endregion

        #region Unity Events

        private void OnValidate()
        {
            // Автоматически обновляем номера волн
            if (waves != null)
            {
                for (int i = 0; i < waves.Count; i++)
                {
                    if (waves[i] != null)
                    {
                        waves[i].waveNumber = i + 1;
                    }
                }
            }

            // Валидация в редакторе
            #if UNITY_EDITOR
            if (IsValid(out List<string> errors) && errors.Count > 0)
            {
                Debug.LogWarning($"Level Config '{name}' has validation errors:\n" + string.Join("\n", errors), this);
            }
            #endif
        }

        #endregion
    }
}
