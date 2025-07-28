using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Game.Path;

namespace Core.Services.Spline
{
    /// <summary>
    /// Main spline system implementation
    /// Generates Catmull-Rom splines from waypoints for tower defense paths
    /// </summary>
    public class SplineSystem : ISplineSystem
    {
        private const string SYSTEM_VERSION = "1.0.0";
        
        public string GetSystemVersion() => SYSTEM_VERSION;
        
        /// <summary>
        /// Generates complete spline data from LevelMap waypoints
        /// </summary>
        public BakedSplineData GenerateSplineData(LevelMap levelMap, SplineSettings settings)
        {
            if (levelMap == null)
            {
                Debug.LogError("[SplineSystem] LevelMap is null");
                return new BakedSplineData();
            }
            
            // Extract waypoint positions
            var waypoints = ExtractWaypointsFromLevelMap(levelMap);
            if (waypoints.Length < 2)
            {
                Debug.LogError($"[SplineSystem] Need at least 2 waypoints, got {waypoints.Length}");
                return new BakedSplineData();
            }
            
            Debug.Log($"[SplineSystem] Generating spline from {waypoints.Length} waypoints");
            return GenerateSplineData(waypoints, settings);
        }
        
        /// <summary>
        /// Generates spline data from array of world positions
        /// </summary>
        public BakedSplineData GenerateSplineData(Vector3[] waypoints, SplineSettings settings)
        {
            var bakedData = new BakedSplineData();
            
            try
            {
                // Validate input
                var validation = ValidateWaypoints(waypoints);
                if (!validation.isValid)
                {
                    Debug.LogError($"[SplineSystem] Waypoint validation failed: {string.Join(", ", validation.errors)}");
                    return bakedData;
                }
                
                // Log warnings if any
                if (validation.HasWarnings)
                {
                    Debug.LogWarning($"[SplineSystem] Validation warnings: {string.Join(", ", validation.warnings)}");
                }
                
                // Generate control points for boundary conditions
                SplineGenerator.GenerateBoundaryPoints(waypoints, out Vector3 startBoundary, out Vector3 endBoundary);
                
                // Create extended waypoints array with boundary points
                var extendedWaypoints = new Vector3[waypoints.Length + 2];
                extendedWaypoints[0] = startBoundary;
                System.Array.Copy(waypoints, 0, extendedWaypoints, 1, waypoints.Length);
                extendedWaypoints[extendedWaypoints.Length - 1] = endBoundary;
                
                // Generate spline segments
                var allPoints = new List<SplinePoint>();
                float totalLength = 0f;
                int segmentCount = waypoints.Length - 1;
                
                for (int i = 0; i < segmentCount; i++)
                {
                    var p0 = extendedWaypoints[i];     // Previous point (or boundary)
                    var p1 = extendedWaypoints[i + 1]; // Start of segment
                    var p2 = extendedWaypoints[i + 2]; // End of segment
                    var p3 = extendedWaypoints[i + 3]; // Next point (or boundary)
                    
                    // Sample this segment
                    var segmentPoints = SplineGenerator.SampleSegment(
                        p0, p1, p2, p3, 
                        settings.GetResolutionPerSegment(), 
                        totalLength, 
                        i
                    );
                    
                    // Add points (skip last point to avoid duplication, except for final segment)
                    int pointsToAdd = (i == segmentCount - 1) ? segmentPoints.Length : segmentPoints.Length - 1;
                    for (int j = 0; j < pointsToAdd; j++)
                    {
                        allPoints.Add(segmentPoints[j]);
                    }
                    
                    // Update total length
                    if (segmentPoints.Length > 0)
                        totalLength = segmentPoints[segmentPoints.Length - 1].distance;
                }
                
                // Resample for uniform spacing
                var uniformPoints = ResampleForUniformSpacing(allPoints.ToArray(), settings.sampleDistance);
                
                // Update distances in uniform points
                UpdateDistances(uniformPoints);
                
                // Generate designer markers
                var markers = GenerateDesignerMarkers(uniformPoints, settings);
                
                // Calculate statistics
                var statistics = CalculateStatistics(uniformPoints, waypoints);
                
                // Populate baked data
                bakedData.referencePoints = uniformPoints;
                bakedData.totalLength = totalLength;
                bakedData.sampleDistance = settings.sampleDistance;
                bakedData.designerMarkers = markers;
                bakedData.markerSpacing = settings.markerSpacing;
                bakedData.segmentCount = segmentCount;
                bakedData.averageCurvature = statistics.averageCurvature;
                bakedData.pathBounds = statistics.bounds;
                bakedData.waypointHash = CalculateWaypointHash(waypoints);
                bakedData.MarkAsValid(bakedData.waypointHash, SYSTEM_VERSION);
                
                Debug.Log($"[SplineSystem] Generated spline: {uniformPoints.Length} points, " +
                         $"{totalLength:F1}m length, {markers.Length} markers");
                
                return bakedData;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SplineSystem] Error generating spline: {ex.Message}\n{ex.StackTrace}");
                return new BakedSplineData();
            }
        }
        
