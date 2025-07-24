using UnityEngine;
using UnityEditor;
using Game.Path;

namespace Editor.Path
{
    [CustomEditor(typeof(Waypoint), true)]
    public class WaypointEditor : UnityEditor.Editor
    {
        protected Waypoint _waypoint;
        protected SerializedProperty _indexProp;
        protected SerializedProperty _gizmoColorProp;
        protected SerializedProperty _gizmoSizeProp;
        protected SerializedProperty _snapToTerrainProp;
        protected SerializedProperty _terrainLayerProp;
        
        protected virtual void OnEnable()
        {
            _waypoint = (Waypoint)target;
            
            _indexProp = serializedObject.FindProperty("_index");
            _gizmoColorProp = serializedObject.FindProperty("_gizmoColor");
            _gizmoSizeProp = serializedObject.FindProperty("_gizmoSize");
            _snapToTerrainProp = serializedObject.FindProperty("_snapToTerrain");
            _terrainLayerProp = serializedObject.FindProperty("_terrainLayer");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawWaypointHeader();
            EditorGUILayout.Space();
            
            DrawWaypointSettings();
            EditorGUILayout.Space();
            
            DrawValidationSettings();
            EditorGUILayout.Space();
            
            DrawQuickActions();
            EditorGUILayout.Space();
            
            DrawValidationResults();
            
            // Рисуем специфические настройки для наследников
            DrawSpecificSettings();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        protected virtual void DrawWaypointHeader()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Цветовая индикация типа waypoint
            Color backgroundColor = GUI.backgroundColor;
            if (_waypoint is SpawnPoint) GUI.backgroundColor = Color.green;
            else if (_waypoint is EndPoint) GUI.backgroundColor = Color.red;
            else if (_waypoint is IntermediateWaypoint) GUI.backgroundColor = Color.blue;
            
            string waypointType = _waypoint.GetType().Name;
            EditorGUILayout.LabelField($"{waypointType} Settings", EditorStyles.boldLabel);
            
            GUI.backgroundColor = backgroundColor;
            EditorGUILayout.EndHorizontal();
            
            // Информация о позиции
            Vector3 pos = _waypoint.transform.position;
            EditorGUILayout.LabelField($"Position: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})", EditorStyles.miniLabel);
        }
        
