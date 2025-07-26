using System.Collections.Generic;
using UnityEngine;
using Game.Enemy.Components;

namespace Game.Enemy.Configs
{
    /// <summary>
    /// Конфигурация игровой логики врага
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/Configs/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public EnemyType enemyType;
        public EnemyCategory category;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Stats")]
        public float maxHealth = 100f;
        public float movementSpeed = 3f;
        
        [Header("Resistances")]
        public List<ResistanceComponent.ResistanceData> resistances = new();
        
        [Header("Abilities")]
        public List<AbilityComponent.AbilityData> abilities = new();
        
        [Header("Rewards")]
        public int experienceReward = 10;
        public int goldReward = 5;
        
        private void OnValidate()
        {
            // Валидация значений
            maxHealth = Mathf.Max(1f, maxHealth);
            movementSpeed = Mathf.Max(0.1f, movementSpeed);
            experienceReward = Mathf.Max(0, experienceReward);
            goldReward = Mathf.Max(0, goldReward);
        }
    }
}