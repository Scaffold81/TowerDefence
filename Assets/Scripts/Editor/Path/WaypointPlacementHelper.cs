using UnityEngine;
using UnityEditor;
using Game.Path;

namespace Editor.Path
{
    /// <summary>
    /// Простая версия инструмента для размещения waypoints через Scene View
    /// </summary>
    public static class WaypointPlacementHelper
    {
        private static bool _isPlacementModeActive = false;
        private static WaypointType _currentPlacementType = WaypointType.IntermediateWaypoint;
        private static bool _snapToTerrain = true;
        private static LayerMask _terrainLayer = 1;
        
        public enum WaypointType
        {
            SpawnPoint,
            IntermediateWaypoint,
            EndPoint
        }
        
        [MenuItem("Tools/Tower Defence/Toggle Waypoint Placement Mode")]
        public static void TogglePlacementMode()
        {
            _isPlacementModeActive = !_isPlacementModeActive;
            
            if (_isPlacementModeActive)
            {
                SceneView.duringSceneGui += OnSceneGUI;
                Debug.Log("Waypoint Placement Mode: ENABLED. Left-click in Scene View to place waypoints.");
            }
            else
            {
                SceneView.duringSceneGui -= OnSceneGUI;
                Debug.Log("Waypoint Placement Mode: DISABLED");
            }
        }
        
        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!_isPlacementModeActive)
                return;
            
            DrawGUI();
            HandleInput();
        }
        
        private static void DrawGUI()
        {
            Handles.BeginGUI();
            
            GUILayout.BeginArea(new Rect(10, 10, 280, 180));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Waypoint Placement Tool", EditorStyles.boldLabel);
            
            GUILayout.Space(5);
            
            // Выбор типа waypoint
            GUILayout.Label("Waypoint Type:");
            _currentPlacementType = (WaypointType)GUILayout.SelectionGrid((int)_currentPlacementType, 
                new string[] { "Spawn Point", "Waypoint", "End Point" }, 1);
            
            GUILayout.Space(5);
            
            // Настройки
            _snapToTerrain = GUILayout.Toggle(_snapToTerrain, "Snap to Terrain");
            
            GUILayout.Space(10);
            
            // Инструкции
            GUILayout.Label("Instructions:", EditorStyles.boldLabel);
            GUILayout.Label("• Left Click: Place waypoint", EditorStyles.wordWrappedMiniLabel);
            GUILayout.Label("• Hold Shift: Place multiple", EditorStyles.wordWrappedMiniLabel);
            GUILayout.Label("• ESC: Exit placement mode", EditorStyles.wordWrappedMiniLabel);
            
            if (GUILayout.Button("Exit Placement Mode"))
            {
                TogglePlacementMode();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
            
            Handles.EndGUI();
        }
        
        private static void HandleInput()
        {
            Event e = Event.current;
            
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                TogglePlacementMode();
                e.Use();
                return;
            }
            
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Vector3 worldPosition = GetWorldPositionFromMouse(e.mousePosition);
                
                if (worldPosition != Vector3.zero)
                {
                    PlaceWaypoint(worldPosition);
                    
                    // Если не держим Shift, продолжаем работу
                    if (!e.shift)
                    {
                        // Оставляем режим активным для множественного размещения
                    }
                    
                    e.Use();
                }
            }
            
            // Предотвращаем выделение объектов при работе с инструментом
            if (e.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            
            // Рисуем курсор
            DrawPreviewCursor();
        }
        
        private static void DrawPreviewCursor()
        {
            Vector3 mousePosition = Event.current.mousePosition;
            Vector3 worldPosition = GetWorldPositionFromMouse(mousePosition);
            
            if (worldPosition != Vector3.zero)
            {
                // Цвет курсора в зависимости от типа
                Color cursorColor = _currentPlacementType switch
                {
                    WaypointType.SpawnPoint => Color.green,
                    WaypointType.EndPoint => Color.red,
                    WaypointType.IntermediateWaypoint => Color.blue,
                    _ => Color.white
                };
                
                Handles.color = cursorColor;
                
                // Рисуем курсор
                float cursorSize = HandleUtility.GetHandleSize(worldPosition) * 0.1f;
                Handles.DrawWireDisc(worldPosition, Vector3.up, cursorSize);
                Handles.DrawLine(worldPosition, worldPosition + Vector3.up * cursorSize * 2);
                
                // Показываем тип waypoint
                Handles.color = Color.white;
                string typeText = _currentPlacementType.ToString();
                Handles.Label(worldPosition + Vector3.up * cursorSize * 3, typeText);
            }
        }
        
        private static Vector3 GetWorldPositionFromMouse(Vector2 mousePosition)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            
            if (_snapToTerrain)
            {
                // Привязка к terrain
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _terrainLayer))
                {
                    return hit.point;
                }
            }
            else
            {
                // Привязка к плоскости Y = 0
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                if (plane.Raycast(ray, out float distance))
                {
                    return ray.GetPoint(distance);
                }
            }
            
            return Vector3.zero;
        }
        
        private static void PlaceWaypoint(Vector3 position)
        {
            // Находим LevelMap в сцене
            LevelMap levelMap = Object.FindFirstObjectByType<LevelMap>();
            if (levelMap == null)
            {
                Debug.LogWarning("No LevelMap found in scene. Create a LevelMap first.");
                return;
            }
            
            // Проверяем ограничения
            if (_currentPlacementType == WaypointType.SpawnPoint && levelMap.SpawnPoint != null)
            {
                Debug.LogWarning("Cannot place SpawnPoint: Level already has one SpawnPoint");
                return;
            }
            
            if (_currentPlacementType == WaypointType.EndPoint && levelMap.EndPoint != null)
            {
                Debug.LogWarning("Cannot place EndPoint: Level already has one EndPoint");
                return;
            }
            
            // Создаем waypoint
            GameObject waypointGO = null;
            Waypoint waypoint = null;
            
            switch (_currentPlacementType)
            {
                case WaypointType.SpawnPoint:
                    waypointGO = new GameObject("SpawnPoint");
                    waypoint = waypointGO.AddComponent<SpawnPoint>();
                    break;
                    
                case WaypointType.IntermediateWaypoint:
                    waypointGO = new GameObject("Waypoint");
                    waypoint = waypointGO.AddComponent<IntermediateWaypoint>();
                    break;
                    
                case WaypointType.EndPoint:
                    waypointGO = new GameObject("EndPoint");
                    waypoint = waypointGO.AddComponent<EndPoint>();
                    break;
            }
            
            if (waypointGO != null && waypoint != null)
            {
                // Настраиваем waypoint
                waypointGO.transform.SetParent(levelMap.transform);
                waypointGO.transform.position = position;
                
                // Регистрируем для Undo
                Undo.RegisterCreatedObjectUndo(waypointGO, $"Place {_currentPlacementType}");
                Undo.RecordObject(levelMap, $"Add {_currentPlacementType}");
                
                // Добавляем в карту
                levelMap.AddWaypoint(waypoint);
                
                EditorUtility.SetDirty(levelMap);
                
                Debug.Log($"Placed {_currentPlacementType} at {position}");
            }
        }
    }
}