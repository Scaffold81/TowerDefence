using UnityEngine;
using System.Collections.Generic;

namespace Core.Services.Spline
{
    /// <summary>
    /// Contains baked spline data for designer reference and runtime movement
    /// Stored as serialized data in LevelMap component
    /// </summary>
    [System.Serializable]
    public class BakedSplineData
    {
        [Header("Spline Reference Data")]
        [Tooltip("Dense sampling of spline points for smooth movement")]
        public SplinePoint[] referencePoints = new SplinePoint[0];
        
        [Tooltip("Total length of the spline path in meters")]
        public float totalLength = 0f;
        
        [Tooltip("Distance between reference points in meters")]
        public float sampleDistance = 0.5f;
        
        [Header("Designer Tools")]
        [Tooltip("Markers for visual reference and decoration placement")]
        public SplineMarker[] designerMarkers = new SplineMarker[0];
        
        [Tooltip("Spacing between designer markers in meters")]
        public float markerSpacing = 2f;
        
        [Header("Validation")]
        [Tooltip("Whether the baked data is valid and up-to-date")]
        public bool isValid = false;
        
        [Tooltip("Version string for tracking bake updates")]
        public string bakedVersion = "";
        
        [Tooltip("Hash of source waypoints to detect changes")]
        public string waypointHash = "";
        
        [Header("Statistics")]
        [Tooltip("Number of original waypoint segments")]
        public int segmentCount = 0;
        
        [Tooltip("Average curvature of the path (0=straight, 1=very curved)")]
        public float averageCurvature = 0f;
        
        [Tooltip("Bounding box of the entire path")]
        public Bounds pathBounds = new Bounds();
        
        /// <summary>
        /// Gets a spline point at the specified distance along the path
        /// Uses linear interpolation between reference points for smooth results
        /// </summary>
        public SplinePoint GetPointAtDistance(float distance)
        {
            if (referencePoints == null || referencePoints.Length == 0)
            {
                Debug.LogWarning("No reference points available in baked spline data");
                return new SplinePoint();
            }
            
            // Clamp distance to valid range
            distance = Mathf.Clamp(distance, 0f, totalLength);
            
            // Handle edge cases
            if (distance <= 0f) return referencePoints[0];
            if (distance >= totalLength) return referencePoints[referencePoints.Length - 1];
            
            // Find the segment containing this distance
            int segmentIndex = FindSegmentIndex(distance);
            if (segmentIndex >= referencePoints.Length - 1)
                return referencePoints[referencePoints.Length - 1];
            
            // Interpolate between the two points
            var pointA = referencePoints[segmentIndex];
            var pointB = referencePoints[segmentIndex + 1];
            
            float segmentStart = pointA.distance;
            float segmentEnd = pointB.distance;
            float segmentLength = segmentEnd - segmentStart;
            
            if (segmentLength <= 0f) return pointA;
            
            float t = (distance - segmentStart) / segmentLength;
            return SplinePoint.Lerp(pointA, pointB, t);
        }
        
        /// <summary>
        /// Gets a spline point at normalized position (0-1) along the path
        /// </summary>
        public SplinePoint GetPointAtNormalizedDistance(float normalizedDistance)
        {
            normalizedDistance = Mathf.Clamp01(normalizedDistance);
            return GetPointAtDistance(normalizedDistance * totalLength);
        }
        
        /// <summary>
        /// Finds the nearest spline point to a world position
        /// Useful for snapping objects to the path
        /// </summary>
        public SplinePoint GetNearestPoint(Vector3 worldPosition)
        {
            if (referencePoints == null || referencePoints.Length == 0)
                return new SplinePoint();
            
            var nearestPoint = referencePoints[0];
            float nearestDistanceSqr = Vector3.SqrMagnitude(worldPosition - nearestPoint.position);
            
            foreach (var point in referencePoints)
            {
                float distanceSqr = Vector3.SqrMagnitude(worldPosition - point.position);
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestPoint = point;
                }
            }
            
            return nearestPoint;
        }
        
        /// <summary>
        /// Gets all designer markers of a specific type
        /// </summary>
        public SplineMarker[] GetMarkersByType(MarkerType markerType)
        {
            if (designerMarkers == null) return new SplineMarker[0];
            
            var result = new List<SplineMarker>();
            foreach (var marker in designerMarkers)
            {
                if (marker.type == markerType)
                    result.Add(marker);
            }
            return result.ToArray();
        }
        
        /// <summary>
        /// Checks if the baked data needs to be regenerated
        /// </summary>
        public bool NeedsRebaking(string currentWaypointHash)
        {
            return !isValid || 
                   string.IsNullOrEmpty(waypointHash) || 
                   waypointHash != currentWaypointHash ||
                   string.IsNullOrEmpty(bakedVersion);
        }
        
        /// <summary>
        /// Marks the data as valid with current version info
        /// </summary>
        public void MarkAsValid(string currentWaypointHash, string version)
        {
            isValid = true;
            waypointHash = currentWaypointHash;
            bakedVersion = version;
        }
        
        /// <summary>
        /// Invalidates the baked data (call when waypoints change)
        /// </summary>
        public void Invalidate()
        {
            isValid = false;
            waypointHash = "";
            bakedVersion = "";
        }
        
        /// <summary>
        /// Binary search to find the segment containing the specified distance
        /// </summary>
        private int FindSegmentIndex(float distance)
        {
            int left = 0;
            int right = referencePoints.Length - 1;
            
            while (left < right)
            {
                int mid = (left + right) / 2;
                if (referencePoints[mid].distance < distance)
                    left = mid + 1;
                else
                    right = mid;
            }
            
            return Mathf.Max(0, left - 1);
        }
        
        /// <summary>
        /// Gets debug info string for Inspector display
        /// </summary>
        public string GetDebugInfo()
        {
            if (!isValid) return "❌ Invalid - needs rebaking";
            
            return $"✅ Valid | Length: {totalLength:F1}m | Points: {referencePoints?.Length ?? 0} | " +
                   $"Markers: {designerMarkers?.Length ?? 0} | Curves: {averageCurvature:F2}";
        }
    }
}