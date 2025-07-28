using UnityEngine;

namespace Core.Services.Spline
{
    /// <summary>
    /// Represents a designer marker for visual reference and decoration placement
    /// </summary>
    [System.Serializable]
    public struct SplineMarker
    {
        [Header("Position")]
        public Vector3 position;        // Position of the marker
        public Vector3 forward;         // Direction at this marker
        
        [Header("Data")]
        public float distance;          // Distance from start of spline
        public MarkerType type;         // Type of marker
        public int index;               // Sequential index of this marker
        
        /// <summary>
        /// Creates a new SplineMarker
        /// </summary>
        public SplineMarker(Vector3 position, Vector3 forward, float distance, MarkerType type, int index = 0)
        {
            this.position = position;
            this.forward = forward.normalized;
            this.distance = distance;
            this.type = type;
            this.index = index;
        }
        
        /// <summary>
        /// Gets the right direction vector
        /// </summary>
        public Vector3 Right => Vector3.Cross(forward, Vector3.up).normalized;
        
        /// <summary>
        /// Gets rotation for objects placed at this marker
        /// </summary>
        public Quaternion Rotation => Quaternion.LookRotation(forward, Vector3.up);
    }
    
    /// <summary>
    /// Types of markers for different design purposes
    /// </summary>
    public enum MarkerType
    {
        Regular,        // Standard reference points along the path
        SpawnPoint,     // Enemy spawn location
        EndPoint,       // Path destination (player base)
        SharpTurn,      // Sharp curve - good for decorations
        Decoration,     // Suggested decoration placement
        Bridge,         // Bridge/elevated section
        Intersection    // Path intersection (future use)
    }
}