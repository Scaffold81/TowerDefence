using UnityEngine;
using UnityEditor;
using Game.Path;
using System.Collections.Generic;
using System.Linq;

namespace Editor.Path
{
    /// <summary>
    /// Окно редактора для управления путями уровня
    /// </summary>
    public class PathManagerWindow : EditorWindow
    {
        private LevelMap _selectedLevelMap;
        private ValidationResult _lastValidationResult;
        private Vector2 _scrollPosition;
        private bool _showValidationDetails = true;
        private bool _showRecommendedPositions = false;
        private List<Vector3> _recommendedTowerPositions = new List<Vector3>();
        private PathTiming _pathTiming;
        
        // Настройки анализа
        private float _enemySpeed = 5f;
        private int _maxTowerPositions = 10;
        
        [MenuItem("Tools/Tower Defence/Path Manager")]
        public static void ShowWindow()
        {
            PathManagerWindow window = GetWindow<PathManagerWindow>("Path Manager");
            window.minSize = new Vector2(400, 600);
        }
        
        private void OnEnable()
        {
            // Автоматически находим LevelMap в сцене при открытии окна
            FindLevelMapInScene();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space();
            
            DrawLevelMapSelection();
            EditorGUILayout.Space();
            
            if (_selectedLevelMap != null)
            {
                DrawLevelMapInfo();
                EditorGUILayout.Space();
                
                DrawValidationSection();
                EditorGUILayout.Space();
                
                DrawAnalysisSection();
                EditorGUILayout.Space();
                
                DrawToolsSection();
            }
            else
            {
                EditorGUILayout.HelpBox("No LevelMap selected. Select a LevelMap in the scene or create a new one.", MessageType.Info);
                
                if (GUILayout.Button("Create New LevelMap"))
                {
                    CreateNewLevelMap();
                }
            }
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Path Manager", EditorStyles.largeLabel);
            EditorGUILayout.LabelField("Tool for managing and validating level paths", EditorStyles.miniLabel);
        }
        