        /// <summary>
        /// Validates waypoints for spline generation
        /// </summary>
        public SplineValidationResult ValidateWaypoints(Vector3[] waypoints)
        {
            var warnings = new List<string>();
            var errors = new List<string>();
            
            // Basic validation
            if (waypoints == null)
            {
                errors.Add("Waypoints array is null");
                return new SplineValidationResult { isValid = false, errors = errors.ToArray() };
            }
            
            if (waypoints.Length < 2)
            {
                errors.Add($"Need at least 2 waypoints, got {waypoints.Length}");
                return new SplineValidationResult { isValid = false, errors = errors.ToArray() };
            }
            
            // Check for duplicate consecutive points
            for (int i = 1; i < waypoints.Length; i++)
            {
                float distance = Vector3.Distance(waypoints[i - 1], waypoints[i]);
                if (distance < 0.1f)
                {
                    warnings.Add($"Very close waypoints at indices {i-1} and {i} (distance: {distance:F3}m)");
                }
                else if (distance < 0.01f)
                {
                    errors.Add($"Duplicate waypoints at indices {i-1} and {i}");
                }
            }
            
            // Check for extremely long segments
            for (int i = 1; i < waypoints.Length; i++)
            {
                float distance = Vector3.Distance(waypoints[i - 1], waypoints[i]);
                if (distance > 100f)
                {
                    warnings.Add($"Very long segment between waypoints {i-1} and {i} (distance: {distance:F1}m)");
                }
            }
            
            // Check for reasonable Y coordinates (basic terrain check)
            foreach (var waypoint in waypoints)
            {
                if (waypoint.y < -1000f || waypoint.y > 1000f)
                {
                    warnings.Add($"Waypoint has extreme Y coordinate: {waypoint.y:F1}");
                }
            }
            
            bool isValid = errors.Count == 0;
            return new SplineValidationResult
            {
                isValid = isValid,
                warnings = warnings.ToArray(),
                errors = errors.ToArray()
            };
        }
        
        /// <summary>
        /// Calculates hash of waypoint positions for change detection
        /// </summary>
        public string CalculateWaypointHash(Vector3[] waypoints)
        {
            if (waypoints == null || waypoints.Length == 0)
                return "";
            
            using (var sha256 = SHA256.Create())
            {
                var sb = new StringBuilder();
                foreach (var waypoint in waypoints)
                {
                    sb.Append($"{waypoint.x:F3},{waypoint.y:F3},{waypoint.z:F3};");
                }
                
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                return System.Convert.ToBase64String(hashBytes).Substring(0, 16); // Truncate for readability
            }
        }
        
        /// <summary>
        /// Extracts waypoint positions from LevelMap in correct order
        /// </summary>
        private Vector3[] ExtractWaypointsFromLevelMap(LevelMap levelMap)
        {
            var waypoints = levelMap.GetWaypoints();
            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogError("[SplineSystem] No waypoints found in LevelMap");
                return new Vector3[0];
            }
            
            // Sort by index to ensure correct order
            var sortedWaypoints = waypoints.OrderBy(w => w.Index).ToArray();
            
            // Extract positions
            var positions = new Vector3[sortedWaypoints.Length];
            for (int i = 0; i < sortedWaypoints.Length; i++)
            {
                positions[i] = sortedWaypoints[i].transform.position;
            }
            
