using Game.Path;

namespace Core.Services.Spline
{
    /// <summary>
    /// Interface for spline system service
    /// Provides spline generation and management functionality
    /// </summary>
    public interface ISplineSystem
    {
        /// <summary>
        /// Generates a complete spline path from LevelMap waypoints
        /// </summary>
        BakedSplineData GenerateSplineData(LevelMap levelMap, SplineSettings settings);
        
        /// <summary>
        /// Generates spline data from array of world positions
        /// </summary>
        BakedSplineData GenerateSplineData(UnityEngine.Vector3[] waypoints, SplineSettings settings);
        
        /// <summary>
        /// Validates if waypoints can create a valid spline
        /// </summary>
        SplineValidationResult ValidateWaypoints(UnityEngine.Vector3[] waypoints);
        
        /// <summary>
        /// Calculates hash of waypoint positions for change detection
        /// </summary>
        string CalculateWaypointHash(UnityEngine.Vector3[] waypoints);
        
        /// <summary>
        /// Gets current system version for bake tracking
        /// </summary>
        string GetSystemVersion();
    }
    
    /// <summary>
    /// Result of waypoint validation
    /// </summary>
    [System.Serializable]
    public struct SplineValidationResult
    {
        public bool isValid;
        public string[] warnings;
        public string[] errors;
        
        public bool HasWarnings => warnings != null && warnings.Length > 0;
        public bool HasErrors => errors != null && errors.Length > 0;
    }
}