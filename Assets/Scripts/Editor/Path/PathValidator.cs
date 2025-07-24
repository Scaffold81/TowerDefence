using UnityEngine;
using UnityEditor;
using Game.Path;
using System.Collections.Generic;
using System.Linq;

namespace Editor.Path
{
    /// <summary>
    /// Инструмент для валидации и анализа путей уровня
    /// </summary>
    public static class PathValidator
    {
        /// <summary>
        /// Провести полную валидацию уровня
        /// </summary>
        public static ValidationResult ValidateLevel(LevelMap levelMap)
        {
            ValidationResult result = new ValidationResult();
            
            if (levelMap == null)
            {
                result.AddError("LevelMap is null");
                return result;
            }
            
            ValidateBasicRequirements(levelMap, result);
            ValidateWaypointPositions(levelMap, result);
            ValidatePathContinuity(levelMap, result);
            ValidateTerrainCompatibility(levelMap, result);
            AnalyzePathQuality(levelMap, result);
            
            return result;
        }
        
        private static void ValidateBasicRequirements(LevelMap levelMap, ValidationResult result)
        {
            // Проверяем наличие terrain
            if (levelMap.Terrain == null)
            {
                result.AddWarning("Terrain is not assigned to LevelMap");
            }
            
            // Проверяем минимальное количество waypoints
            if (levelMap.Waypoints.Count < 2)
            {
                result.AddError("Level must have at least 2 waypoints (SpawnPoint and EndPoint)");
                return;
            }
            
            // Проверяем наличие обязательных waypoints
            var spawnPoints = levelMap.Waypoints.OfType<SpawnPoint>().ToList();
            var endPoints = levelMap.Waypoints.OfType<EndPoint>().ToList();
            
            if (spawnPoints.Count == 0)
            {
                result.AddError("Level must have exactly one SpawnPoint");
            }
            else if (spawnPoints.Count > 1)
            {
                result.AddError($"Level has {spawnPoints.Count} SpawnPoints, but should have exactly 1");
            }
            
            if (endPoints.Count == 0)
            {
                result.AddError("Level must have exactly one EndPoint");
            }
            else if (endPoints.Count > 1)
            {
                result.AddError($"Level has {endPoints.Count} EndPoints, but should have exactly 1");
            }
            
            // Проверяем порядок waypoints
            if (levelMap.Waypoints.Count > 0)
            {
                if (!(levelMap.Waypoints[0] is SpawnPoint))
                {
                    result.AddWarning("First waypoint should be SpawnPoint for optimal path flow");
                }
                
                if (levelMap.Waypoints.Count > 1 && !(levelMap.Waypoints[levelMap.Waypoints.Count - 1] is EndPoint))
                {
                    result.AddWarning("Last waypoint should be EndPoint for optimal path flow");
                }
            }
        }
        
        private static void ValidateWaypointPositions(LevelMap levelMap, ValidationResult result)
        {
            for (int i = 0; i < levelMap.Waypoints.Count; i++)
            {
                Waypoint waypoint = levelMap.Waypoints[i];
                
                if (waypoint == null)
                {
                    result.AddError($"Waypoint at index {i} is null or missing");
                    continue;
                }
                
                // Валидация позиции waypoint
                if (!waypoint.ValidatePosition(out string error))
                {
                    result.AddError($"Waypoint {i} ({waypoint.GetType().Name}): {error}");
                }
                
                // Проверяем что waypoint находится в пределах terrain
                if (levelMap.Terrain != null)
                {
                    Vector3 pos = waypoint.transform.position;
                    TerrainData terrainData = levelMap.Terrain.terrainData;
                    Vector3 terrainPos = levelMap.Terrain.transform.position;
                    
                    if (pos.x < terrainPos.x || pos.x > terrainPos.x + terrainData.size.x ||
                        pos.z < terrainPos.z || pos.z > terrainPos.z + terrainData.size.z)
                    {
                        result.AddWarning($"Waypoint {i} is outside terrain bounds");
                    }
                }
            }
        }
        
        private static void ValidatePathContinuity(LevelMap levelMap, ValidationResult result)
        {
            for (int i = 0; i < levelMap.Waypoints.Count - 1; i++)
            {
                if (levelMap.Waypoints[i] == null || levelMap.Waypoints[i + 1] == null)
                    continue;
                
                Vector3 from = levelMap.Waypoints[i].transform.position;
                Vector3 to = levelMap.Waypoints[i + 1].transform.position;
                float distance = Vector3.Distance(from, to);
                
                // Проверяем минимальное расстояние
                if (distance < 1f)
                {
                    result.AddError($"Waypoints {i} and {i + 1} are too close ({distance:F2}m). Minimum distance is 1m");
                }
                
                // Проверяем максимальное расстояние
                if (distance > 50f)
                {
                    result.AddWarning($"Waypoints {i} and {i + 1} are far apart ({distance:F2}m). Consider adding intermediate waypoints");
                }
                
                // Проверяем препятствия на пути
                if (Physics.Raycast(from + Vector3.up * 0.5f, (to - from).normalized, out RaycastHit hit, distance))
                {
                    if (!hit.collider.GetComponent<Terrain>())
                    {
                        result.AddWarning($"Obstacle detected between waypoints {i} and {i + 1}: {hit.collider.name}");
                    }
                }
            }
        }
        