            return positions;
        }
        
        /// <summary>
        /// Resamples spline points for uniform spacing
        /// </summary>
        private SplinePoint[] ResampleForUniformSpacing(SplinePoint[] originalPoints, float spacing)
        {
            if (originalPoints.Length < 2) return originalPoints;
            
            var resampledPoints = new List<SplinePoint>();
            float totalLength = originalPoints[originalPoints.Length - 1].distance;
            int targetPointCount = Mathf.Max(2, Mathf.CeilToInt(totalLength / spacing) + 1);
            
            for (int i = 0; i < targetPointCount; i++)
            {
                float targetDistance = i * spacing;
                if (targetDistance > totalLength) targetDistance = totalLength;
                
                var point = InterpolateAtDistance(originalPoints, targetDistance);
                resampledPoints.Add(point);
                
                if (targetDistance >= totalLength) break;
            }
            
            return resampledPoints.ToArray();
        }
        
        /// <summary>
        /// Interpolates a point at the specified distance along the original sample points
        /// </summary>
        private SplinePoint InterpolateAtDistance(SplinePoint[] points, float targetDistance)
        {
            // Find the segment containing this distance
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (points[i].distance <= targetDistance && points[i + 1].distance >= targetDistance)
                {
                    float segmentLength = points[i + 1].distance - points[i].distance;
                    if (segmentLength <= 0f) return points[i];
                    
                    float t = (targetDistance - points[i].distance) / segmentLength;
                    return SplinePoint.Lerp(points[i], points[i + 1], t);
                }
            }
            
            // If not found, return closest point
            return targetDistance <= points[0].distance ? points[0] : points[points.Length - 1];
        }
        
        /// <summary>
        /// Updates distance values in uniformly spaced points
        /// </summary>
        private void UpdateDistances(SplinePoint[] points)
        {
            if (points.Length == 0) return;
            
            float accumulatedDistance = 0f;
            points[0] = new SplinePoint(
                points[0].position, points[0].forward, points[0].up,
                0f, points[0].speedMultiplier, points[0].waypointIndex, points[0].curvature
            );
            
            for (int i = 1; i < points.Length; i++)
            {
                accumulatedDistance += Vector3.Distance(points[i - 1].position, points[i].position);
                points[i] = new SplinePoint(
                    points[i].position, points[i].forward, points[i].up,
                    accumulatedDistance, points[i].speedMultiplier, points[i].waypointIndex, points[i].curvature
                );
            }
        }
        
        /// <summary>
        /// Generates designer markers for visual reference
        /// </summary>
        private SplineMarker[] GenerateDesignerMarkers(SplinePoint[] points, SplineSettings settings)
        {
            if (points.Length == 0) return new SplineMarker[0];
            
            var markers = new List<SplineMarker>();
            float totalLength = points[points.Length - 1].distance;
            
            // Add start marker
            markers.Add(new SplineMarker(
                points[0].position, 
                points[0].forward, 
                0f, 
                MarkerType.SpawnPoint, 
                0
            ));
            
            // Add regular markers
            int markerIndex = 1;
            for (float distance = settings.markerSpacing; distance < totalLength; distance += settings.markerSpacing)
            {
                var point = InterpolateAtDistance(points, distance);
                
                MarkerType markerType = MarkerType.Regular;
                if (settings.autoMarkSharpTurns && point.IsSharpTurn)
                {
                    markerType = MarkerType.SharpTurn;
                }
                
                markers.Add(new SplineMarker(
                    point.position,
                    point.forward,
                    distance,
                    markerType,
                    markerIndex++
                ));
            }
            
            // Add end marker
            var lastPoint = points[points.Length - 1];
            markers.Add(new SplineMarker(
                lastPoint.position,
                lastPoint.forward,
                totalLength,
                MarkerType.EndPoint,
                markerIndex
            ));
            
            return markers.ToArray();
        }
        
        /// <summary>
        /// Calculates statistical information about the spline
        /// </summary>
        private (float averageCurvature, Bounds bounds) CalculateStatistics(SplinePoint[] points, Vector3[] originalWaypoints)
        {
            if (points.Length == 0)
                return (0f, new Bounds());
            
            // Calculate average curvature
            float totalCurvature = 0f;
            foreach (var point in points)
            {
                totalCurvature += point.curvature;
            }
            float averageCurvature = totalCurvature / points.Length;
            
            // Calculate bounds
            var bounds = new Bounds(points[0].position, Vector3.zero);
            foreach (var point in points)
            {
                bounds.Encapsulate(point.position);
            }
            
            return (averageCurvature, bounds);
        }
    }
}