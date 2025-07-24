using UnityEngine;

namespace Game.Path
{
    /// <summary>
    /// Базовый класс для всех точек пути
    /// </summary>
    public class Waypoint : MonoBehaviour
    {
        [Header("Waypoint Settings")]
        [SerializeField] private int _index;
        [SerializeField] private Color _gizmoColor = Color.blue;
        [SerializeField] private float _gizmoSize = 1f;
        
        [Header("Validation")]
        [SerializeField] private bool _snapToTerrain = true;
        [SerializeField] private LayerMask _terrainLayer = 1;
        
        public int Index
        {
            get => _index;
            set => _index = value;
        }
        
        public Color GizmoColor
        {
            get => _gizmoColor;
            set => _gizmoColor = value;
        }
        
        public float GizmoSize
        {
            get => _gizmoSize;
            set => _gizmoSize = value;
        }
        
        public bool SnapToTerrain
        {
            get => _snapToTerrain;
            set => _snapToTerrain = value;
        }
        
        public LayerMask TerrainLayer
        {
            get => _terrainLayer;
            set => _terrainLayer = value;
        }
        
        /// <summary>
        /// Привязать waypoint к поверхности террейна
        /// </summary>
        public virtual void SnapToTerrainSurface()
        {
            if (!_snapToTerrain) return;
            
            if (Physics.Raycast(transform.position + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f, _terrainLayer))
            {
                transform.position = hit.point;
            }
        }
        
        /// <summary>
        /// Валидация позиции waypoint'а
        /// </summary>
        public virtual bool ValidatePosition(out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Проверяем что waypoint находится на terrain
            if (!Physics.Raycast(transform.position, Vector3.down, 10f, _terrainLayer))
            {
                errorMessage = "Waypoint is not positioned on terrain surface";
                return false;
            }
            
            return true;
        }
        
        protected virtual void OnValidate()
        {
            // Автоматическая привязка к terrain при изменении позиции в Editor
            if (_snapToTerrain && Application.isEditor && !Application.isPlaying)
            {
                SnapToTerrainSurface();
            }
        }
        
        protected virtual void OnDrawGizmos()
        {
            DrawWaypointGizmo(false);
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            DrawWaypointGizmo(true);
        }
        
        protected virtual void DrawWaypointGizmo(bool selected)
        {
            Color color = selected ? Color.white : _gizmoColor;
            color.a = selected ? 1f : 0.7f;
            
            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, _gizmoSize);
            
            // Отображаем индекс waypoint'а
            if (selected)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(transform.position + Vector3.up * (_gizmoSize + 0.5f), _index.ToString());
#endif
            }
        }
    }
}