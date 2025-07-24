using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        }
        
        /// <summary>
        /// Очистить все waypoints
        /// </summary>
        public void ClearWaypoints()
        {
            _waypoints.Clear();
            _validationErrors.Clear();
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
        }
        
        private void OnValidate()
        {
            if (_autoValidate && Application.isEditor && !Application.isPlaying)
            {
                ValidateLevel();
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!_showPath && !_showConnections && !_showWaypoints)
                return;
            
            DrawPathVisualization();
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
    }
}