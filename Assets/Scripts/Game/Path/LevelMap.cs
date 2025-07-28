using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Services.Spline;

namespace Game.Path
{
    /// <summary>
    /// Основной компонент карты уровня, управляющий waypoints и путями
    /// </summary>
    public class LevelMap : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private string _levelName = "New Level";
        [SerializeField] private Terrain _terrain;
        
        [Header("Path Visualization")]
        [SerializeField] private bool _showPath = true;
        [SerializeField] private bool _showWaypoints = true;
        [SerializeField] private bool _showConnections = true;
        [SerializeField] private Color _pathColor = Color.yellow;
        [SerializeField] private float _pathWidth = 0.2f;
        
        [Header("Waypoints")]
        [SerializeField] private List<Waypoint> _waypoints = new List<Waypoint>();
        
        [Header("Validation")]
        [SerializeField] private bool _autoValidate = true;
        [SerializeField] private bool _showValidationErrors = true;
        
        [Header("Spline System")]
        [SerializeField] private bool _useSplines = true;
        [SerializeField] private SplineSettings _splineSettings;
        [SerializeField] private BakedSplineData _bakedSplineData;
        
        private readonly List<string> _validationErrors = new List<string>();
        
        public string LevelName
        {
            get => _levelName;
            set => _levelName = value;
        }
        
        public Terrain Terrain
        {
            get => _terrain;
            set => _terrain = value;
        }
        
        public bool ShowPath
        {
            get => _showPath;
            set => _showPath = value;
        }
        
        public bool ShowWaypoints
        {
            get => _showWaypoints;
            set => _showWaypoints = value;
        }
        
        public bool ShowConnections
        {
            get => _showConnections;
            set => _showConnections = value;
        }
        
        public Color PathColor
        {
            get => _pathColor;
            set => _pathColor = value;
        }
        
        public float PathWidth
        {
            get => _pathWidth;
            set => _pathWidth = Mathf.Max(0.1f, value);
        }
        
        public List<Waypoint> Waypoints => _waypoints;
        public IReadOnlyList<string> ValidationErrors => _validationErrors;
        
        public SpawnPoint SpawnPoint => _waypoints.OfType<SpawnPoint>().FirstOrDefault();
        public EndPoint EndPoint => _waypoints.OfType<EndPoint>().FirstOrDefault();
        public IEnumerable<IntermediateWaypoint> IntermediateWaypoints => _waypoints.OfType<IntermediateWaypoint>();
        
        /// <summary>
        /// Spline system properties
        /// </summary>
        public bool UseSplines
        {
            get => _useSplines;
            set => _useSplines = value;
        }
        
        public SplineSettings SplineSettings
        {
            get => _splineSettings ?? (_splineSettings = SplineSettings.CreateDefault());
            set => _splineSettings = value;
        }
        
        public BakedSplineData BakedSplineData => _bakedSplineData;
        
        /// <summary>
        /// Gets all waypoints as array (used by spline system)
        /// </summary>
        public Waypoint[] GetWaypoints() => _waypoints.ToArray();
        
