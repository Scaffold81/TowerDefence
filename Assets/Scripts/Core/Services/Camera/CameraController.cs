using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Контроллер управления камерой - отвечает за физическое позиционирование камеры
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour, ICameraController
    {
        [SerializeField]
        private Camera currentCamera;

        private void Awake()
        {
            GetCamera();
        }

        [Inject]
        public void Initialize()
        {
            Debug.Log("[CameraController] Initializing camera controller...");
            
            
            
            Debug.Log("[CameraController] Camera controller initialized");
        }
        
        /// <summary>
        /// Установить позицию и направление камеры
        /// </summary>
        public void SetCameraTransform(Vector3 position, Vector3 lookAtTarget)
        {
            if (currentCamera == null)
            {
                Debug.LogWarning("[CameraController] Cannot set camera transform - camera is null");
                return;
            }
            
            currentCamera.transform.position = position;
            currentCamera.transform.LookAt(lookAtTarget);
            
            Debug.Log($"[CameraController] Camera positioned at: {position}, looking at: {lookAtTarget}");
        }
        
        /// <summary>
        /// Установить ортографическую проекцию
        /// </summary>
        public void SetOrthographicProjection(float orthographicSize)
        {
            if (currentCamera == null)
            {
                Debug.LogWarning("[CameraController] Cannot set orthographic projection - camera is null");
                return;
            }
            
            currentCamera.orthographic = true;
            currentCamera.orthographicSize = orthographicSize;
            
            Debug.Log($"[CameraController] Set orthographic projection with size: {orthographicSize}");
        }
        
        /// <summary>
        /// Установить перспективную проекцию
        /// </summary>
        public void SetPerspectiveProjection(float fieldOfView)
        {
            if (currentCamera == null)
            {
                Debug.LogWarning("[CameraController] Cannot set perspective projection - camera is null");
                return;
            }
            
            currentCamera.orthographic = false;
            currentCamera.fieldOfView = fieldOfView;
            
            Debug.Log($"[CameraController] Set perspective projection with FOV: {fieldOfView}");
        }
        
        /// <summary>
        /// Получить текущую камеру
        /// </summary>
        public Camera GetCamera()
        {
            Debug.Log("[CameraController] Searching for camera...");
            if(currentCamera == null)
                currentCamera = GetComponent<Camera>();
            Debug.Log($"[CameraController] Found camera: {currentCamera.name} at position: {currentCamera.transform.position}");

            return currentCamera;
        }
        
        /// <summary>
        /// Применить изометрическую настройку камеры
        /// </summary>
        public void ApplyIsometricView(Vector3 centerPoint, float height, float angle, float orthographicSize = 10f)
        {
            if (currentCamera == null)
            {
                Debug.LogWarning("[CameraController] Cannot apply isometric view - camera is null");
                return;
            }
            
            // Вычисляем позицию камеры для изометрического вида
            float angleRad = angle * Mathf.Deg2Rad;
            float distance = height / Mathf.Tan(angleRad);
            Vector3 cameraPosition = centerPoint + new Vector3(0, height, -distance);
            
            // Применяем настройки
            SetCameraTransform(cameraPosition, centerPoint);
            SetOrthographicProjection(orthographicSize);
            
            Debug.Log($"[CameraController] Applied isometric view - Center: {centerPoint}, Height: {height}, Angle: {angle}, OrthographicSize: {orthographicSize}");
        }
        
        /// <summary>
        /// Принудительно переинициализировать камеру
        /// </summary>
        [ContextMenu("Reinitialize Camera")]
        public void ReinitializeCamera()
        {
            Debug.Log("[CameraController] Manual reinitialization requested...");
            GetCamera();
        }
        
        /// <summary>
        /// Получить информацию о текущем состоянии камеры
        /// </summary>
        [ContextMenu("Log Camera Info")]
        public void LogCameraInfo()
        {
            if (currentCamera == null)
            {
                Debug.Log("[CameraController] No camera assigned");
                return;
            }
            
            Debug.Log($"[CameraController] Camera Info:\n" +
                     $"Name: {currentCamera.name}\n" +
                     $"Position: {currentCamera.transform.position}\n" +
                     $"Rotation: {currentCamera.transform.rotation.eulerAngles}\n" +
                     $"Projection: {(currentCamera.orthographic ? "Orthographic" : "Perspective")}\n" +
                     $"Orthographic Size: {currentCamera.orthographicSize}\n" +
                     $"Field of View: {currentCamera.fieldOfView}");
        }
    }
}
