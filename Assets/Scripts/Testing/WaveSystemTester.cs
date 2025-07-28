using UnityEngine;
using Zenject;
using Game.Services;
using Game.Enemy.Services;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Game.Testing
{
    /// <summary>
    /// Тестировщик системы волн для демонстрации работы.
    /// </summary>
    public class WaveSystemTester : MonoBehaviour
    {
        [Inject] private IWaveService waveService;
        [Inject] private IEnemyService enemyService;
        
        [Header("Testing Controls")]
        [SerializeField] private bool startWavesOnStart = true;
        [SerializeField] private bool showDebugInfo = true;
        
        // Переменные для скролла
        private Vector2 scrollPosition = Vector2.zero;
        private bool useScrollView = true;
        
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
            // Обработка горячих клавиш
#if ENABLE_INPUT_SYSTEM
            // Новая Input System
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    StartTestWaves();
                }
                else if (Keyboard.current.nKey.wasPressedThisFrame)
                {
                    SpawnNextWave();
                }
                else if (Keyboard.current.sKey.wasPressedThisFrame)
                {
                    StopWaves();
                }
                else if (Keyboard.current.iKey.wasPressedThisFrame)
                {
                    ShowWaveInfoManual();
                }
            }
#else
            // Старая Input Manager
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartTestWaves();
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                SpawnNextWave();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                StopWaves();
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                ShowWaveInfoManual();
            }
