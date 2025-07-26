using Game.Enemy;
using UnityEngine;

namespace Game.Configs.Enemy
{
    /// <summary>
    /// Визуальная конфигурация врага
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyVisualConfig", menuName = "Game/Configs/Enemy Visual Config")]
    public class EnemyVisualConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public EnemyType enemyType;
        
        [Header("Prefab")]
        public GameObject enemyPrefab;
        
        [Header("Animations")]
        public AnimationClip idleAnimation;
        public AnimationClip walkAnimation;
        public AnimationClip attackAnimation;
        public AnimationClip deathAnimation;
        public AnimationClip takeDamageAnimation;
        
        [Header("Materials")]
        public Material[] materials;
        
        [Header("Audio")]
        public AudioClip[] movementSounds;
        public AudioClip[] abilitySounds;
        public AudioClip[] damageSounds;
        public AudioClip[] deathSounds;
        
        [Header("Particle Effects")]
        public ParticleSystem spawnEffect;
        public ParticleSystem deathEffect;
        public ParticleSystem[] abilityEffects;
        
        [Header("UI")]
        public Sprite enemyIcon;
        public Color healthBarColor = Color.red;
        
        private void OnValidate()
        {
            // Проверяем, что prefab содержит компонент Enemy
            if (enemyPrefab != null && enemyPrefab.GetComponent<Game.Enemy.Enemy>() == null)
            {
                Debug.LogWarning($"Enemy prefab {enemyPrefab.name} does not contain Enemy component!");
            }
        }
    }
}