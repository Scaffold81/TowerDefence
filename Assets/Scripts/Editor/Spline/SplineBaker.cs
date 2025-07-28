using UnityEngine;
using UnityEditor;
using Core.Services.Spline;
using Game.Path;
using System.IO;

namespace Editor.Spline
{
    /// <summary>
    /// Editor utility for baking splines from LevelMap waypoints
    /// Provides both automatic and manual spline baking functionality
    /// </summary>
    public static class SplineBaker
    {
        private const string AUTO_BAKE_PREF_KEY = "SplineBaker_AutoBake_";
        private static readonly SplineSystem splineSystem = new SplineSystem();
        
        /// <summary>
        /// Bakes spline data for the specified LevelMap
        /// </summary>
        [MenuItem("Tools/Splines/Bake Selected Level")]
        public static void BakeSelectedLevel()
        {
            var selectedLevelMap = Selection.activeGameObject?.GetComponent<LevelMap>();
            if (selectedLevelMap == null)
            {
                EditorUtility.DisplayDialog("Spline Baker", 
                    "Please select a GameObject with LevelMap component", "OK");
                return;
            }
            
            BakeSpline(selectedLevelMap);
        }
        
        /// <summary>
        /// Bakes splines for all LevelMaps in the current scene
        /// </summary>
        [MenuItem("Tools/Splines/Bake All Levels in Scene")]
        public static void BakeAllLevelsInScene()
        {
            var levelMaps = Object.FindObjectsByType<LevelMap>(FindObjectsSortMode.None);
            if (levelMaps.Length == 0)
            {
                EditorUtility.DisplayDialog("Spline Baker", 
                    "No LevelMaps found in the current scene", "OK");
                return;
            }
            
            int bakedCount = 0;
            for (int i = 0; i < levelMaps.Length; i++)
            {
                string title = $"Baking Splines ({i + 1}/{levelMaps.Length})";
                string info = $"Processing: {levelMaps[i].name}";
                float progress = (float)i / levelMaps.Length;
                
                EditorUtility.DisplayProgressBar(title, info, progress);
                
                if (BakeSpline(levelMaps[i]))
                    bakedCount++;
            }
            
            EditorUtility.ClearProgressBar();
            
            EditorUtility.DisplayDialog("Spline Baker", 
                $"Successfully baked {bakedCount}/{levelMaps.Length} splines", "OK");
        }
        
        /// <summary>
        /// Clears baked spline data for selected LevelMap
        /// </summary>
        [MenuItem("Tools/Splines/Clear Selected Level Spline")]
        public static void ClearSelectedLevelSpline()
        {
            var selectedLevelMap = Selection.activeGameObject?.GetComponent<LevelMap>();
            if (selectedLevelMap == null)
            {
                EditorUtility.DisplayDialog("Spline Baker", 
                    "Please select a GameObject with LevelMap component", "OK");
                return;
            }
            
            ClearBakedData(selectedLevelMap);
        }
        
        /// <summary>
        /// Exports spline data to JSON file for external use
        /// </summary>
        [MenuItem("Tools/Splines/Export Selected Level Spline")]
        public static void ExportSelectedLevelSpline()
        {
            var selectedLevelMap = Selection.activeGameObject?.GetComponent<LevelMap>();
            if (selectedLevelMap == null)
            {
                EditorUtility.DisplayDialog("Spline Baker", 
                    "Please select a GameObject with LevelMap component", "OK");
                return;
            }
            
            ExportSplineData(selectedLevelMap);
        }
        
