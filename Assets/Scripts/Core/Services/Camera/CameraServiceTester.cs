using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Тестовый скрипт для проверки работы CameraService.
    /// </summary>
    public class CameraServiceTester : MonoBehaviour
    {
        [Inject] private ICameraService cameraService;
        [Inject] private ILevelService levelService;
        
        [Header("Test Controls")]
        [SerializeField] private KeyCode testCameraKey = KeyCode.C;
        [SerializeField] private KeyCode setupLevelKey = KeyCode.L;
        
        private void Update()
        {
            if (Input.GetKeyDown(testCameraKey))
            {
                TestCameraPositioning();
            }
            
            if (Input.GetKeyDown(setupLevelKey))
            {
                TestLevelSetup();
            }
        }
        
        private void TestCameraPositioning()
        {
            Debug.Log("[CameraServiceTester] Testing camera positioning...");
            
            if (cameraService == null)
            {
                Debug.LogError("[CameraServiceTester] CameraService is null!");
                return;
            }
            
            // Тест позиционирования в центр сцены
            cameraService.PositionCamera(Vector3.zero, height: 20f, angle: 45f);
        }
        
        private void TestLevelSetup()
        {
            Debug.Log("[CameraServiceTester] Testing level setup...");
            
            if (levelService == null)
            {
                Debug.LogError("[CameraServiceTester] LevelService is null!");
                return;
            }
            
            // Тест установки уровня
            levelService.SetupLevel("Lvl_01");
        }
        
        private void Start()
        {
            Debug.Log("[CameraServiceTester] Camera Service Tester ready!");
            Debug.Log($"Controls: {testCameraKey} - Test Camera, {setupLevelKey} - Setup Level");
            
            if (cameraService != null)
            {
                var camera = cameraService.GetCurrentCamera();
                Debug.Log($"[CameraServiceTester] Current camera: {(camera != null ? camera.name : "None")}");
            }
        }
    }
}
