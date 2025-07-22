using UnityEngine;

namespace Game.Services
{
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "Game/Configs/Balance Config")]
    public class BalanceConfig : ScriptableObject
    {
        [Header("Tower Balance")]
        public float baseTowerDamage = 25f;
        public float baseTowerRange = 5f;
        public float baseTowerAttackSpeed = 1f;
        public float towerUpgradeCostMultiplier = 1.5f;
        
        [Header("Hero Balance")]
        public float baseHeroHealth = 100f;
        public float baseHeroMana = 50f;
        public float baseHeroManaRegeneration = 1f;
        public float heroLevelUpMultiplier = 1.2f;
        
        [Header("Enemy Balance")]
        public float baseEnemyHealth = 50f;
        public float baseEnemySpeed = 2f;
        public float baseEnemyReward = 10f;
        public float enemyHealthScaling = 1.15f;
        public float enemySpeedScaling = 1.05f;
        
        [Header("Wave Balance")]
        public int baseWaveSize = 10;
        public float waveSizeScaling = 1.1f;
        public float baseWaveInterval = 30f;
        public float waveIntervalDecrease = 0.95f;
        public int maxWaves = 50;
        
        [Header("Economy Balance")]
        public int startingGold = 100;
        public float killRewardMultiplier = 1f;
        public float waveCompletionBonus = 50f;
        public float interestRate = 0.1f;
        
        [Header("Magic Balance")]
        public float baseSpellDamage = 40f;
        public float baseSpellCooldown = 5f;
        public float baseSpellManaCost = 20f;
        public float spellUpgradeMultiplier = 1.3f;
        
        [Header("Difficulty Settings")]
        [Range(0.5f, 2f)] public float difficultyMultiplier = 1f;
        public bool enableHardMode = false;
        public float hardModeMultiplier = 1.5f;
        
        public float GetScaledEnemyHealth(int waveNumber)
        {
            float scaled = baseEnemyHealth * Mathf.Pow(enemyHealthScaling, waveNumber - 1);
            return scaled * (enableHardMode ? hardModeMultiplier : 1f) * difficultyMultiplier;
        }
        
        public float GetScaledEnemySpeed(int waveNumber)
        {
            float scaled = baseEnemySpeed * Mathf.Pow(enemySpeedScaling, waveNumber - 1);
            return scaled * difficultyMultiplier;
        }
        
        public int GetScaledWaveSize(int waveNumber)
        {
            float scaled = baseWaveSize * Mathf.Pow(waveSizeScaling, waveNumber - 1);
            return Mathf.RoundToInt(scaled * difficultyMultiplier);
        }
        
        public float GetWaveInterval(int waveNumber)
        {
            return Mathf.Max(5f, baseWaveInterval * Mathf.Pow(waveIntervalDecrease, waveNumber - 1));
        }
        
        private void OnValidate()
        {
            baseTowerDamage = Mathf.Clamp(baseTowerDamage, 1f, 1000f);
            baseTowerRange = Mathf.Clamp(baseTowerRange, 1f, 20f);
            baseTowerAttackSpeed = Mathf.Clamp(baseTowerAttackSpeed, 0.1f, 10f);
            
            baseHeroHealth = Mathf.Clamp(baseHeroHealth, 10f, 1000f);
            baseHeroMana = Mathf.Clamp(baseHeroMana, 10f, 200f);
            
            baseEnemyHealth = Mathf.Clamp(baseEnemyHealth, 1f, 1000f);
            baseEnemySpeed = Mathf.Clamp(baseEnemySpeed, 0.5f, 10f);
            
            baseWaveSize = Mathf.Clamp(baseWaveSize, 1, 100);
            maxWaves = Mathf.Clamp(maxWaves, 1, 200);
            
            startingGold = Mathf.Clamp(startingGold, 0, 10000);
        }
    }
}