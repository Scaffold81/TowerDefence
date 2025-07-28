using System.Collections.Generic;
using UnityEngine;
using R3;
using Cysharp.Threading.Tasks;
using Game.Path;
using Core.Services.Spline;
using System;

namespace Game.Enemy.Components
{
    /// <summary>
    /// Компонент движения врага по пути с поддержкой сплайнов
    /// Может использовать как обычные waypoint'ы, так и плавные сплайн-пути
    /// </summary>
    public class MovementComponent : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _speed = 3f;
        [SerializeField] private float _rotationSpeed = 360f;
        [SerializeField] private float _waypointReachDistance = 0.5f;
        
        [Header("Spline Settings")]
        [SerializeField] private bool _useSplineMovement = true;
        [SerializeField] private float _splineUpdateRate = 60f; // Updates per second for spline movement
        
        private readonly ReactiveProperty<Vector3> _position = new();
        private readonly ReactiveProperty<bool> _isMoving = new(false);
        private readonly ReactiveProperty<Vector3> _movementDirection = new();
        
        // Waypoint-based movement
        private List<Waypoint> _waypoints = new();
        private int _currentWaypointIndex = 0;
        
        // Spline-based movement
        private BakedSplineData _splineData;
        private float _distanceTraveled = 0f;
        private float _baseSpeed;
        
        private LevelMap _levelMap;
        private bool _isInitialized = false;
        
        // События
        public System.Action OnReachedEndPoint { get; set; }
        
        public ReactiveProperty<Vector3> Position => _position;
        public ReactiveProperty<bool> IsMoving => _isMoving;
        public ReactiveProperty<Vector3> MovementDirection => _movementDirection;
        
        public float Speed
        {
            get => _speed;
            set => _speed = Mathf.Max(0f, value);
        }
        
        public bool UseSplineMovement
        {
            get => _useSplineMovement;
            set => _useSplineMovement = value;
        }
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        public void Initialize(float speed)
        {
            _speed = speed;
            _baseSpeed = speed;
            _position.Value = transform.position;
            _isInitialized = true;
        }
        
        /// <summary>
        /// Начать движение по пути
        /// </summary>
        public void StartMovement(LevelMap levelMap)
        {
            if (!_isInitialized)
            {
                Debug.LogError($"MovementComponent on {gameObject.name} is not initialized!");
                return;
            }
            
            _levelMap = levelMap;
            _isMoving.Value = true;
            
            // Выбираем тип движения в зависимости от настроек и доступности данных
            if (_useSplineMovement && levelMap.UseSplines)
            {
                _splineData = levelMap.GetBakedSplineData();
                if (_splineData != null && _splineData.isValid)
                {
                    Debug.Log($"[MovementComponent] Using spline movement for {gameObject.name}");
                    StartSplineMovement();
                    return;
                }
                else
                {
                    Debug.LogWarning($"[MovementComponent] Spline data not available for {levelMap.name}, falling back to waypoint movement");
                }
            }
            
            // Fallback to waypoint movement
            Debug.Log($"[MovementComponent] Using waypoint movement for {gameObject.name}");
            StartWaypointMovement();
        }
        
        /// <summary>
        /// Начать движение по сплайну
        /// </summary>
        private void StartSplineMovement()
        {
            _distanceTraveled = 0f;
            MoveAlongSpline().Forget();
        }
        
        /// <summary>
        /// Начать движение по waypoint'ам
        /// </summary>
        private void StartWaypointMovement()
        {
            _waypoints = _levelMap.Waypoints;
            _currentWaypointIndex = 0;
            MoveAlongWaypoints().Forget();
        }
        
