using UnityEngine;

namespace Core.Services.Spline
{
    /// <summary>
    /// Represents a single point on a spline with position, direction and metadata
    /// Used for both movement and designer reference
    /// </summary>
    [System.Serializable]
    public struct SplinePoint
    {
        [Header("Position Data")]
        public Vector3 position;        // World position of the point
        public Vector3 forward;         // Direction of movement at this point
        public Vector3 up;              // Up direction for full orientation
        
        [Header("Path Data")]
        public float distance;          // Distance from start of spline
        public float speedMultiplier;   // Speed modification at this point (always 1.0 - no slowdown)
        
        [Header("Reference Data")]
        public int waypointIndex;       // Index of nearest waypoint
        public float curvature;         // Curvature value at this point (0 = straight, 1 = sharp turn)
        
        /// <summary>
        /// Creates a new SplinePoint with essential data
        /// </summary>
        public SplinePoint(Vector3 position, Vector3 forward, float distance)
        {
            this.position = position;
            this.forward = forward.normalized;
            this.up = Vector3.up;
            this.distance = distance;
            this.speedMultiplier = 1f;
            this.waypointIndex = -1;
            this.curvature = 0f;
        }
        
        /// <summary>
        /// Creates a SplinePoint with full data
        /// </summary>
        public SplinePoint(Vector3 position, Vector3 forward, Vector3 up, float distance, 
                          float speedMultiplier, int waypointIndex, float curvature)
        {
            this.position = position;
            this.forward = forward.normalized;
            this.up = up.normalized;
            this.distance = distance;
            this.speedMultiplier = speedMultiplier;
            this.waypointIndex = waypointIndex;
            this.curvature = curvature;
        }
        
        /// <summary>
        /// Linear interpolation between two SplinePoints
        /// </summary>
        public static SplinePoint Lerp(SplinePoint a, SplinePoint b, float t)
        {
            return new SplinePoint(
                Vector3.Lerp(a.position, b.position, t),
                Vector3.Slerp(a.forward, b.forward, t),
                Vector3.Slerp(a.up, b.up, t),
                Mathf.Lerp(a.distance, b.distance, t),
                Mathf.Lerp(a.speedMultiplier, b.speedMultiplier, t),
                t < 0.5f ? a.waypointIndex : b.waypointIndex,
                Mathf.Lerp(a.curvature, b.curvature, t)
            );
        }
        
        /// <summary>
        /// Gets the right direction vector (cross product of forward and up)
        /// </summary>
        public Vector3 Right => Vector3.Cross(forward, up).normalized;
        
        /// <summary>
        /// Checks if this point represents a sharp turn (high curvature)
        /// </summary>
        public bool IsSharpTurn => curvature > 0.7f;
        
        /// <summary>
        /// Gets rotation that orients an object along the spline
        /// </summary>
        public Quaternion Rotation => Quaternion.LookRotation(forward, up);
    }
}