#endif
            
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
            
            // Определяем размеры окна
            float windowWidth = 450f;
            float windowHeight = Screen.height - 100f;
            Rect windowRect = new Rect(10, 10, windowWidth, windowHeight);
            
            GUILayout.BeginArea(windowRect);
            GUILayout.BeginVertical(GUI.skin.box);
            
            // Заголовок и кнопка скролла
            GUILayout.BeginHorizontal();
            GUILayout.Label("Wave System Debug Panel", GetBoldLabelStyle());
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(useScrollView ? "Disable Scroll" : "Enable Scroll", GUILayout.Width(100)))
            {
                useScrollView = !useScrollView;
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Область скролла
            if (useScrollView)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(windowHeight - 80));
            }
            
            DrawDebugContent();
            
            if (useScrollView)
            {
                GUILayout.EndScrollView();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// Отрисовка основного контента
        /// </summary>
        private void DrawDebugContent()
        {
            
            // Основная информация о волнах
            GUILayout.Label("=== WAVE STATUS ===", GetBoldLabelStyle());
            GUILayout.Label($"Current Wave: {waveService.GetCurrentWaveNumber()}/{waveService.GetTotalWaveCount()}");
            GUILayout.Label($"Wave Active: {waveService.IsWaveActive()}");
            GUILayout.Label($"Can Trigger Early: {waveService.CanTriggerNextWaveEarly()}");
            
            var currentWave = waveService.GetCurrentWaveConfig();
            if (currentWave != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"=== CURRENT WAVE {currentWave.waveNumber} ===", GetBoldLabelStyle());
                GUILayout.Label($"Enemy Groups: {currentWave.enemyGroups.Count}");
                GUILayout.Label($"Total Enemies: {currentWave.GetTotalEnemyCount()}");
                GUILayout.Label($"Gold Reward: {currentWave.goldReward}");
                GUILayout.Label($"Experience Reward: {currentWave.experienceReward}");
                
                // Детали групп врагов
                if (currentWave.enemyGroups != null && currentWave.enemyGroups.Count > 0)
                {
                    GUILayout.Space(5);
                    GUILayout.Label("--- Enemy Groups ---", GetMiniBoldLabelStyle());
                    
                    for (int i = 0; i < currentWave.enemyGroups.Count; i++)
                    {
                        var group = currentWave.enemyGroups[i];
                        if (group != null)
                        {
                            GUILayout.Label($"  {i+1}. {group.enemyType} x{group.count}");
                            GUILayout.Label($"     Spawn: {group.spawnDelay:F1}s delay");
                            GUILayout.Label($"     Modifiers: HP x{group.healthMultiplier:F1}, Speed x{group.speedMultiplier:F1}");
                        }
                    }
                }
                
                // Модификаторы волны (пока не реализовано)
                // TODO: Добавить модификаторы в WaveConfig
                /*
                if (currentWave.waveModifiers != null)
                {
                    var mods = currentWave.waveModifiers;
                    GUILayout.Space(5);
                    GUILayout.Label("--- Wave Modifiers ---", GetMiniBoldLabelStyle());
                    GUILayout.Label($"Global HP: x{mods.globalHealthMultiplier:F1}");
                    GUILayout.Label($"Global Speed: x{mods.globalSpeedMultiplier:F1}");
                    GUILayout.Label($"Global Damage: x{mods.globalDamageMultiplier:F1}");
                    
                    if (mods.hasShield) GUILayout.Label("  + Shield Protection");
                    if (mods.isInvisible) GUILayout.Label("  + Invisibility");
                    if (mods.hasRegeneration) GUILayout.Label("  + Regeneration");
                    if (mods.isFogOfWar) GUILayout.Label("  + Fog of War");
                    if (mods.isMagicalStorm) GUILayout.Label("  + Magical Storm");
                    if (mods.isNightBattle) GUILayout.Label("  + Night Battle");
                }
                */
            }
            
            // Информация об EnemyService
            GUILayout.Space(10);
            GUILayout.Label("=== ENEMY SERVICE ===", GetBoldLabelStyle());
            
            if (enemyService != null)
            {
                try
                {
                    int aliveCount = enemyService.GetAliveEnemiesCount();
                    var aliveEnemies = enemyService.GetAliveEnemies();
                    
                    GUILayout.Label($"Alive Enemies: {aliveCount}");
                    
                    if (aliveEnemies.Length > 0)
                    {
                        GUILayout.Label("--- Active Enemies ---", GetMiniBoldLabelStyle());
                        for (int i = 0; i < Mathf.Min(aliveEnemies.Length, 5); i++) // Показываем только первых 5
                        {
                            var enemy = aliveEnemies[i];
                            if (enemy != null)
                            {
                                string status = enemy.IsAliveSafe() ? "Alive" : "Dead";
                                GUILayout.Label($"  {i+1}. {enemy.EnemyType} - {status}");
                            }
                        }
                        
                        if (aliveEnemies.Length > 5)
                        {
                            GUILayout.Label($"  ... and {aliveEnemies.Length - 5} more");
                        }
                    }
                    else
                    {
                        GUILayout.Label("No active enemies");
                    }
                }
                catch (System.Exception ex)
                {
                    GUILayout.Label($"Error accessing EnemyService: {ex.Message}");
                }
            }
            else
            {
                GUILayout.Label("EnemyService: Not available");
            }
            
            GUILayout.Space(15);
            GUILayout.Label("=== CONTROLS ===", GetBoldLabelStyle());
            
            // Кнопки управления в несколько рядов
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Waves", GUILayout.Width(100)))
            {
                StartTestWaves();
            }
            
            if (GUILayout.Button("Stop Waves", GUILayout.Width(100)))
            {
                StopWaves();
            }
            
            if (GUILayout.Button("Next Wave", GUILayout.Width(100)))
            {
                SpawnNextWave();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Show Info", GUILayout.Width(100)))
            {
                ShowWaveInfoManual();
            }
            
#if UNITY_EDITOR
            if (GUILayout.Button("Clear Log", GUILayout.Width(100)))
            {
                // Очищаем консоль через рефлексию
                var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.SceneView));
                var type = assembly.GetType("UnityEditor.LogEntries");
                var method = type.GetMethod("Clear");
                method.Invoke(new object(), null);
            }
#endif
            GUILayout.EndHorizontal();
            
            // Время и производительность
            GUILayout.Space(10);
            GUILayout.Label("=== SYSTEM INFO ===", GetBoldLabelStyle());
            GUILayout.Label($"Time: {Time.time:F1}s");
            GUILayout.Label($"FPS: {(1.0f / Time.unscaledDeltaTime):F0}");
            GUILayout.Label($"Frame: {Time.frameCount}");
        }
        
        /// <summary>
        /// Получить стиль для жирного текста
        /// </summary>
        private GUIStyle GetBoldLabelStyle()
        {
#if UNITY_EDITOR
            return EditorStyles.boldLabel;
#else
            var style = new GUIStyle(GUI.skin.label);
            style.fontStyle = FontStyle.Bold;
            return style;
#endif
        }
        
        /// <summary>
        /// Получить стиль для маленького жирного текста
        /// </summary>
        private GUIStyle GetMiniBoldLabelStyle()
        {
#if UNITY_EDITOR
            return EditorStyles.miniBoldLabel;
#else
            var style = new GUIStyle(GUI.skin.label);
            style.fontStyle = FontStyle.Bold;
            style.fontSize = Mathf.Max(8, GUI.skin.label.fontSize - 2);
            return style;
#endif
        }
    }
}