        protected virtual void DrawWaypointSettings()
        {
            EditorGUILayout.LabelField("Waypoint Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_indexProp, new GUIContent("Index"));
            EditorGUILayout.PropertyField(_gizmoColorProp, new GUIContent("Gizmo Color"));
            EditorGUILayout.PropertyField(_gizmoSizeProp, new GUIContent("Gizmo Size"));
        }
        
        protected virtual void DrawValidationSettings()
        {
            EditorGUILayout.LabelField("Validation Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_snapToTerrainProp, new GUIContent("Snap to Terrain"));
            EditorGUILayout.PropertyField(_terrainLayerProp, new GUIContent("Terrain Layer"));
        }
        
        protected virtual void DrawQuickActions()
        {
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Snap to Terrain"))
            {
                Undo.RecordObject(_waypoint.transform, "Snap Waypoint to Terrain");
                _waypoint.SnapToTerrainSurface();
                EditorUtility.SetDirty(_waypoint);
            }
            
            if (GUILayout.Button("Focus in Scene"))
            {
                SceneView.lastActiveSceneView?.FrameSelected();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Find LevelMap"))
            {
                LevelMap levelMap = _waypoint.GetComponentInParent<LevelMap>();
                if (levelMap != null)
                {
                    Selection.activeGameObject = levelMap.gameObject;
                    EditorGUIUtility.PingObject(levelMap.gameObject);
                }
                else
                {
                    EditorUtility.DisplayDialog("LevelMap Not Found", 
                        "This waypoint is not a child of any LevelMap.", "OK");
                }
            }
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete Waypoint"))
            {
                if (EditorUtility.DisplayDialog("Delete Waypoint", 
                    "Are you sure you want to delete this waypoint?", "Yes", "Cancel"))
                {
                    // Находим LevelMap и удаляем waypoint из него
                    LevelMap levelMap = _waypoint.GetComponentInParent<LevelMap>();
                    if (levelMap != null)
                    {
                        Undo.RecordObject(levelMap, "Remove Waypoint from LevelMap");
                        levelMap.RemoveWaypoint(_waypoint);
                        EditorUtility.SetDirty(levelMap);
                    }
                    
                    // Удаляем GameObject
                    Undo.DestroyObjectImmediate(_waypoint.gameObject);
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
        }
        
        protected virtual void DrawValidationResults()
        {
            if (_waypoint.ValidatePosition(out string errorMessage))
            {
                EditorGUILayout.HelpBox("Waypoint validation passed!", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Validation Error: {errorMessage}", MessageType.Error);
            }
        }
        
        protected virtual void DrawSpecificSettings()
        {
            // Переопределяется в наследниках для специфических настроек
        }
        
        protected virtual void OnSceneGUI()
        {
            DrawPositionHandle();
            DrawWaypointInfo();
        }
        
        protected virtual void DrawPositionHandle()
        {
            // Позволяем перемещать waypoint в Scene View
            EditorGUI.BeginChangeCheck();
            
            Vector3 newPosition = Handles.PositionHandle(_waypoint.transform.position, _waypoint.transform.rotation);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_waypoint.transform, "Move Waypoint");
                _waypoint.transform.position = newPosition;
                
                // Автоматически привязываем к terrain если включено
                if (_waypoint.SnapToTerrain)
                {
                    _waypoint.SnapToTerrainSurface();
                }
                
                EditorUtility.SetDirty(_waypoint);
            }
        }
        
        protected virtual void DrawWaypointInfo()
        {
            // Отображаем информацию о waypoint в Scene View
            Handles.color = Color.white;
            
            Vector3 labelPosition = _waypoint.transform.position + Vector3.up * (_waypoint.GizmoSize + 1f);
            
            string info = $"{_waypoint.GetType().Name}\nIndex: {_waypoint.Index}";
            
            // Добавляем информацию о валидации
            if (!_waypoint.ValidatePosition(out string error))
            {
                info += $"\n⚠ {error}";
                Handles.color = Color.red;
            }
            
            Handles.Label(labelPosition, info, EditorStyles.whiteLabel);
        }
    }
    
    // Специализированные редакторы для разных типов waypoints
    
    [CustomEditor(typeof(SpawnPoint))]
    public class SpawnPointEditor : WaypointEditor
    {
        private SerializedProperty _spawnDirectionProp;
        private SerializedProperty _spawnRadiusProp;
        private SerializedProperty _maxConcurrentEnemiesProp;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            _spawnDirectionProp = serializedObject.FindProperty("_spawnDirection");
            _spawnRadiusProp = serializedObject.FindProperty("_spawnRadius");
            _maxConcurrentEnemiesProp = serializedObject.FindProperty("_maxConcurrentEnemies");
        }
        
        protected override void DrawSpecificSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Point Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_spawnDirectionProp, new GUIContent("Spawn Direction"));
            EditorGUILayout.PropertyField(_spawnRadiusProp, new GUIContent("Spawn Radius"));
            EditorGUILayout.PropertyField(_maxConcurrentEnemiesProp, new GUIContent("Max Concurrent Enemies"));
            
            SpawnPoint spawnPoint = (SpawnPoint)_waypoint;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Test Random Spawn Position"))
            {
                Vector3 randomPos = spawnPoint.GetRandomSpawnPosition();
                Debug.Log($"Random spawn position: {randomPos}");
                
                // Создаем временный объект для визуализации
                GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                testObj.name = "Test Spawn Position";
                testObj.transform.position = randomPos;
                testObj.transform.localScale = Vector3.one * 0.5f;
                
                // Удаляем через 2 секунды
                EditorApplication.delayCall += () =>
                {
                    if (testObj != null)
                        DestroyImmediate(testObj);
                };
            }
        }
        
        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            
            SpawnPoint spawnPoint = (SpawnPoint)_waypoint;
            
            // Рисуем направление спавна с возможностью редактирования
            Handles.color = Color.green;
            Vector3 directionEnd = spawnPoint.transform.position + spawnPoint.GetSpawnDirection() * 3f;
            
            EditorGUI.BeginChangeCheck();
            Handles.ArrowHandleCap(0, directionEnd, 
                Quaternion.LookRotation(spawnPoint.GetSpawnDirection()), 1f, EventType.Repaint);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawnPoint, "Change Spawn Direction");
                // Направление можно изменять в Inspectorе
                EditorUtility.SetDirty(spawnPoint);
            }
            
