using UnityEngine;
using UnityEditor;
using Core.Services.Spline;
using Game.Path;

namespace Editor.Spline
{
    /// <summary>
    /// Provides Scene View visualization for baked splines
    /// Shows spline path, direction arrows, and designer markers
    /// </summary>
    public static class SplineSceneGUI
    {
        private static readonly Color SplinePathColor = Color.green;
        private static readonly Color DirectionArrowColor = Color.blue; 
        private static readonly Color MarkerColor = Color.yellow;
        private static readonly Color SpawnMarkerColor = Color.green;
        private static readonly Color EndMarkerColor = Color.red;
        private static readonly Color SharpTurnMarkerColor = new Color(1f, 0.5f, 0f); // Orange
        
        /// <summary>
        /// Draws spline visualization in Scene View for the specified LevelMap
        /// Call this from OnSceneGUI or OnDrawGizmos
        /// </summary>
        public static void DrawSplineVisualization(LevelMap levelMap)
        {
            if (levelMap == null || !levelMap.UseSplines) return;
            
            var bakedData = levelMap.GetBakedSplineData();
            if (bakedData?.referencePoints == null) return;
            
            DrawSplinePath(bakedData);
            DrawDirectionArrows(bakedData);
            DrawDesignerMarkers(bakedData);
        }
        
        /// <summary>
        /// Draws the main spline path as a series of connected lines
        /// </summary>
        private static void DrawSplinePath(BakedSplineData bakedData)
        {
            if (bakedData.referencePoints.Length < 2) return;
            
            Handles.color = SplinePathColor;
            
            for (int i = 0; i < bakedData.referencePoints.Length - 1; i++)
            {
                Vector3 from = bakedData.referencePoints[i].position;
                Vector3 to = bakedData.referencePoints[i + 1].position;
                
                Handles.DrawLine(from, to);
            }
        }
        
        /// <summary>
        /// Draws direction arrows along the spline path
        /// </summary>
        private static void DrawDirectionArrows(BakedSplineData bakedData)
        {
            if (bakedData.referencePoints.Length < 2) return;
            
            Handles.color = DirectionArrowColor;
            
            // Draw arrows every few points to avoid clutter
            int arrowSpacing = Mathf.Max(1, bakedData.referencePoints.Length / 20);
            
            for (int i = 0; i < bakedData.referencePoints.Length; i += arrowSpacing)
            {
                var point = bakedData.referencePoints[i];
                DrawArrow(point.position, point.forward, 1f);
            }
        }
        
        /// <summary>
        /// Draws designer markers as colored spheres
        /// </summary>
        private static void DrawDesignerMarkers(BakedSplineData bakedData)
        {
            if (bakedData.designerMarkers == null) return;
            
            foreach (var marker in bakedData.designerMarkers)
            {
                // Set color based on marker type
                switch (marker.type)
                {
                    case MarkerType.SpawnPoint:
                        Handles.color = SpawnMarkerColor;
                        break;
                    case MarkerType.EndPoint:
                        Handles.color = EndMarkerColor;
                        break;
                    case MarkerType.SharpTurn:
                        Handles.color = SharpTurnMarkerColor;
                        break;
                    default:
                        Handles.color = MarkerColor;
                        break;
                }
                
                // Draw marker sphere
                float markerSize = GetMarkerSize(marker.type);
                Handles.SphereHandleCap(0, marker.position, Quaternion.identity, markerSize, EventType.Repaint);
                
                // Draw marker label
                DrawMarkerLabel(marker);
            }
        }
        
        /// <summary>
        /// Draws an arrow at the specified position and direction
        /// </summary>
        private static void DrawArrow(Vector3 position, Vector3 direction, float size)
        {
            if (direction.magnitude < 0.001f) return;
            
            Vector3 forward = direction.normalized * size;
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * (size * 0.3f);
            Vector3 up = Vector3.up * (size * 0.3f);
            
            Vector3 arrowHead = position + forward;
            
            // Draw arrow shaft
            Handles.DrawLine(position, arrowHead);
            
            // Draw arrow head
            Handles.DrawLine(arrowHead, arrowHead - forward * 0.3f + right);
            Handles.DrawLine(arrowHead, arrowHead - forward * 0.3f - right);
            Handles.DrawLine(arrowHead, arrowHead - forward * 0.3f + up);
            Handles.DrawLine(arrowHead, arrowHead - forward * 0.3f - up);
        }
        
        /// <summary>
        /// Gets the appropriate size for a marker based on its type
        /// </summary>
        private static float GetMarkerSize(MarkerType markerType)
        {
            switch (markerType)
            {
                case MarkerType.SpawnPoint:
                case MarkerType.EndPoint:
                    return 1.5f;
                case MarkerType.SharpTurn:
                    return 1f;
                default:
                    return 0.8f;
            }
        }
        
        /// <summary>
        /// Draws a text label for a marker
        /// </summary>
        private static void DrawMarkerLabel(SplineMarker marker)
        {
            string labelText = GetMarkerLabelText(marker);
            if (string.IsNullOrEmpty(labelText)) return;
            
            // Position label slightly above the marker
            Vector3 labelPosition = marker.position + Vector3.up * 2f;
            
            // Create GUI style for label
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            
            // Draw label with background
            Handles.Label(labelPosition, labelText, labelStyle);
        }
        
        /// <summary>
        /// Gets appropriate label text for a marker
        /// </summary>
        private static string GetMarkerLabelText(SplineMarker marker)
        {
            switch (marker.type)
            {
                case MarkerType.SpawnPoint:
                    return "SPAWN";
                case MarkerType.EndPoint:
                    return "END";
                case MarkerType.SharpTurn:
                    return "TURN";
                case MarkerType.Decoration:
                    return "DECOR";
                default:
                    return null; // No label for regular markers
            }
        }
        
        /// <summary>
        /// Draws spline debug information in Scene View
        /// </summary>
        public static void DrawSplineDebugInfo(LevelMap levelMap, Vector3 position)
        {
            if (levelMap == null || !levelMap.UseSplines) return;
            
            var bakedData = levelMap.GetBakedSplineData();
            if (bakedData == null) return;
            
            string debugInfo = $"Spline: {bakedData.totalLength:F1}m | " +
                              $"{bakedData.referencePoints?.Length ?? 0} points | " +
                              $"{bakedData.designerMarkers?.Length ?? 0} markers";
            
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                fontSize = 11
            };
            
            Handles.Label(position, debugInfo, style);
        }
    }
}