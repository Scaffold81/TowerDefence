using UnityEngine;
using Zenject;
using Game.Services;

namespace Game.Testing
{
    /// <summary>
    /// Тестировщик системы волн для демонстрации работы.
    /// </summary>
    public class WaveSystemTester : MonoBehaviour
    {
        [Inject] private IWaveService waveService;
        
        [Header("Testing Controls")]
        [SerializeField] private bool startWavesOnStart = true;
        [SerializeField] private bool showDebugInfo = true;
        
        private void Start()
        {
            if (startWavesOnStart)
            {
                // Небольшая задержка для инициализации всех сервисов
                Invoke(nameof(StartTestWaves), 1f);
            }
        }
        
        private void StartTestWaves()
        {
            Debug.Log("[WaveSystemTester] Starting wave system test...");
            
            if (waveService != null)
            {
                if (showDebugInfo)
                {
                    ShowWaveInfo();
                }
                waveService.StartWaves();
            }
            else
            {
                Debug.LogError("[WaveSystemTester] WaveService not injected!");
            }
        }
        
        private void ShowWaveInfo()
        {
            int totalWaves = waveService.GetTotalWaveCount();
            var currentWave = waveService.GetCurrentWaveConfig();
            
            Debug.Log($"[WaveSystemTester] Total waves on level: {totalWaves}");
            
            if (currentWave != null)
            {
                Debug.Log($"[WaveSystemTester] Current wave: {currentWave.waveNumber}, " +
                         $"Groups: {currentWave.enemyGroups.Count}, " +
                         $"Total enemies: {currentWave.GetTotalEnemyCount()}");
            }
        }
        
        [ContextMenu("Start Waves")]
        public void StartWavesManual()
        {
            StartTestWaves();
        }
        
        [ContextMenu("Stop Waves")]
        public void StopWaves()
        {
            Debug.Log("[WaveSystemTester] Stopping waves...");
            waveService?.StopWaves();
        }
        
        [ContextMenu("Spawn Next Wave")]
        public void SpawnNextWave()
        {
            Debug.Log("[WaveSystemTester] Manually spawning next wave...");
            waveService?.SpawnNextWave();
        }
        
        [ContextMenu("Show Wave Info")]
        public void ShowWaveInfoManual()
        {
            ShowWaveInfo();
        }
        
        private void Update()
        {
            if (showDebugInfo && waveService != null)
            {
                // Показываем информацию о текущем состоянии каждые 5 секунд
                if (Time.time % 5f < 0.1f)
                {
                    if (waveService.IsWaveActive())
                    {
                        Debug.Log($"[WaveSystemTester] Wave {waveService.GetCurrentWaveNumber()}/{waveService.GetTotalWaveCount()} is active");
                    }
                }
            }
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo || waveService == null) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical(GUI.skin.box);
            
            GUILayout.Label("Wave System Status", GUI.skin.label);
            GUILayout.Space(10);
            
            GUILayout.Label($"Current Wave: {waveService.GetCurrentWaveNumber()}/{waveService.GetTotalWaveCount()}");
            GUILayout.Label($"Wave Active: {waveService.IsWaveActive()}");
            GUILayout.Label($"Can Trigger Early: {waveService.CanTriggerNextWaveEarly()}");
            
            var currentWave = waveService.GetCurrentWaveConfig();
            if (currentWave != null)
            {
                GUILayout.Space(5);
                GUILayout.Label($"Wave {currentWave.waveNumber}:");
                GUILayout.Label($"  Groups: {currentWave.enemyGroups.Count}");
                GUILayout.Label($"  Enemies: {currentWave.GetTotalEnemyCount()}");
                GUILayout.Label($"  Rewards: {currentWave.goldReward}g, {currentWave.experienceReward}xp");
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Start Waves"))
            {
                StartTestWaves();
            }
            
            if (GUILayout.Button("Stop Waves"))
            {
                StopWaves();
            }
            
            if (GUILayout.Button("Next Wave"))
            {
                SpawnNextWave();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
