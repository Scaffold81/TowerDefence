using UnityEngine;
using Game.Enemy;

namespace Game.Wave
{
    /// <summary>
    /// Конфигурация группы врагов в волне.
    /// </summary>
    [System.Serializable]
    public class EnemyGroupConfig
    {
        [Header("Enemy Type")]
        [Tooltip("Тип врага для спавна")]
        public EnemyType enemyType = EnemyType.Animal;
        
        [Tooltip("Количество врагов этого типа в группе")]
        [Range(1, 100)]
        public int count = 5;

        [Header("Spawn Settings")]
        [Tooltip("Задержка перед спавном этой группы (в секундах)")]
        [Range(0f, 10f)]
        public float spawnDelay = 0f;
        
        [Tooltip("Интервал между спавном врагов в группе (в секундах)")]
        [Range(0.1f, 3f)]
        public float intervalBetweenEnemies = 0.5f;

        [Header("Enemy Modifications")]
        [Tooltip("Модификатор здоровья врагов (1.0 = обычное здоровье)")]
        [Range(0.1f, 10f)]
        public float healthMultiplier = 1f;
        
        [Tooltip("Модификатор скорости врагов (1.0 = обычная скорость)")]
        [Range(0.1f, 5f)]
        public float speedMultiplier = 1f;
        
        [Tooltip("Модификатор урона врагов (1.0 = обычный урон)")]
        [Range(0.1f, 10f)]
        public float damageMultiplier = 1f;

        [Header("Special Properties")]
        [Tooltip("Особые свойства для этой группы врагов")]
        public EnemyGroupSpecialProperties specialProperties = new EnemyGroupSpecialProperties();

        /// <summary>
        /// Получить время, необходимое для спавна всей группы.
        /// </summary>
        public float GetGroupSpawnDuration()
        {
            return spawnDelay + (count - 1) * intervalBetweenEnemies;
        }

        /// <summary>
        /// Валидация конфигурации группы.
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (count <= 0)
            {
                errorMessage = "Enemy count must be greater than 0";
                return false;
            }

            if (intervalBetweenEnemies <= 0)
            {
                errorMessage = "Interval between enemies must be greater than 0";
                return false;
            }

            if (healthMultiplier <= 0)
            {
                errorMessage = "Health multiplier must be greater than 0";
                return false;
            }

            if (speedMultiplier <= 0)
            {
                errorMessage = "Speed multiplier must be greater than 0";
                return false;
            }

            if (damageMultiplier < 0)
            {
                errorMessage = "Damage multiplier cannot be negative";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Создать копию конфигурации группы.
        /// </summary>
        public EnemyGroupConfig Clone()
        {
            return new EnemyGroupConfig
            {
                enemyType = enemyType,
                count = count,
                spawnDelay = spawnDelay,
                intervalBetweenEnemies = intervalBetweenEnemies,
                healthMultiplier = healthMultiplier,
                speedMultiplier = speedMultiplier,
                damageMultiplier = damageMultiplier,
                specialProperties = specialProperties.Clone()
            };
        }

        /// <summary>
        /// Получить итоговые характеристики врага с учетом модификаторов группы и волны.
        /// </summary>
        public EnemyStats GetModifiedEnemyStats(EnemyStats baseStats, WaveModifiers waveModifiers)
        {
            return new EnemyStats
            {
                health = baseStats.health * healthMultiplier * waveModifiers.globalHealthMultiplier,
                speed = baseStats.speed * speedMultiplier * waveModifiers.globalSpeedMultiplier,
                damage = baseStats.damage * damageMultiplier * waveModifiers.globalDamageMultiplier
            };
        }
    }

    /// <summary>
    /// Особые свойства группы врагов.
    /// </summary>
    [System.Serializable]
    public class EnemyGroupSpecialProperties
    {
        [Tooltip("Группа имеет щиты")]
        public bool hasShield = false;
        
        [Tooltip("Группа невидима до первой атаки")]
        public bool isInvisible = false;
        
        [Tooltip("Группа регенерирует здоровье")]
        public bool hasRegeneration = false;
        
        [Tooltip("Группа имеет иммунитет к определенным типам урона")]
        public bool hasImmunities = false;

        /// <summary>
        /// Создать копию особых свойств.
        /// </summary>
        public EnemyGroupSpecialProperties Clone()
        {
            return new EnemyGroupSpecialProperties
            {
                hasShield = hasShield,
                isInvisible = isInvisible,
                hasRegeneration = hasRegeneration,
                hasImmunities = hasImmunities
            };
        }
    }

    /// <summary>
    /// Базовые характеристики врага для расчетов.
    /// </summary>
    [System.Serializable]
    public class EnemyStats
    {
        public float health;
        public float speed;
        public float damage;
    }
}