        /// <summary>
        /// Main spline baking method
        /// </summary>
        public static bool BakeSpline(LevelMap levelMap)
        {
            if (levelMap == null)
            {
                Debug.LogError("[SplineBaker] LevelMap is null");
                return false;
            }
            
            try
            {
                Debug.Log($"[SplineBaker] Starting bake for level: {levelMap.name}");
                
                // Validate waypoints
                var waypoints = levelMap.GetWaypointPositions();
                if (waypoints.Length < 2)
                {
                    Debug.LogError($"[SplineBaker] Level '{levelMap.name}' needs at least 2 waypoints, got {waypoints.Length}");
                    return false;
                }
                
                // Generate spline data
                var settings = levelMap.SplineSettings;
                var bakedData = splineSystem.GenerateSplineData(waypoints, settings);
                
                if (!bakedData.isValid)
                {
                    Debug.LogError($"[SplineBaker] Failed to generate valid spline data for level: {levelMap.name}");
                    return false;
                }
                
                // Save baked data to LevelMap
                levelMap.SetBakedSplineData(bakedData);
                
                // Mark scene as dirty
                EditorUtility.SetDirty(levelMap);
                
                Debug.Log($"[SplineBaker] ✅ Successfully baked spline for '{levelMap.name}': " +
                         $"{bakedData.referencePoints.Length} points, " +
                         $"{bakedData.totalLength:F1}m length, " +
                         $"{bakedData.designerMarkers.Length} markers");
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SplineBaker] Error baking spline for '{levelMap.name}': {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Clears baked spline data from LevelMap
        /// </summary>
        public static void ClearBakedData(LevelMap levelMap)
        {
            if (levelMap == null) return;
            
            levelMap.InvalidateSplineData();
            EditorUtility.SetDirty(levelMap);
            
            Debug.Log($"[SplineBaker] Cleared baked spline data for: {levelMap.name}");
        }
        
        /// <summary>
        /// Exports spline data to JSON file
        /// </summary>
        public static void ExportSplineData(LevelMap levelMap)
        {
            if (levelMap == null) return;
            
            var bakedData = levelMap.GetBakedSplineData();
            if (bakedData == null)
            {
                EditorUtility.DisplayDialog("Export Spline", 
                    "No baked spline data found. Please bake the spline first.", "OK");
                return;
            }
            
            // Choose export path
            string defaultName = $"{levelMap.name}_SplineData.json";
            string path = EditorUtility.SaveFilePanel("Export Spline Data", 
                Application.dataPath, defaultName, "json");
            
            if (string.IsNullOrEmpty(path)) return;
            
            try
            {
                // Create export data structure
                var exportData = new SplineExportData
                {
                    levelName = levelMap.name,
                    totalLength = bakedData.totalLength,
                    pointCount = bakedData.referencePoints.Length,
                    markerCount = bakedData.designerMarkers.Length,
                    exportTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    points = bakedData.referencePoints,
                    markers = bakedData.designerMarkers
                };
                
                string json = JsonUtility.ToJson(exportData, true);
                File.WriteAllText(path, json);
                
                Debug.Log($"[SplineBaker] Exported spline data to: {path}");
                EditorUtility.DisplayDialog("Export Spline", 
                    $"Successfully exported spline data to:\n{path}", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SplineBaker] Error exporting spline data: {ex.Message}");
                EditorUtility.DisplayDialog("Export Error", 
                    $"Failed to export spline data:\n{ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Auto-baking functionality
        /// </summary>
        public static bool GetAutoBakeEnabled(LevelMap levelMap)
        {
            if (levelMap == null) return false;
            string key = AUTO_BAKE_PREF_KEY + levelMap.GetInstanceID();
            return EditorPrefs.GetBool(key, false);
        }
        
        public static void SetAutoBakeEnabled(LevelMap levelMap, bool enabled)
        {
            if (levelMap == null) return;
            string key = AUTO_BAKE_PREF_KEY + levelMap.GetInstanceID();
            EditorPrefs.SetBool(key, enabled);
        }
        
        /// <summary>
        /// Checks if auto-rebaking is needed and performs it
        /// </summary>
        public static void CheckAndPerformAutoBake(LevelMap levelMap)
        {
            if (levelMap == null || !GetAutoBakeEnabled(levelMap)) return;
            if (!levelMap.NeedsSplineRebaking()) return;
            
            Debug.Log($"[SplineBaker] Auto-baking spline for: {levelMap.name}");
            BakeSpline(levelMap);
        }
        
        /// <summary>
        /// Validates spline data and returns user-friendly info
        /// </summary>
        public static string GetSplineValidationInfo(LevelMap levelMap)
        {
            if (levelMap == null) return "❌ No LevelMap";
            
            var waypoints = levelMap.GetWaypointPositions();
            if (waypoints.Length < 2)
                return $"❌ Need at least 2 waypoints (have {waypoints.Length})";
            
            var validation = splineSystem.ValidateWaypoints(waypoints);
            if (!validation.isValid)
                return $"❌ Validation failed: {string.Join(", ", validation.errors)}";
            
            if (validation.HasWarnings)
                return $"⚠️ Has warnings: {string.Join(", ", validation.warnings)}";
            
            return "✅ Ready for baking";
        }
        
        /// <summary>
        /// Data structure for JSON export
        /// </summary>
        [System.Serializable]
        private class SplineExportData
        {
            public string levelName;
            public float totalLength;
            public int pointCount;
            public int markerCount;
            public string exportTime;
            public SplinePoint[] points;
            public SplineMarker[] markers;
        }
    }
}