        /// <summary>
        /// Основной цикл движения по сплайну
        /// </summary>
        private async UniTaskVoid MoveAlongSpline()
        {
            if (_splineData?.referencePoints == null)
            {
                Debug.LogError($"[MovementComponent] Invalid spline data for {gameObject.name}");
                _isMoving.Value = false;
                return;
            }
            
            float updateInterval = 1f / _splineUpdateRate;
            
            while (_isMoving.Value && _distanceTraveled < _splineData.totalLength)
            {
                // Получаем текущую точку на сплайне
                var splinePoint = _splineData.GetPointAtDistance(_distanceTraveled);
                
                // Обновляем позицию и поворот
                transform.position = splinePoint.position;
                _position.Value = splinePoint.position;
                
                // Плавный поворот в направлении движения
                if (splinePoint.forward.magnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(splinePoint.forward, splinePoint.up);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, 
                        targetRotation, 
                        _rotationSpeed * updateInterval
                    );
                }
                
                // Обновляем направление движения
                _movementDirection.Value = splinePoint.forward;
                
                // Скорость остается постоянной (замедление на поворотах отключено)
                float currentSpeed = _baseSpeed; // splinePoint.speedMultiplier всегда равен 1.0
                
                // Продвигаемся вдоль сплайна
                _distanceTraveled += currentSpeed * updateInterval;
                
                // Ждем до следующего обновления
                await UniTask.Delay(TimeSpan.FromSeconds(updateInterval));
            }
            