        private static void ValidateTerrainCompatibility(LevelMap levelMap, ValidationResult result)
        {
            if (levelMap.Terrain == null)
                return;
            
            Terrain terrain = levelMap.Terrain;
            
            for (int i = 0; i < levelMap.Waypoints.Count - 1; i++)
            {
                if (levelMap.Waypoints[i] == null || levelMap.Waypoints[i + 1] == null)
                    continue;
                
                Vector3 from = levelMap.Waypoints[i].transform.position;
                Vector3 to = levelMap.Waypoints[i + 1].transform.position;
                
                // Проверяем наклон между waypoints
                float heightDifference = Mathf.Abs(to.y - from.y);
                float horizontalDistance = Vector3.Distance(new Vector3(from.x, 0, from.z), new Vector3(to.x, 0, to.z));
                
                if (horizontalDistance > 0)
                {
                    float slope = Mathf.Atan(heightDifference / horizontalDistance) * Mathf.Rad2Deg;
                    
                    if (slope > 30f)
                    {
                        result.AddWarning($"Steep slope ({slope:F1}°) between waypoints {i} and {i + 1}. Enemies may have difficulty traversing");
                    }
                    else if (slope > 45f)
                    {
                        result.AddError($"Very steep slope ({slope:F1}°) between waypoints {i} and {i + 1}. Path may be impassable");
                    }
                }
                
                // Проверяем высоту над terrain
                float terrainHeight = terrain.SampleHeight(from);
                if (Mathf.Abs(from.y - terrainHeight) > 2f)
                {
                    result.AddWarning($"Waypoint {i} is {Mathf.Abs(from.y - terrainHeight):F2}m above terrain surface");
                }
            }
        }
        
        private static void AnalyzePathQuality(LevelMap levelMap, ValidationResult result)
        {
            if (levelMap.Waypoints.Count < 2)
                return;
            
            // Анализ общей длины пути
            float totalPathLength = 0f;
            int sharpTurns = 0;
            
            for (int i = 0; i < levelMap.Waypoints.Count - 1; i++)
            {
                if (levelMap.Waypoints[i] == null || levelMap.Waypoints[i + 1] == null)
                    continue;
                
                Vector3 from = levelMap.Waypoints[i].transform.position;
                Vector3 to = levelMap.Waypoints[i + 1].transform.position;
                totalPathLength += Vector3.Distance(from, to);
                
                // Анализ поворотов
                if (i > 0 && i < levelMap.Waypoints.Count - 1)
                {
                    Vector3 prev = levelMap.Waypoints[i - 1].transform.position;
                    Vector3 current = levelMap.Waypoints[i].transform.position;
                    Vector3 next = levelMap.Waypoints[i + 1].transform.position;
                    
                    Vector3 dir1 = (current - prev).normalized;
                    Vector3 dir2 = (next - current).normalized;
                    
                    float angle = Vector3.Angle(dir1, dir2);
                    
                    if (angle > 90f)
                    {
                        sharpTurns++;
                        if (angle > 135f)
                        {
                            result.AddWarning($"Very sharp turn ({angle:F1}°) at waypoint {i}. Consider smoothing the path");
                        }
                    }
                }
            }
            
            // Рекомендации по длине пути
            if (totalPathLength < 20f)
            {
                result.AddInfo("Path is quite short. Consider adding more waypoints for a more interesting route");
            }
            else if (totalPathLength > 200f)
            {
                result.AddInfo("Path is very long. Ensure gameplay balance is maintained");
            }
            
            // Рекомендации по количеству поворотов
            if (sharpTurns > levelMap.Waypoints.Count / 2)
            {
                result.AddInfo("Path has many sharp turns. This may slow down enemy movement significantly");
            }
            
            result.AddInfo($"Path Analysis: Length={totalPathLength:F1}m, Sharp Turns={sharpTurns}, Waypoints={levelMap.Waypoints.Count}");
        }
        