            // Рисуем радиус спавна с возможностью редактирования
            Handles.color = new Color(0f, 1f, 0f, 0.1f);
            EditorGUI.BeginChangeCheck();
            float newRadius = Handles.RadiusHandle(Quaternion.identity, spawnPoint.transform.position, spawnPoint.SpawnRadius);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawnPoint, "Change Spawn Radius");
                spawnPoint.SpawnRadius = newRadius;
                EditorUtility.SetDirty(spawnPoint);
            }
        }
    }
    
    [CustomEditor(typeof(EndPoint))]
    public class EndPointEditor : WaypointEditor
    {
        private SerializedProperty _baseRadiusProp;
        private SerializedProperty _damageZoneRadiusProp;
        private SerializedProperty _baseHealthProp;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            _baseRadiusProp = serializedObject.FindProperty("_baseRadius");
            _damageZoneRadiusProp = serializedObject.FindProperty("_damageZoneRadius");
            _baseHealthProp = serializedObject.FindProperty("_baseHealth");
        }
        
        protected override void DrawSpecificSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("End Point Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_baseRadiusProp, new GUIContent("Base Radius"));
            EditorGUILayout.PropertyField(_damageZoneRadiusProp, new GUIContent("Damage Zone Radius"));
            EditorGUILayout.PropertyField(_baseHealthProp, new GUIContent("Base Health"));
            
            EndPoint endPoint = (EndPoint)_waypoint;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Zone Information", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Base Area: {(endPoint.BaseRadius * endPoint.BaseRadius * Mathf.PI):F2} sq units");
            EditorGUILayout.LabelField($"Damage Zone Area: {(endPoint.DamageZoneRadius * endPoint.DamageZoneRadius * Mathf.PI):F2} sq units");
        }
        
        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            
            EndPoint endPoint = (EndPoint)_waypoint;
            
            // Рисуем радиус базы с возможностью редактирования
            Handles.color = new Color(1f, 0f, 0f, 0.1f);
            EditorGUI.BeginChangeCheck();
            float newBaseRadius = Handles.RadiusHandle(Quaternion.identity, endPoint.transform.position, endPoint.BaseRadius);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(endPoint, "Change Base Radius");
                endPoint.BaseRadius = newBaseRadius;
                EditorUtility.SetDirty(endPoint);
            }
            
            // Рисуем радиус зоны поражения
            Handles.color = new Color(1f, 0.5f, 0f, 0.2f);
            EditorGUI.BeginChangeCheck();
            float newDamageRadius = Handles.RadiusHandle(Quaternion.identity, endPoint.transform.position, endPoint.DamageZoneRadius);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(endPoint, "Change Damage Zone Radius");
                endPoint.DamageZoneRadius = newDamageRadius;
                EditorUtility.SetDirty(endPoint);
            }
        }
    }
    
    [CustomEditor(typeof(IntermediateWaypoint))]
    public class IntermediateWaypointEditor : WaypointEditor
    {
        private SerializedProperty _turnTypeProp;
        private SerializedProperty _influenceRadiusProp;
        private SerializedProperty _smoothingFactorProp;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            _turnTypeProp = serializedObject.FindProperty("_turnType");
            _influenceRadiusProp = serializedObject.FindProperty("_influenceRadius");
            _smoothingFactorProp = serializedObject.FindProperty("_smoothingFactor");
        }
        
        protected override void DrawSpecificSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Intermediate Waypoint Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_turnTypeProp, new GUIContent("Turn Type"));
            EditorGUILayout.PropertyField(_influenceRadiusProp, new GUIContent("Influence Radius"));
            
            IntermediateWaypoint waypoint = (IntermediateWaypoint)_waypoint;
            
            if (waypoint.Turn == IntermediateWaypoint.TurnType.Custom)
            {
                EditorGUILayout.PropertyField(_smoothingFactorProp, new GUIContent("Smoothing Factor"));
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Turn Information", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Influence Factor: {waypoint.GetInfluenceFactor():F2}");
        }
        
        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            
            IntermediateWaypoint waypoint = (IntermediateWaypoint)_waypoint;
            
            // Рисуем зону влияния с возможностью редактирования
            Handles.color = new Color(0f, 0f, 1f, 0.1f);
            EditorGUI.BeginChangeCheck();
            float newInfluenceRadius = Handles.RadiusHandle(Quaternion.identity, waypoint.transform.position, waypoint.InfluenceRadius);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(waypoint, "Change Influence Radius");
                waypoint.InfluenceRadius = newInfluenceRadius;
                EditorUtility.SetDirty(waypoint);
            }
        }
    }
}