            // Достигли конца сплайна
            if (_distanceTraveled >= _splineData.totalLength)
            {
                OnReachedEndOfPath();
            }
        }
        
        /// <summary>
        /// Основной цикл движения по waypoint'ам (старая система)
        /// </summary>
        private async UniTaskVoid MoveAlongWaypoints()
        {
            if (_waypoints == null || _waypoints.Count == 0)
            {
                Debug.LogWarning($"No waypoints found for {gameObject.name}");
                _isMoving.Value = false;
                return;
            }
            
            while (_isMoving.Value && _currentWaypointIndex < _waypoints.Count)
            {
                var targetWaypoint = _waypoints[_currentWaypointIndex];
                if (targetWaypoint == null)
                {
                    _currentWaypointIndex++;
                    continue;
                }
                
                // Движемся к текущей точке
                await MoveToWaypoint(targetWaypoint.transform.position);
                
                // Переходим к следующей точке
                _currentWaypointIndex++;
                
                // Проверяем, не остановили ли нас
                if (!_isMoving.Value)
                    break;
            }
            
            // Достигли конца пути
            if (_currentWaypointIndex >= _waypoints.Count)
            {
                OnReachedEndOfPath();
            }
        }
        
        /// <summary>
        /// Движение к конкретной точке (для waypoint системы)
        /// </summary>
        private async UniTask MoveToWaypoint(Vector3 targetPosition)
        {
            while (_isMoving.Value && Vector3.Distance(transform.position, targetPosition) > _waypointReachDistance)
            {
                // Вычисляем направление
                Vector3 direction = (targetPosition - transform.position).normalized;
                _movementDirection.Value = direction;
                
                // Поворачиваем к цели
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, 
                        targetRotation, 
                        _rotationSpeed * Time.deltaTime
                    );
                }
                
                // Двигаемся к цели
                Vector3 newPosition = transform.position + direction * _speed * Time.deltaTime;
                transform.position = newPosition;
                _position.Value = newPosition;
                
                await UniTask.Yield();
            }
        }
        
        /// <summary>
        /// Остановить движение
        /// </summary>
        public void StopMovement()
        {
            _isMoving.Value = false;
        }
        
        /// <summary>
        /// Установить позицию
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            _position.Value = position;
            
            // Если используем сплайн, найдем ближайшую точку на нем
            if (_useSplineMovement && _splineData != null)
            {
                var nearestPoint = _splineData.GetNearestPoint(position);
                _distanceTraveled = nearestPoint.distance;
            }
        }
        
        /// <summary>
        /// Получить прогресс движения (0-1)
        /// </summary>
        public float GetMovementProgress()
        {
            if (_useSplineMovement && _splineData != null)
            {
                return _splineData.totalLength > 0 ? _distanceTraveled / _splineData.totalLength : 0f;
            }
            else if (_waypoints != null && _waypoints.Count > 0)
            {
                return (float)_currentWaypointIndex / _waypoints.Count;
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Получить оставшееся расстояние до конца пути
        /// </summary>
        public float GetRemainingDistance()
        {
            if (_useSplineMovement && _splineData != null)
            {
                return Mathf.Max(0f, _splineData.totalLength - _distanceTraveled);
            }
            else if (_waypoints != null && _waypoints.Count > 0 && _currentWaypointIndex < _waypoints.Count)
            {
                float distance = Vector3.Distance(transform.position, _waypoints[_currentWaypointIndex].transform.position);
                
                // Добавляем расстояние до остальных waypoint'ов
                for (int i = _currentWaypointIndex; i < _waypoints.Count - 1; i++)
                {
                    if (_waypoints[i] != null && _waypoints[i + 1] != null)
                    {
                        distance += Vector3.Distance(_waypoints[i].transform.position, _waypoints[i + 1].transform.position);
                    }
                }
                
                return distance;
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Вызывается при достижении конца пути
        /// </summary>
        private void OnReachedEndOfPath()
        {
            _isMoving.Value = false;
            
            Debug.Log($"{gameObject.name} reached the end of path!");
            
            // Уведомляем о достижении конечной точки
            OnReachedEndPoint?.Invoke();
        }
        
        private void Update()
        {
            // Обновляем позицию в реактивном свойстве только для waypoint движения
            // Для сплайн движения позиция обновляется в MoveAlongSpline
            if (!_useSplineMovement && _position.Value != transform.position)
            {
                _position.Value = transform.position;
            }
        }
        
        private void OnDestroy()
        {
            _position?.Dispose();
            _isMoving?.Dispose();
            _movementDirection?.Dispose();
        }
        
        // Gizmos для отладки
        private void OnDrawGizmosSelected()
        {
            // Отладка waypoint движения
            if (!_useSplineMovement && _waypoints != null && _waypoints.Count > 0 && _currentWaypointIndex < _waypoints.Count)
            {
                Gizmos.color = Color.yellow;
                var targetWaypoint = _waypoints[_currentWaypointIndex];
                if (targetWaypoint != null)
                {
                    Gizmos.DrawLine(transform.position, targetWaypoint.transform.position);
                    Gizmos.DrawWireSphere(targetWaypoint.transform.position, _waypointReachDistance);
                }
            }
            
            // Отладка сплайн движения
            if (_useSplineMovement && _splineData != null)
            {
                Gizmos.color = Color.green;
                
                // Показываем текущую позицию на сплайне
                var currentPoint = _splineData.GetPointAtDistance(_distanceTraveled);
                Gizmos.DrawWireSphere(currentPoint.position, 0.5f);
                
                // Показываем направление движения
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(currentPoint.position, currentPoint.forward * 2f);
                
                // Показываем прогресс
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                    $"Progress: {GetMovementProgress():P1}\nDistance: {_distanceTraveled:F1}m/{_splineData.totalLength:F1}m");
#endif
            }
        }
        
        // Debug info для Inspector
        [System.Serializable]
        public class DebugInfo
        {
            [ReadOnly] public bool isUsingSplineMovement;
            [ReadOnly] public float distanceTraveled;
            [ReadOnly] public float totalDistance;
            [ReadOnly] public float movementProgress;
            [ReadOnly] public int currentWaypointIndex;
            [ReadOnly] public bool isMoving;
        }
        
        [Header("Debug Info")]
        [SerializeField] private DebugInfo _debugInfo = new DebugInfo();
        
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                _debugInfo.isUsingSplineMovement = _useSplineMovement && _splineData != null;
                _debugInfo.distanceTraveled = _distanceTraveled;
                _debugInfo.totalDistance = _splineData?.totalLength ?? 0f;
                _debugInfo.movementProgress = GetMovementProgress();
                _debugInfo.currentWaypointIndex = _currentWaypointIndex;
                _debugInfo.isMoving = _isMoving?.Value ?? false;
            }
        }
    }
    
    /// <summary>
    /// Attribute for read-only fields in Inspector
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }
}