        /// <summary>
        /// Sets baked spline data (called by spline baker)
        /// </summary>
        public void SetBakedSplineData(BakedSplineData data)
        {
            _bakedSplineData = data;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// Gets baked spline data, ensuring it's valid
        /// </summary>
        public BakedSplineData GetBakedSplineData()
        {
            if (_bakedSplineData != null && _bakedSplineData.isValid)
                return _bakedSplineData;
                
            return null;
        }
        
        /// <summary>
        /// Checks if spline data needs rebaking
        /// </summary>
        public bool NeedsSplineRebaking()
        {
            if (!_useSplines) return false;
            if (_bakedSplineData == null) return true;
            
            var waypoints = GetWaypointPositions();
            var splineSystem = new SplineSystem();
            string currentHash = splineSystem.CalculateWaypointHash(waypoints);
            
            return _bakedSplineData.NeedsRebaking(currentHash);
        }
        
        /// <summary>
        /// Gets waypoint positions for spline generation
        /// </summary>
        public Vector3[] GetWaypointPositions()
        {
            var sortedWaypoints = _waypoints.Where(w => w != null)
                                           .OrderBy(w => w.Index)
                                           .ToArray();
            
            var positions = new Vector3[sortedWaypoints.Length];
            for (int i = 0; i < sortedWaypoints.Length; i++)
            {
                positions[i] = sortedWaypoints[i].transform.position;
            }
            
            return positions;
        }
        
        /// <summary>
        /// Gets nearest spline point to world position (for designer tools)
        /// </summary>
        public Vector3 GetNearestSplinePoint(Vector3 worldPosition)
        {
            if (_bakedSplineData?.referencePoints == null)
                return worldPosition;
                
            var nearestPoint = _bakedSplineData.GetNearestPoint(worldPosition);
            return nearestPoint.position;
        }
        
        /// <summary>
        /// Invalidates baked spline data (call when waypoints change)
        /// </summary>
        public void InvalidateSplineData()
        {
            if (_bakedSplineData != null)
            {
                _bakedSplineData.Invalidate();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }
        
        /// <summary>
        /// Добавить waypoint в карту
        /// </summary>
        public void AddWaypoint(Waypoint waypoint)
        {
            if (waypoint == null || _waypoints.Contains(waypoint))
                return;
            
            _waypoints.Add(waypoint);
            RefreshWaypointIndices();
            
            if (_autoValidate)
                ValidateLevel();
                
            // Invalidate spline data when waypoints change
            InvalidateSplineData();
        }
        
        /// <summary>
        /// Удалить waypoint из карты
        /// </summary>
        public void RemoveWaypoint(Waypoint waypoint)
        {
            if (waypoint == null || !_waypoints.Contains(waypoint))
                return;
            
            _waypoints.Remove(waypoint);
            RefreshWaypointIndices();
            
            if (_autoValidate)
                ValidateLevel();
                
            // Invalidate spline data when waypoints change
            InvalidateSplineData();
        }
        
        /// <summary>
        /// Обновить индексы всех waypoints
        /// </summary>
        public void RefreshWaypointIndices()
        {
            for (int i = 0; i < _waypoints.Count; i++)
            {
                if (_waypoints[i] != null)
                    _waypoints[i].Index = i;
            }
        }
        
        /// <summary>
        /// Получить waypoint по индексу
        /// </summary>
        public Waypoint GetWaypointByIndex(int index)
        {
            return index >= 0 && index < _waypoints.Count ? _waypoints[index] : null;
        }
        
        /// <summary>
        /// Переместить waypoint в списке
        /// </summary>
        public void MoveWaypoint(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= _waypoints.Count ||
                toIndex < 0 || toIndex >= _waypoints.Count ||
                fromIndex == toIndex)
                return;
            
            Waypoint waypoint = _waypoints[fromIndex];
            _waypoints.RemoveAt(fromIndex);
            _waypoints.Insert(toIndex, waypoint);
            
            RefreshWaypointIndices();
            
            if (_autoValidate)
                ValidateLevel();
                
            // Invalidate spline data when waypoint order changes
            InvalidateSplineData();
        }
        
        /// <summary>
        /// Очистить все waypoints
        /// </summary>
        public void ClearWaypoints()
        {
            _waypoints.Clear();
            _validationErrors.Clear();
            
            // Invalidate spline data when all waypoints cleared
            InvalidateSplineData();
        }
        
        /// <summary>
        /// Валидация уровня
        /// </summary>
        public bool ValidateLevel()
        {
            _validationErrors.Clear();
            
            // Проверяем наличие terrain
            if (_terrain == null)
            {
                _validationErrors.Add("Terrain is not assigned");
            }
            
            // Проверяем минимальное количество waypoints
            if (_waypoints.Count < 2)
            {
                _validationErrors.Add("Level must have at least 2 waypoints (SpawnPoint and EndPoint)");
                return false;
            }
            
            // Проверяем наличие SpawnPoint
            if (SpawnPoint == null)
            {
                _validationErrors.Add("Level must have exactly one SpawnPoint");
            }
            
            // Проверяем наличие EndPoint
            if (EndPoint == null)
            {
                _validationErrors.Add("Level must have exactly one EndPoint");
            }
            
            // Проверяем количество SpawnPoint и EndPoint
            var spawnPoints = _waypoints.OfType<SpawnPoint>().Count();
            var endPoints = _waypoints.OfType<EndPoint>().Count();
            
            if (spawnPoints > 1)
            {
                _validationErrors.Add($"Level has {spawnPoints} SpawnPoints, but should have exactly 1");
            }
            
            if (endPoints > 1)
            {
                _validationErrors.Add($"Level has {endPoints} EndPoints, but should have exactly 1");
            }
            
            // Валидация каждого waypoint
            for (int i = 0; i < _waypoints.Count; i++)
            {
                if (_waypoints[i] == null)
                {
                    _validationErrors.Add($"Waypoint at index {i} is null");
                    continue;
                }
                
                if (!_waypoints[i].ValidatePosition(out string error))
                {
                    _validationErrors.Add($"Waypoint {i} ({_waypoints[i].GetType().Name}): {error}");
                }
            }
            
            // Проверяем расстояния между соседними waypoints
            for (int i = 0; i < _waypoints.Count - 1; i++)
            {
                if (_waypoints[i] == null || _waypoints[i + 1] == null)
                    continue;
                
                float distance = Vector3.Distance(_waypoints[i].transform.position, _waypoints[i + 1].transform.position);
                
                if (distance < 1f)
                {
                    _validationErrors.Add($"Waypoints {i} and {i + 1} are too close ({distance:F2}m). Minimum distance is 1m");
                }
                else if (distance > 50f)
                {
                    _validationErrors.Add($"Waypoints {i} and {i + 1} are too far apart ({distance:F2}m). Consider adding intermediate waypoints");
                }
            }
            
            return _validationErrors.Count == 0;
        }
        
        /// <summary>
        /// Найти все waypoints на сцене и добавить их в карту
        /// </summary>
        public void CollectWaypointsFromScene()
        {
            var foundWaypoints = Object.FindObjectsByType<Waypoint>(FindObjectsSortMode.None)
                .Where(w => w.transform.IsChildOf(transform))
                .OrderBy(w => w.Index)
                .ToList();
            
            _waypoints.Clear();
            _waypoints.AddRange(foundWaypoints);
            RefreshWaypointIndices();
            
            if (_autoValidate)
                ValidateLevel();
                
            // Invalidate spline data when waypoints collected
            InvalidateSplineData();
        }
        
        /// <summary>
        /// Привязать все waypoints к поверхности terrain
        /// </summary>
        public void SnapAllWaypointsToTerrain()
        {
            foreach (var waypoint in _waypoints)
            {
                waypoint?.SnapToTerrainSurface();
            }
            
            // Invalidate spline data when waypoint positions change
            InvalidateSplineData();
        }
        
        private void OnValidate()
        {
            if (_autoValidate && Application.isEditor && !Application.isPlaying)
            {
                ValidateLevel();
            }
            
            // Ensure spline settings exist
            if (_splineSettings == null)
            {
                _splineSettings = SplineSettings.CreateDefault();
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!_showPath && !_showConnections && !_showWaypoints)
                return;
            
            DrawPathVisualization();
            
            // Draw spline visualization if enabled
            if (_useSplines)
            {
                DrawSplineVisualization();
            }
        }
        
        private void DrawPathVisualization()
        {
            if (_waypoints == null || _waypoints.Count < 2)
                return;
            
            // Отображаем соединения между waypoints
            if (_showConnections)
            {
                Gizmos.color = _pathColor;
                
                for (int i = 0; i < _waypoints.Count - 1; i++)
                {
                    if (_waypoints[i] == null || _waypoints[i + 1] == null)
                        continue;
                    
                    Vector3 from = _waypoints[i].transform.position;
                    Vector3 to = _waypoints[i + 1].transform.position;
                    
                    // Рисуем линию соединения
                    Gizmos.DrawLine(from, to);
                    
                    // Рисуем стрелку направления
                    Vector3 direction = (to - from).normalized;
                    Vector3 arrowPos = Vector3.Lerp(from, to, 0.7f);
                    DrawArrow(arrowPos, direction, 0.5f);
                }
            }
            
            // Отображаем путь как последовательность точек
            if (_showPath)
            {
                Gizmos.color = new Color(_pathColor.r, _pathColor.g, _pathColor.b, 0.6f);
                
                for (int i = 0; i < _waypoints.Count; i++)
                {
                    if (_waypoints[i] == null) continue;
                    
                    Vector3 pos = _waypoints[i].transform.position;
                    Gizmos.DrawWireCube(pos + Vector3.up * 0.1f, Vector3.one * _pathWidth);
                }
            }
            
            // Отображаем ошибки валидации
            if (_showValidationErrors && _validationErrors.Count > 0)
            {
                Gizmos.color = Color.red;
                Vector3 center = transform.position + Vector3.up * 5f;
                Gizmos.DrawWireCube(center, Vector3.one * 2f);
                
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.red;
                string errorText = $"Validation Errors: {_validationErrors.Count}";
                UnityEditor.Handles.Label(center + Vector3.up * 1.5f, errorText);
#endif
            }
        }
        
        private void DrawArrow(Vector3 position, Vector3 direction, float size)
        {
            Vector3 arrowHead = position + direction * size;
            Vector3 arrowSide1 = Quaternion.AngleAxis(30f, Vector3.up) * -direction * (size * 0.5f);
            Vector3 arrowSide2 = Quaternion.AngleAxis(-30f, Vector3.up) * -direction * (size * 0.5f);
            
            Gizmos.DrawLine(arrowHead, arrowHead + arrowSide1);
            Gizmos.DrawLine(arrowHead, arrowHead + arrowSide2);
        }
        
        /// <summary>
        /// Draws spline visualization using Gizmos (runtime-safe)
        /// </summary>
        private void DrawSplineVisualization()
        {
            var bakedData = GetBakedSplineData();
            if (bakedData?.referencePoints == null) return;
            
            // Draw spline path
            Gizmos.color = Color.green;
            for (int i = 0; i < bakedData.referencePoints.Length - 1; i++)
            {
                Vector3 from = bakedData.referencePoints[i].position;
                Vector3 to = bakedData.referencePoints[i + 1].position;
                Gizmos.DrawLine(from, to);
            }
            
            // Draw direction arrows (less frequent to avoid clutter)
            Gizmos.color = Color.blue;
            int arrowSpacing = Mathf.Max(1, bakedData.referencePoints.Length / 10);
            for (int i = 0; i < bakedData.referencePoints.Length; i += arrowSpacing)
            {
                var point = bakedData.referencePoints[i];
                DrawSplineArrow(point.position, point.forward, 0.8f);
            }
            
            // Draw designer markers
            if (bakedData.designerMarkers != null)
            {
                foreach (var marker in bakedData.designerMarkers)
                {
                    // Set color based on marker type
                    switch (marker.type)
                    {
                        case Core.Services.Spline.MarkerType.SpawnPoint:
                            Gizmos.color = Color.green;
                            break;
                        case Core.Services.Spline.MarkerType.EndPoint:
                            Gizmos.color = Color.red;
                            break;
                        case Core.Services.Spline.MarkerType.SharpTurn:
                            Gizmos.color = Color.yellow;
                            break;
                        default:
                            Gizmos.color = Color.white;
                            break;
                    }
                    
                    Gizmos.DrawWireSphere(marker.position, 0.8f);
                }
            }
        }
        
        /// <summary>
        /// Draws a simple arrow using Gizmos
        /// </summary>
        private void DrawSplineArrow(Vector3 position, Vector3 direction, float size)
        {
            if (direction.magnitude < 0.001f) return;
            
            Vector3 forward = direction.normalized * size;
            Vector3 arrowHead = position + forward;
            
            // Draw arrow shaft
            Gizmos.DrawLine(position, arrowHead);
            
            // Draw simple arrow head
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * (size * 0.3f);
            Gizmos.DrawLine(arrowHead, arrowHead - forward * 0.5f + right);
            Gizmos.DrawLine(arrowHead, arrowHead - forward * 0.5f - right);
        }
    }
}