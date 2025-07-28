using UnityEngine;
using UnityEditor;
using Game.Path;
using System.Linq;
using Core.Services.Spline;
using Editor.Spline;

namespace Editor.Path
{
    [CustomEditor(typeof(LevelMap))]
    public class LevelMapEditor : UnityEditor.Editor
    {
        private LevelMap _levelMap;
        private SerializedProperty _levelNameProp;
        private SerializedProperty _terrainProp;
        private SerializedProperty _showPathProp;
        private SerializedProperty _showWaypointsProp;
        private SerializedProperty _showConnectionsProp;
        private SerializedProperty _pathColorProp;
        private SerializedProperty _pathWidthProp;
        private SerializedProperty _waypointsProp;
        private SerializedProperty _autoValidateProp;
        private SerializedProperty _showValidationErrorsProp;
        private SerializedProperty _useSplinesProp;
        private SerializedProperty _splineSettingsProp;
        
        private bool _showVisualizationSettings = true;
        private bool _showWaypointsList = true;
        private bool _showValidationSettings = true;
        private bool _showQuickActions = true;
        private bool _showSplineTools = true;
        
        private void OnEnable()
        {
            _levelMap = (LevelMap)target;
            
            _levelNameProp = serializedObject.FindProperty("_levelName");
            _terrainProp = serializedObject.FindProperty("_terrain");
            _showPathProp = serializedObject.FindProperty("_showPath");
            _showWaypointsProp = serializedObject.FindProperty("_showWaypoints");
            _showConnectionsProp = serializedObject.FindProperty("_showConnections");
            _pathColorProp = serializedObject.FindProperty("_pathColor");
            _pathWidthProp = serializedObject.FindProperty("_pathWidth");
            _waypointsProp = serializedObject.FindProperty("_waypoints");
            _autoValidateProp = serializedObject.FindProperty("_autoValidate");
            _showValidationErrorsProp = serializedObject.FindProperty("_showValidationErrors");
            _useSplinesProp = serializedObject.FindProperty("_useSplines");
            _splineSettingsProp = serializedObject.FindProperty("_splineSettings");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawLevelSettings();
            EditorGUILayout.Space();
            
            DrawVisualizationSettings();
            EditorGUILayout.Space();
            
            DrawQuickActions();
            EditorGUILayout.Space();
            
            DrawSplineTools();
            EditorGUILayout.Space();
            
            DrawWaypointsList();
            EditorGUILayout.Space();
            
            DrawValidationSettings();
            EditorGUILayout.Space();
            
            DrawValidationResults();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawSplineTools()
        {
            _showSplineTools = EditorGUILayout.Foldout(_showSplineTools, "Spline Designer Tools", true);
            
            if (_showSplineTools)
            {
                EditorGUI.indentLevel++;
                
                // Spline enable/disable
                EditorGUILayout.PropertyField(_useSplinesProp, new GUIContent("Use Splines"));
                
                if (_levelMap.UseSplines)
                {
                    // Spline settings
                    EditorGUILayout.PropertyField(_splineSettingsProp, new GUIContent("Spline Settings"), true);
                    
                    EditorGUILayout.Space();
                    
                    // Status info
                    var bakedData = _levelMap.GetBakedSplineData();
                    if (bakedData != null)
                    {
                        EditorGUILayout.HelpBox(bakedData.GetDebugInfo(), MessageType.Info);
                    }
                    else
                    {
                        string validationInfo = SplineBaker.GetSplineValidationInfo(_levelMap);
                        MessageType messageType = validationInfo.StartsWith("✅") ? MessageType.Info : 
                                                validationInfo.StartsWith("⚠️") ? MessageType.Warning : MessageType.Error;
                        EditorGUILayout.HelpBox(validationInfo, messageType);
                    }
                    
                    EditorGUILayout.Space();
                    
                    // Baking controls
                    EditorGUILayout.BeginHorizontal();
                    
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Bake Spline"))
                    {
                        SplineBaker.BakeSpline(_levelMap);
                    }
                    
                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("Export Spline"))
                    {
                        SplineBaker.ExportSplineData(_levelMap);
                    }
                    
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Clear Baked Data"))
                    {
                        SplineBaker.ClearBakedData(_levelMap);
                    }
                    
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                    
                    // Auto-bake settings
                    EditorGUILayout.Space();
                    bool autoBake = SplineBaker.GetAutoBakeEnabled(_levelMap);
                    bool newAutoBake = EditorGUILayout.Toggle("Auto-bake on changes", autoBake);
                    if (newAutoBake != autoBake)
                    {
                        SplineBaker.SetAutoBakeEnabled(_levelMap, newAutoBake);
                    }
                    
                    // Designer tools
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Designer Tools", EditorStyles.boldLabel);
                    
                    if (bakedData != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        if (GUILayout.Button("Snap Selected to Spline"))
                        {
                            SnapSelectedToSpline();
                        }
                        
                        if (GUILayout.Button("Show Markers in Scene"))
                        {
                            ShowMarkersInScene(bakedData);
                        }
                        
                        EditorGUILayout.EndHorizontal();
                        
                        // Spline statistics
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField($"Total Length: {bakedData.totalLength:F1}m");
                        EditorGUILayout.LabelField($"Reference Points: {bakedData.referencePoints?.Length ?? 0}");
                        EditorGUILayout.LabelField($"Designer Markers: {bakedData.designerMarkers?.Length ?? 0}");
                        EditorGUILayout.LabelField($"Average Curvature: {bakedData.averageCurvature:F3}");
                        EditorGUILayout.LabelField($"Sample Distance: {bakedData.sampleDistance:F2}m");
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Bake spline first to access designer tools", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Splines are disabled. Enable to access spline tools.", MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawLevelSettings()
        {
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_levelNameProp, new GUIContent("Level Name"));
            EditorGUILayout.PropertyField(_terrainProp, new GUIContent("Terrain"));
            
            // Автоматический поиск terrain если не назначен
            if (_levelMap.Terrain == null)
            {
                EditorGUILayout.HelpBox("Terrain is not assigned. Click 'Find Terrain' to automatically find it.", MessageType.Warning);
                if (GUILayout.Button("Find Terrain"))
                {
                    Terrain terrain = Object.FindFirstObjectByType<Terrain>();
                    if (terrain != null)
                    {
                        _terrainProp.objectReferenceValue = terrain;
                        EditorUtility.SetDirty(_levelMap);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Terrain Not Found", "No Terrain found in the scene.", "OK");
                    }
                }
            }
        }
        
        private void DrawVisualizationSettings()
        {
            _showVisualizationSettings = EditorGUILayout.Foldout(_showVisualizationSettings, "Visualization Settings", true);
            
            if (_showVisualizationSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(_showPathProp, new GUIContent("Show Path"));
                EditorGUILayout.PropertyField(_showWaypointsProp, new GUIContent("Show Waypoints"));
                EditorGUILayout.PropertyField(_showConnectionsProp, new GUIContent("Show Connections"));
                EditorGUILayout.PropertyField(_pathColorProp, new GUIContent("Path Color"));
                EditorGUILayout.PropertyField(_pathWidthProp, new GUIContent("Path Width"));
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Show All"))
                {
                    _showPathProp.boolValue = true;
                    _showWaypointsProp.boolValue = true;
                    _showConnectionsProp.boolValue = true;
                }
                
                if (GUILayout.Button("Hide All"))
                {
                    _showPathProp.boolValue = false;
                    _showWaypointsProp.boolValue = false;
                    _showConnectionsProp.boolValue = false;
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawQuickActions()
        {
            _showQuickActions = EditorGUILayout.Foldout(_showQuickActions, "Quick Actions", true);
            
            if (_showQuickActions)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal();
                
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Add SpawnPoint"))
                {
                    CreateWaypoint<SpawnPoint>("SpawnPoint");
                }
                
                GUI.backgroundColor = Color.blue;
                if (GUILayout.Button("Add IntermediateWaypoint"))
                {
                    CreateWaypoint<IntermediateWaypoint>("Waypoint");
                }
                
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Add EndPoint"))
                {
                    CreateWaypoint<EndPoint>("EndPoint");
                }
                
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Collect Waypoints from Scene"))
                {
                    Undo.RecordObject(_levelMap, "Collect Waypoints");
                    _levelMap.CollectWaypointsFromScene();
                    EditorUtility.SetDirty(_levelMap);
                }
                
                if (GUILayout.Button("Snap All to Terrain"))
                {
                    Undo.RecordObject(_levelMap, "Snap Waypoints to Terrain");
                    _levelMap.SnapAllWaypointsToTerrain();
                    EditorUtility.SetDirty(_levelMap);
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Refresh Indices"))
                {
                    Undo.RecordObject(_levelMap, "Refresh Waypoint Indices");
                    _levelMap.RefreshWaypointIndices();
                    EditorUtility.SetDirty(_levelMap);
                }
                
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Clear All Waypoints"))
                {
                    if (EditorUtility.DisplayDialog("Clear Waypoints", 
                        "Are you sure you want to remove all waypoints? This action cannot be undone.", 
                        "Yes", "Cancel"))
                    {
                        Undo.RecordObject(_levelMap, "Clear All Waypoints");
                        
                        // Удаляем все waypoint GameObjects
                        foreach (var waypoint in _levelMap.Waypoints.ToArray())
                        {
                            if (waypoint != null)
                            {
                                Undo.DestroyObjectImmediate(waypoint.gameObject);
                            }
                        }
                        
                        _levelMap.ClearWaypoints();
                        EditorUtility.SetDirty(_levelMap);
                    }
                }
                
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawWaypointsList()
        {
            _showWaypointsList = EditorGUILayout.Foldout(_showWaypointsList, $"Waypoints ({_levelMap.Waypoints.Count})", true);
            
            if (_showWaypointsList)
            {
                EditorGUI.indentLevel++;
                
                if (_levelMap.Waypoints.Count == 0)
                {
                    EditorGUILayout.HelpBox("No waypoints in the level. Use Quick Actions to add waypoints.", MessageType.Info);
                }
                else
                {
                    for (int i = 0; i < _levelMap.Waypoints.Count; i++)
                    {
                        DrawWaypointItem(i);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawWaypointItem(int index)
        {
            Waypoint waypoint = _levelMap.Waypoints[index];
            
            EditorGUILayout.BeginHorizontal();
            
            // Цветовая индикация типа waypoint
            Color backgroundColor = GUI.backgroundColor;
            if (waypoint is SpawnPoint) GUI.backgroundColor = Color.green;
            else if (waypoint is EndPoint) GUI.backgroundColor = Color.red;
            else if (waypoint is IntermediateWaypoint) GUI.backgroundColor = Color.blue;
            
            // Информация о waypoint
            string waypointInfo = waypoint != null ? 
                $"{index}: {waypoint.GetType().Name}" : 
                $"{index}: [Missing]";
            
            if (GUILayout.Button(waypointInfo, GUILayout.ExpandWidth(true)))
            {
                if (waypoint != null)
                {
                    Selection.activeGameObject = waypoint.gameObject;
                    EditorGUIUtility.PingObject(waypoint.gameObject);
                }
            }
            
            GUI.backgroundColor = backgroundColor;
            
            // Кнопки управления
            if (GUILayout.Button("↑", GUILayout.Width(25)) && index > 0)
            {
                Undo.RecordObject(_levelMap, "Move Waypoint Up");
                _levelMap.MoveWaypoint(index, index - 1);
                EditorUtility.SetDirty(_levelMap);
            }
            
            if (GUILayout.Button("↓", GUILayout.Width(25)) && index < _levelMap.Waypoints.Count - 1)
            {
                Undo.RecordObject(_levelMap, "Move Waypoint Down");
                _levelMap.MoveWaypoint(index, index + 1);
                EditorUtility.SetDirty(_levelMap);
            }
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                if (EditorUtility.DisplayDialog("Remove Waypoint", 
                    $"Remove waypoint {index}?", "Yes", "Cancel"))
                {
                    Undo.RecordObject(_levelMap, "Remove Waypoint");
                    
                    if (waypoint != null)
                    {
                        Undo.DestroyObjectImmediate(waypoint.gameObject);
                    }
                    
                    _levelMap.RemoveWaypoint(waypoint);
                    EditorUtility.SetDirty(_levelMap);
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawValidationSettings()
        {
            _showValidationSettings = EditorGUILayout.Foldout(_showValidationSettings, "Validation Settings", true);
            
            if (_showValidationSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(_autoValidateProp, new GUIContent("Auto Validate"));
                EditorGUILayout.PropertyField(_showValidationErrorsProp, new GUIContent("Show Validation Errors"));
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Validate Level Now"))
                {
                    _levelMap.ValidateLevel();
                    EditorUtility.SetDirty(_levelMap);
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawValidationResults()
        {
            if (_levelMap.ValidationErrors.Count > 0)
            {
                EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
                
                EditorGUILayout.HelpBox($"Found {_levelMap.ValidationErrors.Count} validation errors:", MessageType.Error);
                
                EditorGUI.indentLevel++;
                foreach (string error in _levelMap.ValidationErrors)
                {
                    EditorGUILayout.LabelField("• " + error, EditorStyles.wordWrappedLabel);
                }
                EditorGUI.indentLevel--;
            }
            else if (_levelMap.Waypoints.Count > 0)
            {
                EditorGUILayout.HelpBox("Level validation passed successfully!", MessageType.Info);
            }
        }
        
        private void CreateWaypoint<T>(string name) where T : Waypoint
        {
            // Проверяем ограничения
            if (typeof(T) == typeof(SpawnPoint) && _levelMap.SpawnPoint != null)
            {
                EditorUtility.DisplayDialog("Cannot Add SpawnPoint", 
                    "Level already has a SpawnPoint. Only one SpawnPoint is allowed per level.", "OK");
                return;
            }
            
            if (typeof(T) == typeof(EndPoint) && _levelMap.EndPoint != null)
            {
                EditorUtility.DisplayDialog("Cannot Add EndPoint", 
                    "Level already has an EndPoint. Only one EndPoint is allowed per level.", "OK");
                return;
            }
            
            // Создаем новый waypoint
            GameObject waypointGO = new GameObject(name);
            waypointGO.transform.SetParent(_levelMap.transform);
            
            // Позиционируем waypoint
            Vector3 position = _levelMap.transform.position;
            if (_levelMap.Waypoints.Count > 0)
            {
                Waypoint lastWaypoint = _levelMap.Waypoints[_levelMap.Waypoints.Count - 1];
                if (lastWaypoint != null)
                {
                    position = lastWaypoint.transform.position + Vector3.forward * 5f;
                }
            }
            waypointGO.transform.position = position;
            
            // Добавляем компонент
            T waypoint = waypointGO.AddComponent<T>();
            
            // Привязываем к terrain если он назначен
            if (_levelMap.Terrain != null)
            {
                waypoint.SnapToTerrainSurface();
            }
            
            // Регистрируем изменения для Undo
            Undo.RegisterCreatedObjectUndo(waypointGO, $"Create {name}");
            Undo.RecordObject(_levelMap, $"Add {name}");
            
            // Добавляем в карту
            _levelMap.AddWaypoint(waypoint);
            
            // Выделяем созданный waypoint
            Selection.activeGameObject = waypointGO;
            
            EditorUtility.SetDirty(_levelMap);
        }
        
        /// <summary>
        /// Snaps selected GameObjects to nearest spline points
        /// </summary>
        private void SnapSelectedToSpline()
        {
            var bakedData = _levelMap.GetBakedSplineData();
            if (bakedData?.referencePoints == null)
            {
                EditorUtility.DisplayDialog("Snap to Spline", "No baked spline data available", "OK");
                return;
            }
            
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Snap to Spline", "No objects selected", "OK");
                return;
            }
            
            Undo.RecordObjects(selectedObjects.Select(go => go.transform).ToArray(), "Snap to Spline");
            
            foreach (var obj in selectedObjects)
            {
                var nearestPoint = bakedData.GetNearestPoint(obj.transform.position);
                obj.transform.position = nearestPoint.position;
                obj.transform.rotation = nearestPoint.Rotation;
            }
            
            Debug.Log($"[LevelMapEditor] Snapped {selectedObjects.Length} objects to spline");
        }
        
        /// <summary>
        /// Creates temporary GameObjects to visualize spline markers in scene
        /// </summary>
        private void ShowMarkersInScene(BakedSplineData bakedData)
        {
            if (bakedData?.designerMarkers == null)
                return;
                
            // Remove existing marker objects
            var existingMarkers = GameObject.FindGameObjectsWithTag("SplineMarker");
            foreach (var marker in existingMarkers)
            {
                DestroyImmediate(marker);
            }
            
            // Create new marker objects
            var markerParent = new GameObject("Spline Markers (Temporary)");
            markerParent.transform.SetParent(_levelMap.transform);
            
            for (int i = 0; i < bakedData.designerMarkers.Length; i++)
            {
                var marker = bakedData.designerMarkers[i];
                var markerGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                markerGO.name = $"Marker_{i}_{marker.type}";
                markerGO.tag = "SplineMarker";
                markerGO.transform.SetParent(markerParent.transform);
                markerGO.transform.position = marker.position;
                markerGO.transform.localScale = Vector3.one * 0.5f;
                
                // Color code by marker type
                var renderer = markerGO.GetComponent<Renderer>();
                switch (marker.type)
                {
                    case MarkerType.SpawnPoint:
                        renderer.material.color = Color.green;
                        break;
                    case MarkerType.EndPoint:
                        renderer.material.color = Color.red;
                        break;
                    case MarkerType.SharpTurn:
                        renderer.material.color = Color.yellow;
                        break;
                    case MarkerType.Decoration:
                        renderer.material.color = Color.cyan;
                        break;
                    default:
                        renderer.material.color = Color.white;
                        break;
                }
                
                // Remove collider to avoid physics interactions
                DestroyImmediate(markerGO.GetComponent<Collider>());
            }
            
            Debug.Log($"[LevelMapEditor] Created {bakedData.designerMarkers.Length} temporary marker objects");
            EditorUtility.DisplayDialog("Show Markers", 
                $"Created {bakedData.designerMarkers.Length} temporary marker objects in scene.\n" +
                "These are for reference only - delete them when done.", "OK");
        }
    }
}