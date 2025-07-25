using Game.Path;
using UnityEngine;
using Zenject;
using System.Linq;

namespace Game.Services
{
    /// <summary>
    /// Сервис управления камерой - отвечает за расчеты позиций и параметров камеры
    /// </summary>
    public class CameraService : ICameraService
    {
        [Inject] private ILevelService levelService;
        private ICameraController cameraController;
        
        [Header("Camera Calculation Settings")]
        [SerializeField] private float defaultHeight = 20f;
        [SerializeField] private float defaultAngle = 45f;
        [SerializeField] private float defaultOrthographicSize = 10f;
        [SerializeField] private bool useOrthographicProjection = false;
        
        [Inject]
        public void Initialize()
        {
            Debug.Log("[CameraService] Initializing camera service...");
           
            cameraController=MonoBehaviour.FindAnyObjectByType<CameraController>();
            Debug.Log(cameraController);
            // Подписываемся на события уровня
            if (levelService != null)
            {
                levelService.OnLevelSetupCompleted += OnLevelLoaded;
                Debug.Log("[CameraService] Subscribed to level events");
                
                // Проверяем, есть ли уже загруженный уровень
                var currentLevel = levelService.GetCurrentLevelInstance();
                if (currentLevel != null)
                {
                    Debug.Log("[CameraService] Found existing level, positioning camera...");
                    var levelMap = currentLevel.GetComponent<LevelMap>();
                    if (levelMap != null)
                    {
                        OnLevelLoaded(levelService.GetCurrentLevelId(), levelMap);
                    }
                }
            }
            else
            {
                Debug.LogError("[CameraService] LevelService is null!");
            }
            
            Debug.Log("[CameraService] Camera service initialized");
        }
        
        /// <summary>
        /// Обработчик загрузки уровня
        /// </summary>
        private void OnLevelLoaded(string levelId, LevelMap levelMap)
        {
            Debug.Log($"[CameraService] Calculating camera position for level: {levelId}");
            PositionCameraForLevel(levelMap);
        }
        
        /// <summary>
        /// Позиционировать камеру для уровня
        /// </summary>
        public void PositionCameraForLevel(LevelMap levelMap)
        {
            if (levelMap == null)
            {
                Debug.LogWarning("[CameraService] Cannot position camera - levelMap is null");
                return;
            }
            
            // Рассчитываем оптимальные параметры камеры для уровня
            var cameraData = CalculateCameraDataForLevel(levelMap);
            
            // Передаем данные в CameraController для установки
            ApplyCameraData(cameraData);
        }
        
        /// <summary>
        /// Позиционировать камеру в конкретную точку
        /// </summary>
        public void PositionCamera(Vector3 center, float height = 20f, float angle = 45f)
        {
            var cameraData = CalculateCameraData(center, height, angle);
            ApplyCameraData(cameraData);
        }
        
        /// <summary>
        /// Получить текущую камеру
        /// </summary>
        public Camera GetCurrentCamera()
        {
            return cameraController?.GetCamera();
        }
        
        /// <summary>
        /// Рассчитать данные камеры для уровня
        /// </summary>
        private CameraData CalculateCameraDataForLevel(LevelMap levelMap)
        {
            // Проверяем есть ли CameraTarget на уровне
            var cameraTarget = levelMap.transform.Find("CameraTarget");
            Vector3 center;
            
            if (cameraTarget != null)
            {
                center = cameraTarget.position;
                Debug.Log($"[CameraService] Using CameraTarget position: {center}");
            }
            else
            {
                center = CalculateLevelCenter(levelMap);
                Debug.Log($"[CameraService] Calculated level center: {center}");
            }
            
            // Рассчитываем оптимальную высоту и размер на основе размера уровня
            var levelBounds = CalculateLevelBounds(levelMap);
            float optimalHeight = CalculateOptimalHeight(levelBounds);
            float optimalOrthographicSize = CalculateOptimalOrthographicSize(levelBounds);
            
            return CalculateCameraData(center, optimalHeight, defaultAngle, optimalOrthographicSize);
        }
        
