using UnityEngine;
using UnityEditor;
using Game.Path;

namespace Editor.Spline
{
    /// <summary>
    /// Handles automatic spline rebaking when waypoints change
    /// Monitors LevelMap changes and triggers rebaking as needed
    /// </summary>
    [InitializeOnLoad]
    public static class SplineAutoBaker
    {
        private const float AUTO_BAKE_DELAY = 1f; // Delay in seconds before auto-baking
        private static double lastChangeTime = 0;
        private static LevelMap pendingLevelMap = null;
        
        /// <summary>
        /// Static constructor - sets up editor callbacks
        /// </summary>
        static SplineAutoBaker()
        {
            EditorApplication.update += OnEditorUpdate;
            
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
#else
            EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
#endif
            
            // Monitor transform changes
            Selection.selectionChanged += OnSelectionChanged;
        }
        
        /// <summary>
        /// Called every editor frame - handles delayed auto-baking
        /// </summary>
        private static void OnEditorUpdate()
        {
            if (pendingLevelMap == null) return;
            if (EditorApplication.timeSinceStartup - lastChangeTime < AUTO_BAKE_DELAY) return;
            
            // Perform auto-baking
            if (pendingLevelMap != null && SplineBaker.GetAutoBakeEnabled(pendingLevelMap))
            {
                Debug.Log($"[SplineAutoBaker] Auto-baking spline for: {pendingLevelMap.name}");
                SplineBaker.BakeSpline(pendingLevelMap);
            }
            
            pendingLevelMap = null;
        }
        
        /// <summary>
        /// Called when hierarchy changes (objects added/removed/moved)
        /// </summary>
        private static void OnHierarchyChanged()
        {
            if (Application.isPlaying) return;
            
            // Find all LevelMaps that might need rebaking
            var levelMaps = Object.FindObjectsByType<LevelMap>(FindObjectsSortMode.None);
            foreach (var levelMap in levelMaps)
            {
                if (SplineBaker.GetAutoBakeEnabled(levelMap) && levelMap.NeedsSplineRebaking())
                {
                    ScheduleAutoBake(levelMap);
                    break; // Only bake one at a time to avoid performance issues
                }
            }
        }
        
        /// <summary>
        /// Called when selection changes - monitors waypoint transforms
        /// </summary>
        private static void OnSelectionChanged()
        {
            if (Application.isPlaying) return;
            
            // Check if selected object is a waypoint
            var selectedWaypoint = Selection.activeGameObject?.GetComponent<Waypoint>();
            if (selectedWaypoint != null)
            {
                var levelMap = selectedWaypoint.GetComponentInParent<LevelMap>();
                if (levelMap != null && SplineBaker.GetAutoBakeEnabled(levelMap))
                {
                    // Monitor this waypoint for transform changes
                    MonitorWaypointTransform(selectedWaypoint, levelMap);
                }
            }
        }
        
        /// <summary>
        /// Schedules auto-baking for the specified LevelMap
        /// </summary>
        public static void ScheduleAutoBake(LevelMap levelMap)
        {
            if (levelMap == null) return;
            
            pendingLevelMap = levelMap;
            lastChangeTime = EditorApplication.timeSinceStartup;
            
            Debug.Log($"[SplineAutoBaker] Scheduled auto-bake for: {levelMap.name}");
        }
        
        /// <summary>
        /// Monitors a waypoint's transform for changes
        /// </summary>
        private static void MonitorWaypointTransform(Waypoint waypoint, LevelMap levelMap)
        {
            if (waypoint == null || levelMap == null) return;
            
            var transform = waypoint.transform;
            var initialPosition = transform.position;
            var initialRotation = transform.rotation;
            
            // Use EditorApplication.update to monitor changes
            EditorApplication.CallbackFunction updateCallback = null;
            updateCallback = () =>
            {
                if (waypoint == null || levelMap == null)
                {
                    EditorApplication.update -= updateCallback;
                    return;
                }
                
                // Check if transform changed
                if (Vector3.Distance(transform.position, initialPosition) > 0.01f ||
                    Quaternion.Angle(transform.rotation, initialRotation) > 0.1f)
                {
                    ScheduleAutoBake(levelMap);
                    EditorApplication.update -= updateCallback;
                }
                
                // Stop monitoring if waypoint is no longer selected
                if (Selection.activeGameObject != waypoint.gameObject)
                {
                    EditorApplication.update -= updateCallback;
                }
            };
            
            EditorApplication.update += updateCallback;
        }
        
        /// <summary>
        /// Forces immediate auto-baking for all enabled LevelMaps
        /// </summary>
        [MenuItem("Tools/Splines/Force Auto-Bake All")]
        public static void ForceAutoBakeAll()
        {
            var levelMaps = Object.FindObjectsByType<LevelMap>(FindObjectsSortMode.None);
            int bakedCount = 0;
            
            foreach (var levelMap in levelMaps)
            {
                if (SplineBaker.GetAutoBakeEnabled(levelMap))
                {
                    if (SplineBaker.BakeSpline(levelMap))
                        bakedCount++;
                }
            }
            
            if (bakedCount > 0)
            {
                EditorUtility.DisplayDialog("Force Auto-Bake", 
                    $"Force-baked {bakedCount} splines", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Force Auto-Bake", 
                    "No LevelMaps have auto-baking enabled", "OK");
            }
        }
        
        /// <summary>
        /// Enables auto-baking for all LevelMaps in scene
        /// </summary>
        [MenuItem("Tools/Splines/Enable Auto-Bake for All")]
        public static void EnableAutoBakeForAll()
        {
            var levelMaps = Object.FindObjectsByType<LevelMap>(FindObjectsSortMode.None);
            
            foreach (var levelMap in levelMaps)
            {
                SplineBaker.SetAutoBakeEnabled(levelMap, true);
            }
            
            EditorUtility.DisplayDialog("Auto-Bake Settings", 
                $"Enabled auto-baking for {levelMaps.Length} LevelMaps", "OK");
        }
        
        /// <summary>
        /// Disables auto-baking for all LevelMaps in scene
        /// </summary>
        [MenuItem("Tools/Splines/Disable Auto-Bake for All")]
        public static void DisableAutoBakeForAll()
        {
            var levelMaps = Object.FindObjectsByType<LevelMap>(FindObjectsSortMode.None);
            
            foreach (var levelMap in levelMaps)
            {
                SplineBaker.SetAutoBakeEnabled(levelMap, false);
            }
            
            EditorUtility.DisplayDialog("Auto-Bake Settings", 
                $"Disabled auto-baking for {levelMaps.Length} LevelMaps", "OK");
        }
    }
}