using Game.Services;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    /// <summary>
    /// Global installer for project-level services.
    /// Contains services that live throughout the entire game and survive scene changes.
    /// </summary>
    public class ProjectContextInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Debug.Log("[ProjectContextInstaller] Starting global services initialization...");
            
            BindCoreServices();
            BindDataServices();
            
            Debug.Log("[ProjectContextInstaller] All global services initialized successfully!");
        }

        /// <summary>
        /// Binding core game services.
        /// </summary>
        private void BindCoreServices()
        {
            Debug.Log("[ProjectContextInstaller] Binding core services...");
            
            // Save system - manages saving and loading player data
            Container.Bind<ISaveService>().To<SaveService>().AsSingle().NonLazy();
            Debug.Log("[ProjectContextInstaller] ✓ SaveService: Player data persistence system");
            
            // Scene manager - handles transitions between game scenes
            Container.Bind<ISceneManagerService>().To<SceneManagerService>().AsSingle().NonLazy();
            Debug.Log("[ProjectContextInstaller] ✓ SceneManagerService: Scene transition management");
            
            // Object factory - creates and instantiates game objects
            Container.Bind<IGameFactory>().To<GameFactory>().AsSingle().NonLazy();
            Debug.Log("[ProjectContextInstaller] ✓ GameFactory: Object creation and instantiation");
            
            // UI manager - manages all user interface pages
            Container.Bind<IUIPageService>().To<UIPageService>().AsSingle().NonLazy();
            Debug.Log("[ProjectContextInstaller] ✓ UIPageService: Global UI pages management");
            
            // Object pool - optimizes performance through object reuse
            BindPoolService();
        }
        
        /// <summary>
        /// Binding object pool service as MonoBehaviour component.
        /// </summary>
        private void BindPoolService()
        {
            Debug.Log("[ProjectContextInstaller] Creating PoolService as MonoBehaviour...");
            
            // Create PoolService as MonoBehaviour component
            var poolServiceGO = new GameObject("[PoolService]");
            var poolService = poolServiceGO.AddComponent<PoolService>();
            DontDestroyOnLoad(poolServiceGO);
            Container.Bind<IPoolService>().FromInstance(poolService).AsSingle();
            
            Debug.Log("[ProjectContextInstaller] ✓ PoolService: Object pooling for performance optimization");
        }

        /// <summary>
        /// Binding data and configuration services.
        /// </summary>
        private void BindDataServices()
        {
            Debug.Log("[ProjectContextInstaller] Binding data services...");
            
            // Configuration service - manages game settings from ScriptableObjects
            Container.Bind<IConfigService>().To<ConfigService>().AsSingle().NonLazy();
            Debug.Log("[ProjectContextInstaller] ✓ ConfigService: Game configuration management from ScriptableObjects");
            
            // Future data services:
            // Container.Bind<IDataService>().To<DataService>().AsSingle().NonLazy();
            // Container.Bind<ILocalizationService>().To<LocalizationService>().AsSingle().NonLazy();
        }
    }
}