        /// <summary>
        /// Рассчитать данные камеры для конкретных параметров
        /// </summary>
        private CameraData CalculateCameraData(Vector3 center, float height, float angle, float orthographicSize = 0f)
        {
            // Конвертируем угол в радианы
            float angleRad = angle * Mathf.Deg2Rad;
            
            // Вычисляем позицию камеры для изометрического вида
            float distance = height / Mathf.Tan(angleRad);
            Vector3 cameraPosition = center + new Vector3(0, height, -distance);
            
            if (orthographicSize <= 0f)
                orthographicSize = defaultOrthographicSize;
            
            return new CameraData
            {
                position = cameraPosition,
                lookAtTarget = center,
                height = height,
                angle = angle,
                orthographicSize = orthographicSize,
                useOrthographic = useOrthographicProjection
            };
        }
        
        /// <summary>
        /// Применить рассчитанные данные камеры через CameraController
        /// </summary>
        private void ApplyCameraData(CameraData data)
        {
            if (cameraController == null)
            {
                Debug.LogError("[CameraService] CameraController is null!");
                return;
            }
            
            Debug.Log($"[CameraService] Applying camera data - Position: {data.position}, LookAt: {data.lookAtTarget}, Height: {data.height}, Angle: {data.angle}");
            
            if (data.useOrthographic)
            {
                cameraController.ApplyIsometricView(data.lookAtTarget, data.height, data.angle, data.orthographicSize);
            }
            else
            {
                cameraController.SetCameraTransform(data.position, data.lookAtTarget);
                cameraController.SetPerspectiveProjection(70f); // Стандартный FOV
            }
        }
        
        /// <summary>
        /// Вычислить центр уровня на основе waypoints
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
        
        /// <summary>
        /// Рассчитать границы уровня
        /// </summary>
        private Bounds CalculateLevelBounds(LevelMap levelMap)
        {
            var waypoints = levelMap.Waypoints.Where(w => w != null).ToList();
            
            if (waypoints.Count == 0)
            {
                return new Bounds(levelMap.transform.position, Vector3.one * 20f);
            }
            
            Bounds bounds = new Bounds(waypoints[0].transform.position, Vector3.zero);
            
            foreach (var waypoint in waypoints)
            {
                bounds.Encapsulate(waypoint.transform.position);
            }
            
            // Добавляем небольшой отступ
            bounds.Expand(5f);
            
            return bounds;
        }
        
        /// <summary>
        /// Рассчитать оптимальную высоту камеры
        /// </summary>
        private float CalculateOptimalHeight(Bounds levelBounds)
        {
            // Берем большую сторону уровня и добавляем коэффициент
            float maxSize = Mathf.Max(levelBounds.size.x, levelBounds.size.z);
            float optimalHeight = Mathf.Max(defaultHeight, maxSize * 0.6f);
            
            Debug.Log($"[CameraService] Level bounds: {levelBounds.size}, Optimal height: {optimalHeight}");
            
            return optimalHeight;
        }
        
        /// <summary>
        /// Рассчитать оптимальный размер ортографической проекции
        /// </summary>
        private float CalculateOptimalOrthographicSize(Bounds levelBounds)
        {
            // Для ортографической проекции берем половину большей стороны
            float maxSize = Mathf.Max(levelBounds.size.x, levelBounds.size.z);
            float optimalSize = Mathf.Max(defaultOrthographicSize, maxSize * 0.6f);
            
            Debug.Log($"[CameraService] Optimal orthographic size: {optimalSize}");
            
            return optimalSize;
        }
    }
    
    /// <summary>
    /// Структура данных для передачи параметров камеры
    /// </summary>
    public struct CameraData
    {
        public Vector3 position;
        public Vector3 lookAtTarget;
        public float height;
        public float angle;
        public float orthographicSize;
        public bool useOrthographic;
    }
}
