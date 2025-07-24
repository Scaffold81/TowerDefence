using UnityEngine;
using Game.Path;
using System.Linq;

namespace Test
{
    /// <summary>
    /// Простой тест для проверки компиляции системы путей
    /// </summary>
    public class PathSystemTest : MonoBehaviour
    {
        [Header("Test Components")]
        public LevelMap levelMap;
        
        private void Start()
        {
            if (levelMap != null)
            {
                Debug.Log($"LevelMap found: {levelMap.LevelName} with {levelMap.Waypoints.Count} waypoints");
                
                // Проверяем валидацию
                bool isValid = levelMap.ValidateLevel();
                Debug.Log($"Level validation: {(isValid ? "PASSED" : "FAILED")}");
                
                // Показываем статистику
                Debug.Log($"SpawnPoint: {(levelMap.SpawnPoint != null ? "✓" : "✗")}");
                Debug.Log($"EndPoint: {(levelMap.EndPoint != null ? "✓" : "✗")}");
                Debug.Log($"Intermediate waypoints: {levelMap.IntermediateWaypoints.Count()}");
            }
            else
            {
                Debug.Log("No LevelMap assigned to test script");
            }
        }
        
        [ContextMenu("Validate Level")]
        public void ValidateLevel()
        {
            if (levelMap != null)
            {
                bool isValid = levelMap.ValidateLevel();
                Debug.Log($"Manual validation result: {(isValid ? "PASSED" : "FAILED")}");
                
                if (levelMap.ValidationErrors.Count > 0)
                {
                    Debug.LogWarning($"Validation errors: {string.Join(", ", levelMap.ValidationErrors)}");
                }
            }
        }
        
        [ContextMenu("Snap All Waypoints to Terrain")]
        public void SnapToTerrain()
        {
            if (levelMap != null)
            {
                levelMap.SnapAllWaypointsToTerrain();
                Debug.Log("All waypoints snapped to terrain");
            }
        }
    }
}