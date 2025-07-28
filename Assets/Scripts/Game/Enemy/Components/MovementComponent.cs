using System.Collections.Generic;
using UnityEngine;
using R3;
using Cysharp.Threading.Tasks;
using Game.Path;
using System;

namespace Game.Enemy.Components
{
    /// <summary>
    /// Компонент движения врага по пути
    /// </summary>
    public class MovementComponent : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _speed = 3f;
        [SerializeField] private float _rotationSpeed = 360f;
        [SerializeField] private float _waypointReachDistance = 0.5f;
        
        private readonly ReactiveProperty<Vector3> _position = new();
        private readonly ReactiveProperty<bool> _isMoving = new(false);
        private readonly ReactiveProperty<Vector3> _movementDirection = new();
        
        private List<Waypoint> _waypoints = new();
        private int _currentWaypointIndex = 0;
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
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        public void Initialize(float speed)
        {
            _speed = speed;
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
            _waypoints = levelMap.Waypoints;
            _currentWaypointIndex = 0;
            _isMoving.Value = true;
            
            // Начинаем движение асинхронно
            MoveAlongPath().Forget();
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
        }
        
        /// <summary>
        /// Основной цикл движения по пути
        /// </summary>
        private async UniTaskVoid MoveAlongPath()
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
        /// Движение к конкретной точке
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
            // Обновляем позицию в реактивном свойстве
            if (_position.Value != transform.position)
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
            if (_waypoints != null && _waypoints.Count > 0 && _currentWaypointIndex < _waypoints.Count)
            {
                Gizmos.color = Color.yellow;
                var targetWaypoint = _waypoints[_currentWaypointIndex];
                if (targetWaypoint != null)
                {
                    Gizmos.DrawLine(transform.position, targetWaypoint.transform.position);
                    Gizmos.DrawWireSphere(targetWaypoint.transform.position, _waypointReachDistance);
                }
            }
        }
    }
}