using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Упрощенный контроллер камеры - только применение настроек
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour, ICameraController
    {
        [SerializeField]
        private Camera currentCamera;

        [Inject]
        public void Initialize()
        {
            GetCamera();
            Debug.Log("[CameraController] Initialized");
        }
        
        /// <summary>
        /// Установить изометрический вид с автоматическими вычислениями
        /// </summary>
        public void SetIsometricView(Vector3 centerPoint, float height, float angle, bool useOrthographic = true, float orthographicSize = 10f)
        {
            if (currentCamera == null)
            {
                Debug.LogWarning("[CameraController] Camera is null");
                return;
            }
            
            // Вычисляем позицию камеры для изометрического вида
            Vector3 cameraPosition = CalculateIsometricPosition(centerPoint, height, angle);
            
            // Применяем трансформ
            SetTransform(cameraPosition, centerPoint);
            
            // Устанавливаем проекцию
            SetProjection(useOrthographic, orthographicSize);
            
            Debug.Log($"[CameraController] Applied isometric view - Center: {centerPoint}, Height: {height}, Angle: {angle}");
        }
        
        /// <summary>
        /// Установить позицию и направление камеры
        /// </summary>
        public void SetTransform(Vector3 position, Vector3 lookAtTarget)
        {
            if (currentCamera == null) return;
            
            currentCamera.transform.position = position;
            currentCamera.transform.LookAt(lookAtTarget);
        }
        
        /// <summary>
        /// Переключить тип проекции
        /// </summary>
        public void SetProjection(bool orthographic, float size = 10f, float fov = 70f)
        {
            if (currentCamera == null) return;
            
            currentCamera.orthographic = orthographic;
            
            if (orthographic)
                currentCamera.orthographicSize = size;
            else
                currentCamera.fieldOfView = fov;
        }
        
        /// <summary>
        /// Получить камеру
        /// </summary>
        public Camera GetCamera()
        {
            if (currentCamera == null)
                currentCamera = GetComponent<Camera>();
            
            return currentCamera;
        }
        
        /// <summary>
        /// Вычислить позицию камеры для изометрического вида
        /// </summary>
        private Vector3 CalculateIsometricPosition(Vector3 centerPoint, float height, float angle)
        {
            float angleRad = angle * Mathf.Deg2Rad;
            float distance = height / Mathf.Tan(angleRad);
            return centerPoint + new Vector3(0, height, -distance);
        }
        
        [ContextMenu("Log Camera Info")]
        public void LogCameraInfo()
        {
            if (currentCamera == null)
            {
                Debug.Log("[CameraController] No camera assigned");
                return;
            }
            
            Debug.Log($"[CameraController] Camera Info:\n" +
                     $"Position: {currentCamera.transform.position}\n" +
                     $"Rotation: {currentCamera.transform.rotation.eulerAngles}\n" +
                     $"Projection: {(currentCamera.orthographic ? "Orthographic" : "Perspective")}\n" +
                     $"Size/FOV: {(currentCamera.orthographic ? currentCamera.orthographicSize.ToString() : currentCamera.fieldOfView.ToString())}");
        }
    }
}