        /// <summary>
        /// Получить рекомендации по размещению башен на основе пути
        /// </summary>
        public static List<Vector3> GetRecommendedTowerPositions(LevelMap levelMap, int maxPositions = 10)
        {
            List<Vector3> positions = new List<Vector3>();
            
            if (levelMap.Waypoints.Count < 2)
                return positions;
            
            // Находим места с наибольшим радиусом покрытия
            for (int i = 0; i < levelMap.Waypoints.Count - 1; i++)
            {
                if (levelMap.Waypoints[i] == null || levelMap.Waypoints[i + 1] == null)
                    continue;
                
                Vector3 from = levelMap.Waypoints[i].transform.position;
                Vector3 to = levelMap.Waypoints[i + 1].transform.position;
                Vector3 midpoint = (from + to) * 0.5f;
                
                // Ищем позиции сбоку от пути
                Vector3 pathDirection = (to - from).normalized;
                Vector3 sideDirection = Vector3.Cross(pathDirection, Vector3.up).normalized;
                
                // Проверяем позиции по обеим сторонам пути
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 towerPos = midpoint + sideDirection * side * 5f;
                    
                    // Проверяем что позиция доступна (нет препятствий)
                    if (!Physics.CheckSphere(towerPos, 1f))
                    {
                        // Привязываем к terrain если он есть
                        if (levelMap.Terrain != null)
                        {
                            float terrainHeight = levelMap.Terrain.SampleHeight(towerPos);
                            towerPos.y = terrainHeight;
                        }
                        
                        positions.Add(towerPos);
                        
                        if (positions.Count >= maxPositions)
                            return positions;
                    }
                }
            }
            
            return positions;
        }
        
        /// <summary>
        /// Анализ времени прохождения пути
        /// </summary>
        public static PathTiming AnalyzePathTiming(LevelMap levelMap, float enemySpeed = 5f)
        {
            PathTiming timing = new PathTiming();
            
            if (levelMap.Waypoints.Count < 2)
                return timing;
            
            float totalDistance = 0f;
            float totalTime = 0f;
            
            for (int i = 0; i < levelMap.Waypoints.Count - 1; i++)
            {
                if (levelMap.Waypoints[i] == null || levelMap.Waypoints[i + 1] == null)
                    continue;
                
                Vector3 from = levelMap.Waypoints[i].transform.position;
                Vector3 to = levelMap.Waypoints[i + 1].transform.position;
                float segmentDistance = Vector3.Distance(from, to);
                
                totalDistance += segmentDistance;
                
                // Учитываем замедление на поворотах
                float segmentSpeed = enemySpeed;
                
                if (i > 0 && i < levelMap.Waypoints.Count - 1)
                {
                    Vector3 prev = levelMap.Waypoints[i - 1].transform.position;
                    Vector3 current = levelMap.Waypoints[i].transform.position;
                    Vector3 next = levelMap.Waypoints[i + 1].transform.position;
                    
                    Vector3 dir1 = (current - prev).normalized;
                    Vector3 dir2 = (next - current).normalized;
                    float angle = Vector3.Angle(dir1, dir2);
                    
                    // Замедление на поворотах
                    if (angle > 45f)
                    {
                        float slowdownFactor = Mathf.Lerp(1f, 0.5f, (angle - 45f) / 90f);
                        segmentSpeed *= slowdownFactor;
                    }
                }
                
                totalTime += segmentDistance / segmentSpeed;
            }
            
            timing.TotalDistance = totalDistance;
            timing.TotalTime = totalTime;
            timing.AverageSpeed = totalDistance / totalTime;
            
            return timing;
        }
    }
    
    /// <summary>
    /// Результат валидации уровня
    /// </summary>
    public class ValidationResult
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
        public List<string> Info { get; } = new List<string>();
        
        public bool IsValid => Errors.Count == 0;
        public bool HasWarnings => Warnings.Count > 0;
        public bool HasInfo => Info.Count > 0;
        
        public void AddError(string message) => Errors.Add(message);
        public void AddWarning(string message) => Warnings.Add(message);
        public void AddInfo(string message) => Info.Add(message);
        
        public void Clear()
        {
            Errors.Clear();
            Warnings.Clear();
            Info.Clear();
        }
        
        public override string ToString()
        {
            var lines = new List<string>();
            
            if (Errors.Count > 0)
            {
                lines.Add($"ERRORS ({Errors.Count}):");
                lines.AddRange(Errors.Select(e => $"  • {e}"));
            }
            
            if (Warnings.Count > 0)
            {
                lines.Add($"WARNINGS ({Warnings.Count}):");
                lines.AddRange(Warnings.Select(w => $"  • {w}"));
            }
            
            if (Info.Count > 0)
            {
                lines.Add($"INFO ({Info.Count}):");
                lines.AddRange(Info.Select(i => $"  • {i}"));
            }
            
            return string.Join("\n", lines);
        }
    }
    
    /// <summary>
    /// Информация о времени прохождения пути
    /// </summary>
    public struct PathTiming
    {
        public float TotalDistance;
        public float TotalTime;
        public float AverageSpeed;
        
        public override string ToString()
        {
            return $"Distance: {TotalDistance:F1}m, Time: {TotalTime:F1}s, Avg Speed: {AverageSpeed:F1}m/s";
        }
    }
}