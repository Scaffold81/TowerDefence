using Game.Path;
using UnityEngine;
using Zenject;
using System.Linq;

namespace Game.Services
{
    /// <summary>
    /// Сервис управления камерой.
    /// </summary>
    public class CameraService : ICameraService
    {
        [Inject] private ILevelService levelService;
        
        private Camera currentCamera;
        
        [Inject]
        public void Initialize()
        {
            Debug.Log("[CameraService] Initializing camera service...");
            
            // Находим основную камеру в сцене
            currentCamera = Camera.main;
            if (currentCamera == null)
            {
                Debug.LogWarning("[CameraService] Camera.main not found, searching for any camera...");
                currentCamera = Object.FindFirstObjectByType<Camera>();
            }
            
            if (currentCamera == null)
            {
                Debug.LogError("[CameraService] No camera found in scene!");
                return;
            }
            
            Debug.Log($"[CameraService] Found camera: {currentCamera.name} at position: {currentCamera.transform.position}");
            
            // Подписываемся на события уровня
            if (levelService != null)
            {
                levelService.OnLevelSetupCompleted += OnLevelLoaded;
                Debug.Log("[CameraService] Subscribed to level events");
            }
            else
            {
                Debug.LogError("[CameraService] LevelService is null!");
            }
            
            Debug.Log("[CameraService] Camera service initialized");
        }
        
        /// <summary>
        /// Обработчик загрузки уровня.
        /// </summary>
        private void OnLevelLoaded(string levelId, LevelMap levelMap)
        {
            Debug.Log($"[CameraService] Positioning camera for level: {levelId}");
            PositionCameraForLevel(levelMap);
        }
        
        /// <summary>
        /// Позиционировать камеру для уровня.
        /// </summary>
        public void PositionCameraForLevel(LevelMap levelMap)
        {
            if (currentCamera == null || levelMap == null)
            {
                Debug.LogWarning("[CameraService] Cannot position camera - camera or levelMap is null");
                return;
            }
            
            // Проверяем есть ли CameraTarget на уровне
            var cameraTarget = levelMap.transform.Find("CameraTarget");
            Vector3 center;
            
            if (cameraTarget != null)
            {
                // Используем заданный CameraTarget
                center = cameraTarget.position;
                Debug.Log($"[CameraService] Using CameraTarget position: {center}");
            }
            else
            {
                // Вычисляем центр автоматически через waypoints
                center = CalculateLevelCenter(levelMap);
                Debug.Log($"[CameraService] Calculated level center: {center}");
            }
            
            // Позиционируем камеру с параметрами по умолчанию
            PositionCamera(center, height: 20f, angle: 45f);
        }
        
        /// <summary>
        /// Позиционировать камеру в конкретную точку.
        /// </summary>
        public void PositionCamera(Vector3 center, float height = 20f, float angle = 45f)
        {
            if (currentCamera == null)
            {
                Debug.LogWarning("[CameraService] Cannot position camera - camera is null");
                return;
            }
            
            // Вычисляем позицию камеры для угла 45 градусов
            float distance = height; // При угле 45° высота равна горизонтальному расстоянию
            Vector3 offset = new Vector3(0, height, -distance);
            Vector3 cameraPosition = center + offset;
            
            // Устанавливаем позицию и поворот камеры
            currentCamera.transform.position = cameraPosition;
            currentCamera.transform.LookAt(center);
            
            Debug.Log($"[CameraService] Camera positioned at: {cameraPosition}, looking at: {center}");
        }
        
        /// <summary>
        /// Получить текущую камеру.
        /// </summary>
        public Camera GetCurrentCamera()
        {
            return currentCamera;
        }
        
        /// <summary>
        /// Вычислить центр уровня на основе waypoints.
        /// </summary>
        private Vector3 CalculateLevelCenter(LevelMap levelMap)
        {
            Debug.Log("[CameraService] Starting to calculate level center...");
            
            var waypoints = levelMap.Waypoints.Where(w => w != null).ToList();
            
            Debug.Log($"[CameraService] Found {waypoints.Count} waypoints in LevelMap");
            
            if (waypoints.Count == 0)
            {
                Debug.LogWarning("[CameraService] No waypoints found, using level transform position");
                return levelMap.transform.position;
            }
            
            // Логируем позиции всех waypoints
            for (int i = 0; i < waypoints.Count; i++)
            {
                Debug.Log($"[CameraService] Waypoint {i}: {waypoints[i].name} at {waypoints[i].transform.position}");
            }
            
            // Вычисляем среднее арифметическое позиций всех waypoints
            Vector3 sum = Vector3.zero;
            foreach (var waypoint in waypoints)
            {
                sum += waypoint.transform.position;
            }
            
            Vector3 center = sum / waypoints.Count;
            
            Debug.Log($"[CameraService] Calculated center from {waypoints.Count} waypoints: {center}");
            return center;
        }
    }
}
