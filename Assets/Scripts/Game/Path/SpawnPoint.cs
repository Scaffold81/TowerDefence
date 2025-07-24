using UnityEngine;

namespace Game.Path
{
    /// <summary>
    /// Точка появления врагов
    /// </summary>
    public class SpawnPoint : Waypoint
    {
        [Header("Spawn Point Settings")]
        [SerializeField] private Vector3 _spawnDirection = Vector3.forward;
        [SerializeField] private float _spawnRadius = 2f;
        [SerializeField] private int _maxConcurrentEnemies = 10;
        
        public Vector3 SpawnDirection
        {
            get => _spawnDirection.normalized;
            set => _spawnDirection = value.normalized;
        }
        
        public float SpawnRadius
        {
            get => _spawnRadius;
            set => _spawnRadius = Mathf.Max(0f, value);
        }
        
        public int MaxConcurrentEnemies
        {
            get => _maxConcurrentEnemies;
            set => _maxConcurrentEnemies = Mathf.Max(1, value);
        }
        
        /// <summary>
        /// Получить случайную позицию в зоне спавна
        /// </summary>
        public Vector3 GetRandomSpawnPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle * _spawnRadius;
            Vector3 spawnOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
            return transform.position + spawnOffset;
        }
        
        /// <summary>
        /// Получить направление для врага при спавне
        /// </summary>
        public Vector3 GetSpawnDirection()
        {
            return transform.TransformDirection(_spawnDirection);
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Устанавливаем зеленый цвет для spawn point
            if (GizmoColor != Color.green)
            {
                GizmoColor = Color.green;
            }
            
            // Нормализуем направление спавна
            _spawnDirection = _spawnDirection.normalized;
        }
        
        protected override void DrawWaypointGizmo(bool selected)
        {
            base.DrawWaypointGizmo(selected);
            
            // Отображаем зону спавна
            Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.2f);
            Gizmos.DrawSphere(transform.position, _spawnRadius);
            
            // Отображаем направление спавна
            Gizmos.color = selected ? Color.white : GizmoColor;
            Vector3 direction = GetSpawnDirection();
            Vector3 arrowEnd = transform.position + direction * (GizmoSize + 1f);
            Gizmos.DrawLine(transform.position, arrowEnd);
            
            // Стрелка направления
            Vector3 arrowSide1 = Quaternion.AngleAxis(30f, Vector3.up) * -direction * 0.5f;
            Vector3 arrowSide2 = Quaternion.AngleAxis(-30f, Vector3.up) * -direction * 0.5f;
            Gizmos.DrawLine(arrowEnd, arrowEnd + arrowSide1);
            Gizmos.DrawLine(arrowEnd, arrowEnd + arrowSide2);
        }
    }
}