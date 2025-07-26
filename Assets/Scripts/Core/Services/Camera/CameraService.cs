using Game.Path;
using UnityEngine;
using Zenject;
using System.Linq;

namespace Game.Services
{
    /// <summary>
    /// Упрощенный сервис управления камерой - только вычисления и координация
    /// </summary>
    public class CameraService : ICameraService
    {
        [Inject] private ILevelService levelService;
        [Inject] private ICameraController cameraController;
        
        [Header("Camera Settings")]
        [SerializeField] private float defaultHeight = 20f;
        [SerializeField] private float defaultAngle = 45f;
        [SerializeField] private float defaultOrthographicSize = 10f;
        [SerializeField] private bool useOrthographicProjection = false;
        
        [Inject]
        public void Initialize()
        {
            Debug.Log("[CameraService] Initializing...");
           
            // Подписываемся на события уровня
            if (levelService != null)
            {
                levelService.OnLevelSetupCompleted += OnLevelLoaded;
                
                // Проверяем текущий уровень
                var currentLevel = levelService.GetCurrentLevelInstance();
                if (currentLevel != null)
                {
                    var levelMap = currentLevel.GetComponent<LevelMap>();
                    if (levelMap != null)
                    {
                        OnLevelLoaded(levelService.GetCurrentLevelId(), levelMap);
                    }
                }
            }
            
            Debug.Log("[CameraService] Initialized");
        }
        
        private void OnLevelLoaded(string levelId, LevelMap levelMap)
        {
            Debug.Log($"[CameraService] Positioning camera for level: {levelId}");
            PositionCameraForLevel(levelMap);
        }
        
        public void PositionCameraForLevel(LevelMap levelMap)
        {
            if (levelMap == null)
            {
                Debug.LogWarning("[CameraService] LevelMap is null");
                return;
            }
            
            var cameraParams = CalculateCameraParamsForLevel(levelMap);
            ApplyCameraParams(cameraParams);
        }
        
        public void PositionCamera(Vector3 center, float height = 20f, float angle = 45f)
        {
            var cameraParams = new CameraParams(center, height, angle, useOrthographicProjection, defaultOrthographicSize);
            ApplyCameraParams(cameraParams);
        }
        
        public Camera GetCurrentCamera()
        {
            return cameraController?.GetCamera();
        }
        
        /// <summary>
        /// Вычислить оптимальные параметры камеры для уровня
        /// </summary>
        private CameraParams CalculateCameraParamsForLevel(LevelMap levelMap)
        {
            // Ищем CameraTarget или вычисляем центр
            var cameraTarget = levelMap.transform.Find("CameraTarget");
            Vector3 center = cameraTarget != null ? cameraTarget.position : CalculateLevelCenter(levelMap);
            
            // Вычисляем оптимальные параметры на основе размера уровня
            var levelBounds = CalculateLevelBounds(levelMap);
            float optimalHeight = CalculateOptimalHeight(levelBounds);
            float optimalSize = CalculateOptimalSize(levelBounds);
            
            Debug.Log($"[CameraService] Camera params - Center: {center}, Height: {optimalHeight}, Size: {optimalSize}");
            
            return new CameraParams(center, optimalHeight, defaultAngle, useOrthographicProjection, optimalSize);
        }
        
        /// <summary>
        /// Применить параметры камеры через контроллер
        /// </summary>
        private void ApplyCameraParams(CameraParams cameraParams)
        {
            if (cameraController == null)
            {
                Debug.LogError("[CameraService] CameraController is null!");
                return;
            }
            
            cameraController.SetIsometricView(
                cameraParams.center, 
                cameraParams.height, 
                cameraParams.angle, 
                cameraParams.useOrthographic, 
                cameraParams.size
            );
        }
        
        private Vector3 CalculateLevelCenter(LevelMap levelMap)
        {
            var waypoints = levelMap.Waypoints.Where(w => w != null).ToList();
            
            if (waypoints.Count == 0)
                return levelMap.transform.position;
            
            Vector3 sum = Vector3.zero;
            foreach (var waypoint in waypoints)
                sum += waypoint.transform.position;
            
            return sum / waypoints.Count;
        }
        
        private Bounds CalculateLevelBounds(LevelMap levelMap)
        {
            var waypoints = levelMap.Waypoints.Where(w => w != null).ToList();
            
            if (waypoints.Count == 0)
                return new Bounds(levelMap.transform.position, Vector3.one * 20f);
            
            Bounds bounds = new Bounds(waypoints[0].transform.position, Vector3.zero);
            foreach (var waypoint in waypoints)
                bounds.Encapsulate(waypoint.transform.position);
            
            bounds.Expand(5f); // Отступ
            return bounds;
        }
        
        private float CalculateOptimalHeight(Bounds levelBounds)
        {
            float maxSize = Mathf.Max(levelBounds.size.x, levelBounds.size.z);
            return Mathf.Max(defaultHeight, maxSize * 0.6f);
        }
        
        private float CalculateOptimalSize(Bounds levelBounds)
        {
            float maxSize = Mathf.Max(levelBounds.size.x, levelBounds.size.z);
            return Mathf.Max(defaultOrthographicSize, maxSize * 0.6f);
        }
    }
}