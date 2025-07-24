using UnityEngine;

namespace Game.Path
{
    /// <summary>
    /// Конечная точка пути - база игрока
    /// </summary>
    public class EndPoint : Waypoint
    {
        [Header("End Point Settings")]
        [SerializeField] private float _baseRadius = 3f;
        [SerializeField] private float _damageZoneRadius = 1f;
        [SerializeField] private int _baseHealth = 100;
        
        public float BaseRadius
        {
            get => _baseRadius;
            set => _baseRadius = Mathf.Max(0f, value);
        }
        
        public float DamageZoneRadius
        {
            get => _damageZoneRadius;
            set => _damageZoneRadius = Mathf.Max(0f, value);
        }
        
        public int BaseHealth
        {
            get => _baseHealth;
            set => _baseHealth = Mathf.Max(1, value);
        }
        
        /// <summary>
        /// Проверить попал ли враг в зону поражения базы
        /// </summary>
        public bool IsEnemyInDamageZone(Vector3 enemyPosition)
        {
            float distance = Vector3.Distance(transform.position, enemyPosition);
            return distance <= _damageZoneRadius;
        }
        
        /// <summary>
        /// Проверить находится ли позиция в области базы
        /// </summary>
        public bool IsPositionInBase(Vector3 position)
        {
            float distance = Vector3.Distance(transform.position, position);
            return distance <= _baseRadius;
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Устанавливаем красный цвет для end point
            if (GizmoColor != Color.red)
            {
                GizmoColor = Color.red;
            }
            
            // Зона поражения не может быть больше радиуса базы
            if (_damageZoneRadius > _baseRadius)
            {
                _damageZoneRadius = _baseRadius;
            }
        }
        
        protected override void DrawWaypointGizmo(bool selected)
        {
            base.DrawWaypointGizmo(selected);
            
            // Отображаем область базы
            Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.1f);
            Gizmos.DrawSphere(transform.position, _baseRadius);
            
            // Отображаем зону поражения
            Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.3f);
            Gizmos.DrawSphere(transform.position, _damageZoneRadius);
            
            // Контуры зон
            Gizmos.color = selected ? Color.white : GizmoColor;
            DrawCircle(transform.position, _baseRadius);
            
            Gizmos.color = new Color(1f, 0.5f, 0f, selected ? 1f : 0.7f);
            DrawCircle(transform.position, _damageZoneRadius);
        }
        
        private void DrawCircle(Vector3 center, float radius)
        {
            const int segments = 32;
            float angleStep = 360f / segments;
            
            Vector3 prevPoint = center + Vector3.forward * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep;
                Vector3 currentPoint = center + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * radius;
                Gizmos.DrawLine(prevPoint, currentPoint);
                prevPoint = currentPoint;
            }
        }
    }
}