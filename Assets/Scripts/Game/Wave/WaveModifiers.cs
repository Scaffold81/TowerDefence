using UnityEngine;

namespace Game.Wave
{
    /// <summary>
    /// Глобальные модификаторы для волны врагов.
    /// </summary>
    [System.Serializable]
    public class WaveModifiers
    {
        [Header("Global Multipliers")]
        [Tooltip("Глобальный модификатор здоровья для всех врагов в волне")]
        [Range(0.1f, 10f)]
        public float globalHealthMultiplier = 1f;
        
        [Tooltip("Глобальный модификатор скорости для всех врагов в волне")]
        [Range(0.1f, 5f)]
        public float globalSpeedMultiplier = 1f;
        
        [Tooltip("Глобальный модификатор урона для всех врагов в волне")]
        [Range(0.1f, 10f)]
        public float globalDamageMultiplier = 1f;

        [Header("Special Wave Effects")]
        [Tooltip("Все враги в волне имеют щиты")]
        public bool allHaveShields = false;
        
        [Tooltip("Все враги в волне невидимы")]
        public bool allInvisible = false;
        
        [Tooltip("Все враги в волне регенерируют здоровье")]
        public bool allHaveRegeneration = false;
        
        [Tooltip("Волна имеет ускоренный спавн")]
        public bool hasAcceleratedSpawn = false;

        [Header("Environmental Effects")]
        [Tooltip("Волна происходит во время тумана (снижает дальность башен)")]
        public bool fogOfWar = false;
        
        [Tooltip("Волна происходит во время магической бури (усиливает врагов)")]
        public bool magicalStorm = false;
        
        [Tooltip("Волна происходит ночью (влияет на видимость)")]
        public bool nightTime = false;

        [Header("Advanced Modifiers")]
        [Tooltip("Модификатор сопротивления к магии")]
        [Range(0f, 0.9f)]
        public float magicResistance = 0f;
        
        [Tooltip("Модификатор сопротивления к физическому урону")]
        [Range(0f, 0.9f)]
        public float physicalResistance = 0f;
        
        [Tooltip("Бонус к золоту за убийство врагов этой волны")]
        [Range(0f, 5f)]
        public float goldBonusMultiplier = 1f;

        /// <summary>
        /// Проверить, есть ли какие-либо особые эффекты.
        /// </summary>
        public bool HasSpecialEffects()
        {
            return allHaveShields || allInvisible || allHaveRegeneration || 
                   hasAcceleratedSpawn || fogOfWar || magicalStorm || nightTime ||
                   magicResistance > 0f || physicalResistance > 0f;
        }

        /// <summary>
        /// Получить итоговый модификатор скорости спавна.
        /// </summary>
        public float GetSpawnSpeedMultiplier()
        {
            float multiplier = 1f;
            
            if (hasAcceleratedSpawn)
                multiplier *= 0.5f; // Ускорение спавна в 2 раза
                
            if (magicalStorm)
                multiplier *= 0.8f; // Магическая буря немного ускоряет спавн
                
            return multiplier;
        }

        /// <summary>
        /// Получить модификатор видимости для башен.
        /// </summary>
        public float GetVisionRangeMultiplier()
        {
            float multiplier = 1f;
            
            if (fogOfWar)
                multiplier *= 0.7f; // Туман снижает дальность на 30%
                
            if (nightTime)
                multiplier *= 0.85f; // Ночь снижает дальность на 15%
                
            return multiplier;
        }

        /// <summary>
        /// Создать копию модификаторов волны.
        /// </summary>
        public WaveModifiers Clone()
        {
            return new WaveModifiers
            {
                globalHealthMultiplier = globalHealthMultiplier,
                globalSpeedMultiplier = globalSpeedMultiplier,
                globalDamageMultiplier = globalDamageMultiplier,
                allHaveShields = allHaveShields,
                allInvisible = allInvisible,
                allHaveRegeneration = allHaveRegeneration,
                hasAcceleratedSpawn = hasAcceleratedSpawn,
                fogOfWar = fogOfWar,
                magicalStorm = magicalStorm,
                nightTime = nightTime,
                magicResistance = magicResistance,
                physicalResistance = physicalResistance,
                goldBonusMultiplier = goldBonusMultiplier
            };
        }

        /// <summary>
        /// Сбросить все модификаторы к значениям по умолчанию.
        /// </summary>
        public void Reset()
        {
            globalHealthMultiplier = 1f;
            globalSpeedMultiplier = 1f;
            globalDamageMultiplier = 1f;
            allHaveShields = false;
            allInvisible = false;
            allHaveRegeneration = false;
            hasAcceleratedSpawn = false;
            fogOfWar = false;
            magicalStorm = false;
            nightTime = false;
            magicResistance = 0f;
            physicalResistance = 0f;
            goldBonusMultiplier = 1f;
        }

        /// <summary>
        /// Применить модификаторы сложности (для автоматического масштабирования).
        /// </summary>
        public void ApplyDifficultyScaling(int waveNumber, float difficultyMultiplier = 1.1f)
        {
            float scalingFactor = Mathf.Pow(difficultyMultiplier, waveNumber - 1);
            
            globalHealthMultiplier *= scalingFactor;
            globalDamageMultiplier *= Mathf.Sqrt(scalingFactor); // Урон растет медленнее здоровья
            
            // Добавляем сопротивления на поздних волнах
            if (waveNumber > 5)
            {
                magicResistance = Mathf.Min(0.3f, (waveNumber - 5) * 0.05f);
                physicalResistance = Mathf.Min(0.2f, (waveNumber - 5) * 0.03f);
            }
        }
    }
}
