using UnityEngine;

namespace Core.Services.Spline
{
    /// <summary>
    /// Static utility class for Catmull-Rom spline calculations
    /// Provides mathematical functions for generating smooth curves through waypoints
    /// </summary>
    public static class SplineGenerator
    {
        /// <summary>
        /// Evaluates a Catmull-Rom spline at parameter t between points p1 and p2
        /// </summary>
        /// <param name="p0">Point before p1 (for tangent calculation)</param>
        /// <param name="p1">Start point of segment</param>
        /// <param name="p2">End point of segment</param>
        /// <param name="p3">Point after p2 (for tangent calculation)</param>
        /// <param name="t">Parameter from 0 to 1</param>
        /// <returns>Interpolated position on the spline</returns>
        public static Vector3 EvaluateCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            // Catmull-Rom formula:
            // P(t) = 0.5 * [(2 * P1) + (-P0 + P2) * t + (2*P0 - 5*P1 + 4*P2 - P3) * t² + (-P0 + 3*P1 - 3*P2 + P3) * t³]
            
            float t2 = t * t;
            float t3 = t2 * t;
            
            Vector3 a = 2f * p1;
            Vector3 b = (-p0 + p2) * t;
            Vector3 c = (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2;
            Vector3 d = (-p0 + 3f * p1 - 3f * p2 + p3) * t3;
            
            return 0.5f * (a + b + c + d);
        }
        
        /// <summary>
        /// Calculates the first derivative (tangent) of Catmull-Rom spline at parameter t
        /// </summary>
        public static Vector3 EvaluateCatmullRomDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            // First derivative of Catmull-Rom:
            // P'(t) = 0.5 * [(-P0 + P2) + 2 * (2*P0 - 5*P1 + 4*P2 - P3) * t + 3 * (-P0 + 3*P1 - 3*P2 + P3) * t²]
            
            float t2 = t * t;
            
            Vector3 a = -p0 + p2;
            Vector3 b = 2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t;
            Vector3 c = 3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t2;
            
            return 0.5f * (a + b + c);
        }
        
        /// <summary>
        /// Calculates curvature at parameter t (measures how "curved" the path is)
        /// </summary>
        public static float CalculateCurvature(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 firstDerivative = EvaluateCatmullRomDerivative(p0, p1, p2, p3, t);
            Vector3 secondDerivative = EvaluateCatmullRomSecondDerivative(p0, p1, p2, p3, t);
            
            // Curvature = |v' × v''| / |v'|³
            Vector3 cross = Vector3.Cross(firstDerivative, secondDerivative);
            float crossMagnitude = cross.magnitude;
            float velocityMagnitude = firstDerivative.magnitude;
            
            if (velocityMagnitude < 0.001f) return 0f;
            
            return crossMagnitude / (velocityMagnitude * velocityMagnitude * velocityMagnitude);
        }
        
        /// <summary>
        /// Calculates the second derivative of Catmull-Rom spline at parameter t
        /// </summary>
        private static Vector3 EvaluateCatmullRomSecondDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            // Second derivative of Catmull-Rom:
            // P''(t) = 0.5 * [2 * (2*P0 - 5*P1 + 4*P2 - P3) + 6 * (-P0 + 3*P1 - 3*P2 + P3) * t]
            
            Vector3 a = 2f * (2f * p0 - 5f * p1 + 4f * p2 - p3);
            Vector3 b = 6f * (-p0 + 3f * p1 - 3f * p2 + p3) * t;
            
            return 0.5f * (a + b);
        }
        
        /// <summary>
        /// Estimates the arc length of a Catmull-Rom segment using numerical integration
        /// </summary>
        public static float EstimateSegmentLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int samples = 20)
        {
            if (samples < 2) samples = 2;
            
            float totalLength = 0f;
            Vector3 previousPoint = p1; // Start at p1
            
            for (int i = 1; i <= samples; i++)
            {
                float t = (float)i / samples;
                Vector3 currentPoint = EvaluateCatmullRom(p0, p1, p2, p3, t);
                totalLength += Vector3.Distance(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
            
            return totalLength;
        }
        
        /// <summary>
        /// Calculates speed multiplier based on curvature (disabled - always returns 1.0)
        /// </summary>
        public static float CalculateSpeedMultiplier(float curvature, float minSpeed = 0.3f, float maxSpeed = 1f)
        {
            // Замедление на поворотах отключено - всегда возвращаем максимальную скорость
            return maxSpeed;
        }
        
        /// <summary>
        /// Generates boundary control points for first and last segments
        /// </summary>
        public static void GenerateBoundaryPoints(Vector3[] waypoints, out Vector3 startBoundary, out Vector3 endBoundary)
        {
            if (waypoints.Length < 2)
            {
                startBoundary = Vector3.zero;
                endBoundary = Vector3.zero;
                return;
            }
            
            // For first segment: P0 = P1 + (P1 - P2)
            startBoundary = waypoints[0] + (waypoints[0] - waypoints[1]);
            
            // For last segment: P3 = P2 + (P2 - P1)
            int lastIndex = waypoints.Length - 1;
            endBoundary = waypoints[lastIndex] + (waypoints[lastIndex] - waypoints[lastIndex - 1]);
        }
        
        /// <summary>
        /// Samples a single Catmull-Rom segment with the specified resolution
        /// </summary>
        public static SplinePoint[] SampleSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, 
                                                 int resolution, float startDistance, int waypointIndex)
        {
            var points = new SplinePoint[resolution + 1]; // +1 to include the end point
            
            for (int i = 0; i <= resolution; i++)
            {
                float t = (float)i / resolution;
                
                Vector3 position = EvaluateCatmullRom(p0, p1, p2, p3, t);
                Vector3 tangent = EvaluateCatmullRomDerivative(p0, p1, p2, p3, t);
                float curvature = CalculateCurvature(p0, p1, p2, p3, t);
                float speedMultiplier = CalculateSpeedMultiplier(curvature);
                
                // Calculate distance (approximate)
                float segmentLength = EstimateSegmentLength(p0, p1, p2, p3, 20);
                float distance = startDistance + (t * segmentLength);
                
                points[i] = new SplinePoint(
                    position,
                    tangent.normalized,
                    Vector3.up,
                    distance,
                    speedMultiplier,
                    waypointIndex,
                    curvature
                );
            }
            
            return points;
        }
    }
}