        private void DrawLevelMapSelection()
        {
            EditorGUILayout.LabelField("Level Map", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            _selectedLevelMap = (LevelMap)EditorGUILayout.ObjectField("LevelMap", _selectedLevelMap, typeof(LevelMap), true);
            
            if (EditorGUI.EndChangeCheck())
            {
                OnLevelMapChanged();
            }
            
            if (GUILayout.Button("Find in Scene", GUILayout.Width(100)))
            {
                FindLevelMapInScene();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (_selectedLevelMap != null && GUILayout.Button("Select in Hierarchy"))
            {
                Selection.activeGameObject = _selectedLevelMap.gameObject;
                EditorGUIUtility.PingObject(_selectedLevelMap.gameObject);
            }
        }
        
        private void DrawLevelMapInfo()
        {
            EditorGUILayout.LabelField("Level Information", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField($"Level Name: {_selectedLevelMap.LevelName}");
            EditorGUILayout.LabelField($"Waypoints: {_selectedLevelMap.Waypoints.Count}");
            EditorGUILayout.LabelField($"Terrain: {(_selectedLevelMap.Terrain != null ? _selectedLevelMap.Terrain.name : "Not Assigned")}");
            
            // Статистика по типам waypoints
            var spawnPoints = _selectedLevelMap.Waypoints.OfType<SpawnPoint>().Count();
            var endPoints = _selectedLevelMap.Waypoints.OfType<EndPoint>().Count();
            var intermediatePoints = _selectedLevelMap.Waypoints.OfType<IntermediateWaypoint>().Count();
            
            EditorGUILayout.LabelField($"SpawnPoints: {spawnPoints}, EndPoints: {endPoints}, Intermediate: {intermediatePoints}");
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawValidationSection()
        {
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Validate Level"))
            {
                ValidateLevel();
            }
            
            if (GUILayout.Button("Auto-Fix Issues"))
            {
                AutoFixIssues();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (_lastValidationResult != null)
            {
                DrawValidationResults();
            }
        }
        
        private void DrawValidationResults()
        {
            _showValidationDetails = EditorGUILayout.Foldout(_showValidationDetails, "Validation Results", true);
            
            if (_showValidationDetails)
            {
                EditorGUI.indentLevel++;
                
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
                
                // Ошибки
                if (_lastValidationResult.Errors.Count > 0)
                {
                    EditorGUILayout.LabelField($"Errors ({_lastValidationResult.Errors.Count}):", EditorStyles.boldLabel);
                    foreach (string error in _lastValidationResult.Errors)
                    {
                        EditorGUILayout.HelpBox(error, MessageType.Error);
                    }
                }
                
                // Предупреждения
                if (_lastValidationResult.Warnings.Count > 0)
                {
                    EditorGUILayout.LabelField($"Warnings ({_lastValidationResult.Warnings.Count}):", EditorStyles.boldLabel);
                    foreach (string warning in _lastValidationResult.Warnings)
                    {
                        EditorGUILayout.HelpBox(warning, MessageType.Warning);
                    }
                }
                
                // Информация
                if (_lastValidationResult.Info.Count > 0)
                {
                    EditorGUILayout.LabelField($"Information ({_lastValidationResult.Info.Count}):", EditorStyles.boldLabel);
                    foreach (string info in _lastValidationResult.Info)
                    {
                        EditorGUILayout.HelpBox(info, MessageType.Info);
                    }
                }
                
                // Если всё в порядке
                if (_lastValidationResult.IsValid && !_lastValidationResult.HasWarnings)
                {
                    EditorGUILayout.HelpBox("Level validation passed successfully! ✓", MessageType.Info);
                }
                
                EditorGUILayout.EndScrollView();
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawAnalysisSection()
        {
            EditorGUILayout.LabelField("Path Analysis", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            _enemySpeed = EditorGUILayout.FloatField("Enemy Speed", _enemySpeed);
            if (GUILayout.Button("Analyze Timing", GUILayout.Width(100)))
            {
                AnalyzePathTiming();
            }
            EditorGUILayout.EndHorizontal();
            
            if (_pathTiming.TotalDistance > 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Total Distance: {_pathTiming.TotalDistance:F1}m");
                EditorGUILayout.LabelField($"Estimated Time: {_pathTiming.TotalTime:F1}s");
                EditorGUILayout.LabelField($"Average Speed: {_pathTiming.AverageSpeed:F1}m/s");
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            _maxTowerPositions = EditorGUILayout.IntField("Max Tower Positions", _maxTowerPositions);
            if (GUILayout.Button("Find Tower Positions", GUILayout.Width(150)))
            {
                FindRecommendedTowerPositions();
            }
            EditorGUILayout.EndHorizontal();
            
            if (_recommendedTowerPositions.Count > 0)
            {
                _showRecommendedPositions = EditorGUILayout.Foldout(_showRecommendedPositions, 
                    $"Recommended Tower Positions ({_recommendedTowerPositions.Count})", true);
                
                if (_showRecommendedPositions)
                {
                    EditorGUI.indentLevel++;
                    
                    for (int i = 0; i < _recommendedTowerPositions.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        Vector3 pos = _recommendedTowerPositions[i];
                        EditorGUILayout.LabelField($"Position {i + 1}: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
                        
                        if (GUILayout.Button("Focus", GUILayout.Width(50)))
                        {
                            SceneView.lastActiveSceneView.pivot = pos;
                            SceneView.lastActiveSceneView.Repaint();
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Show in Scene"))
                    {
                        ShowTowerPositionsInScene(true);
                    }
                    
                    if (GUILayout.Button("Hide in Scene"))
                    {
                        ShowTowerPositionsInScene(false);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        private void DrawToolsSection()
        {
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Collect Waypoints"))
            {
                Undo.RecordObject(_selectedLevelMap, "Collect Waypoints");
                _selectedLevelMap.CollectWaypointsFromScene();
                EditorUtility.SetDirty(_selectedLevelMap);
                ValidateLevel();
            }
            
            if (GUILayout.Button("Snap to Terrain"))
            {
                Undo.RecordObject(_selectedLevelMap, "Snap Waypoints to Terrain");
                _selectedLevelMap.SnapAllWaypointsToTerrain();
                EditorUtility.SetDirty(_selectedLevelMap);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Refresh Indices"))
            {
                Undo.RecordObject(_selectedLevelMap, "Refresh Waypoint Indices");
                _selectedLevelMap.RefreshWaypointIndices();
                EditorUtility.SetDirty(_selectedLevelMap);
            }
            
            if (GUILayout.Button("Open Placement Tool"))
            {
                WaypointPlacementHelper.TogglePlacementMode();
                SceneView.lastActiveSceneView?.Focus();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Export Path Data"))
            {
                ExportPathData();
            }
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Import Path Data"))
            {
                ImportPathData();
            }
            
            GUI.backgroundColor = Color.white;
        }
        
        private void FindLevelMapInScene()
        {
            LevelMap levelMap = FindObjectOfType<LevelMap>();
            if (levelMap != null)
            {
                _selectedLevelMap = levelMap;
                OnLevelMapChanged();
            }
        }
        
        private void CreateNewLevelMap()
        {
            GameObject levelMapGO = new GameObject("LevelMap");
            _selectedLevelMap = levelMapGO.AddComponent<LevelMap>();
            
            // Находим terrain автоматически
            Terrain terrain = FindObjectOfType<Terrain>();
            if (terrain != null)
            {
                _selectedLevelMap.Terrain = terrain;
            }
            
            Undo.RegisterCreatedObjectUndo(levelMapGO, "Create LevelMap");
            Selection.activeGameObject = levelMapGO;
            
            OnLevelMapChanged();
        }
        
        private void OnLevelMapChanged()
        {
            if (_selectedLevelMap != null)
            {
                ValidateLevel();
                AnalyzePathTiming();
            }
            
            SceneView.RepaintAll();
        }
        
        private void ValidateLevel()
        {
            if (_selectedLevelMap == null) return;
            
            _lastValidationResult = PathValidator.ValidateLevel(_selectedLevelMap);
            
            // Выводим результат в консоль
            if (_lastValidationResult.IsValid)
            {
                Debug.Log($"Level '{_selectedLevelMap.LevelName}' validation passed successfully!");
            }
            else
            {
                Debug.LogWarning($"Level '{_selectedLevelMap.LevelName}' has validation issues:\n{_lastValidationResult}");
            }
        }
        
        private void AutoFixIssues()
        {
            if (_selectedLevelMap == null || _lastValidationResult == null) return;
            
            Undo.RecordObject(_selectedLevelMap, "Auto-Fix Path Issues");
            
            bool fixedIssues = false;
            
            // Автоматически привязываем waypoints к terrain
            foreach (var waypoint in _selectedLevelMap.Waypoints)
            {
                if (waypoint != null)
                {
                    waypoint.SnapToTerrainSurface();
                    fixedIssues = true;
                }
            }
            
            // Обновляем индексы
            _selectedLevelMap.RefreshWaypointIndices();
            
            // Собираем waypoints из сцены
            _selectedLevelMap.CollectWaypointsFromScene();
            
            if (fixedIssues)
            {
                EditorUtility.SetDirty(_selectedLevelMap);
                ValidateLevel();
                Debug.Log("Auto-fix completed. Some issues may require manual attention.");
            }
            else
            {
                Debug.Log("No issues were automatically fixed. Manual intervention may be required.");
            }
        }
        
        private void AnalyzePathTiming()
        {
            if (_selectedLevelMap == null) return;
            
            _pathTiming = PathValidator.AnalyzePathTiming(_selectedLevelMap, _enemySpeed);
            
            Debug.Log($"Path timing analysis: {_pathTiming}");
        }
        
        private void FindRecommendedTowerPositions()
        {
            if (_selectedLevelMap == null) return;
            
            _recommendedTowerPositions = PathValidator.GetRecommendedTowerPositions(_selectedLevelMap, _maxTowerPositions);
            
            Debug.Log($"Found {_recommendedTowerPositions.Count} recommended tower positions");
        }
        
        private void ShowTowerPositionsInScene(bool show)
        {
            // В будущем здесь можно добавить визуализацию позиций башен в Scene View
            // Пока просто выводим в консоль
            if (show)
            {
                Debug.Log("Tower positions visualization enabled (feature coming soon)");
            }
            else
            {
                Debug.Log("Tower positions visualization disabled");
            }
        }
        
        private void ExportPathData()
        {
            if (_selectedLevelMap == null) return;
            
            string path = EditorUtility.SaveFilePanel("Export Path Data", 
                Application.dataPath, _selectedLevelMap.LevelName + "_path", "json");
            
            if (!string.IsNullOrEmpty(path))
            {
                // Создаем данные для экспорта
                var pathData = new PathExportData
                {
                    levelName = _selectedLevelMap.LevelName,
                    waypoints = _selectedLevelMap.Waypoints.Where(w => w != null)
                        .Select(w => new WaypointData
                        {
                            type = w.GetType().Name,
                            position = w.transform.position,
                            index = w.Index
                        }).ToList()
                };
                
                string json = JsonUtility.ToJson(pathData, true);
                System.IO.File.WriteAllText(path, json);
                
                Debug.Log($"Path data exported to: {path}");
            }
        }
        
        private void ImportPathData()
        {
            string path = EditorUtility.OpenFilePanel("Import Path Data", Application.dataPath, "json");
            
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(path);
                    var pathData = JsonUtility.FromJson<PathExportData>(json);
                    
                    if (EditorUtility.DisplayDialog("Import Path Data",
                        $"Import path data '{pathData.levelName}' with {pathData.waypoints.Count} waypoints?\n\nThis will replace current waypoints.",
                        "Import", "Cancel"))
                    {
                        ImportPathDataFromJson(pathData);
                    }
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("Import Error", $"Failed to import path data:\n{e.Message}", "OK");
                }
            }
        }
        
        private void ImportPathDataFromJson(PathExportData pathData)
        {
            if (_selectedLevelMap == null) return;
            
            Undo.RecordObject(_selectedLevelMap, "Import Path Data");
            
            // Очищаем существующие waypoints
            foreach (var waypoint in _selectedLevelMap.Waypoints.ToArray())
            {
                if (waypoint != null)
                {
                    Undo.DestroyObjectImmediate(waypoint.gameObject);
                }
            }
            
            _selectedLevelMap.ClearWaypoints();
            
            // Создаем новые waypoints
            foreach (var waypointData in pathData.waypoints.OrderBy(w => w.index))
            {
                GameObject waypointGO = new GameObject(waypointData.type);
                waypointGO.transform.SetParent(_selectedLevelMap.transform);
                waypointGO.transform.position = waypointData.position;
                
                Waypoint waypoint = null;
                
                switch (waypointData.type)
                {
                    case nameof(SpawnPoint):
                        waypoint = waypointGO.AddComponent<SpawnPoint>();
                        break;
                    case nameof(EndPoint):
                        waypoint = waypointGO.AddComponent<EndPoint>();
                        break;
                    case nameof(IntermediateWaypoint):
                        waypoint = waypointGO.AddComponent<IntermediateWaypoint>();
                        break;
                }
                
                if (waypoint != null)
                {
                    waypoint.Index = waypointData.index;
                    _selectedLevelMap.AddWaypoint(waypoint);
                    Undo.RegisterCreatedObjectUndo(waypointGO, "Create Imported Waypoint");
                }
            }
            
            _selectedLevelMap.LevelName = pathData.levelName;
            EditorUtility.SetDirty(_selectedLevelMap);
            
            ValidateLevel();
            
            Debug.Log($"Successfully imported {pathData.waypoints.Count} waypoints");
        }
    }
    
    /// <summary>
    /// Данные для экспорта/импорта путей
    /// </summary>
    [System.Serializable]
    public class PathExportData
    {
        public string levelName;
        public List<WaypointData> waypoints;
    }
    
    [System.Serializable]
    public class WaypointData
    {
        public string type;
        public Vector3 position;
        public int index;
    }
}