using UnityEngine;

namespace Game.Path
{
    /// <summary>
    /// Промежуточная точка пути
    /// </summary>
    public class IntermediateWaypoint : Waypoint
    {
        [Header("Intermediate Waypoint Settings")]
        [SerializeField] private TurnType _turnType = TurnType.Smooth;
        [SerializeField] private float _influenceRadius = 2f;
        [SerializeField] private float _smoothingFactor = 0.5f;
        
        public enum TurnType
        {
            Sharp,      // Резкий поворот
            Smooth,     // Плавный поворот
            Custom      // Кастомная настройка
        }
        
        public TurnType Turn
        {
            get => _turnType;
            set => _turnType = value;
        }
        
        public float InfluenceRadius
        {
            get => _influenceRadius;
            set => _influenceRadius = Mathf.Max(0f, value);
        }
        
        public float SmoothingFactor
        {
            get => _smoothingFactor;
            set => _smoothingFactor = Mathf.Clamp01(value);
        }
        
        /// <summary>
        /// Получить фактор влияния для создания сплайна
        /// </summary>
        public float GetInfluenceFactor()
        {
            return _turnType switch
            {
                TurnType.Sharp => 0.1f,
                TurnType.Smooth => 1f,
                TurnType.Custom => _smoothingFactor,
                _ => 0.5f
            };
        }
        
        /// <summary>
        /// Проверить находится ли позиция в зоне влияния waypoint'а
        /// </summary>
        public bool IsPositionInInfluence(Vector3 position)
        {
            float distance = Vector3.Distance(transform.position, position);
            return distance <= _influenceRadius;
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Устанавливаем синий цвет для промежуточных точек
            if (GizmoColor != Color.blue)
            {
                GizmoColor = Color.blue;
            }
        }
        
        protected override void DrawWaypointGizmo(bool selected)
        {
            base.DrawWaypointGizmo(selected);
            
            // Отображаем зону влияния для настройки сплайна
            if (selected)
            {
                Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.1f);
                Gizmos.DrawSphere(transform.position, _influenceRadius);
                
                Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.3f);
                DrawCircle(transform.position, _influenceRadius);
            }
            
            // Визуальная индикация типа поворота
            DrawTurnTypeIndicator(selected);
        }
        
        private void DrawTurnTypeIndicator(bool selected)
        {
            Color indicatorColor = selected ? Color.white : GizmoColor;
            indicatorColor.a = 0.8f;
            Gizmos.color = indicatorColor;
            
            Vector3 basePos = transform.position + Vector3.up * (GizmoSize + 0.2f);
            
            switch (_turnType)
            {
                case TurnType.Sharp:
                    // Рисуем угловую линию для резкого поворота
                    Vector3[] sharpPoints = {
                        basePos + Vector3.left * 0.3f,
                        basePos,
                        basePos + Vector3.right * 0.3f
                    };
                    for (int i = 0; i < sharpPoints.Length - 1; i++)
                        Gizmos.DrawLine(sharpPoints[i], sharpPoints[i + 1]);
                    break;
                    
                case TurnType.Smooth:
                    // Рисуем кривую для плавного поворота
                    DrawSmoothCurve(basePos);
                    break;
                    
                case TurnType.Custom:
                    // Рисуем комбинированную индикацию
                    DrawCustomIndicator(basePos);
                    break;
            }
        }
        
        private void DrawSmoothCurve(Vector3 center)
        {
            const int segments = 8;
            Vector3 startPoint = center + Vector3.left * 0.3f;
            
            for (int i = 0; i < segments; i++)
            {
                float t1 = (float)i / segments;
                float t2 = (float)(i + 1) / segments;
                
                Vector3 p1 = Vector3.Lerp(startPoint, center + Vector3.right * 0.3f, t1);
                p1.y += Mathf.Sin(t1 * Mathf.PI) * 0.2f;
                
                Vector3 p2 = Vector3.Lerp(startPoint, center + Vector3.right * 0.3f, t2);
                p2.y += Mathf.Sin(t2 * Mathf.PI) * 0.2f;
                
                Gizmos.DrawLine(p1, p2);
            }
        }
        
        private void DrawCustomIndicator(Vector3 center)
        {
            // Комбинация угловой и кривой линии
            float factor = _smoothingFactor;
            
            if (factor < 0.5f)
            {
                // Больше угловая
                Vector3[] points = {
                    center + Vector3.left * 0.3f,
                    center + Vector3.up * (factor * 0.4f),
                    center + Vector3.right * 0.3f
                };
                for (int i = 0; i < points.Length - 1; i++)
                    Gizmos.DrawLine(points[i], points[i + 1]);
            }
            else
            {
                // Больше плавная
                DrawSmoothCurve(center);
            }
        }
        
        private void DrawCircle(Vector3 center, float radius)
        {
            const int segments = 16;
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