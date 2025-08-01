using Game.Configs.Enemy;
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
        [SerializeField]
        EnemyConfigRepository enemyConfigRepository;
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
            
            // Create PoolService as MonoBehaviour component with proper Zenject binding
            var poolServiceGO = new GameObject("[PoolService]");
            var poolService = poolServiceGO.AddComponent<PoolService>();
            DontDestroyOnLoad(poolServiceGO);
            
            // Bind and inject dependencies
            Container.Bind<IPoolService>().FromInstance(poolService).AsSingle();
            Container.QueueForInject(poolService); // Это вызовет Construct()
            
            Debug.Log("[ProjectContextInstaller] ✓ PoolService: Object pooling for performance optimization");
        }

        /// <summary>
        /// Binding data and configuration services.
        /// </summary>
        private void BindDataServices()
        {
            Debug.Log("[ProjectContextInstaller] Binding data services...");
            
            // Level config repository - manages level configurations
            var levelConfigRepository = Resources.Load<Game.Configs.LevelConfigRepository>("Configs/LevelConfigRepository");
            if (levelConfigRepository != null)
            {
                Container.Bind<Game.Configs.LevelConfigRepository>().FromInstance(levelConfigRepository).AsSingle();
                Debug.Log("[ProjectContextInstaller] ✓ LevelConfigRepository: Level configuration management");
            }
            else
            {
                Debug.LogWarning("[ProjectContextInstaller] LevelConfigRepository not found in Resources/Configs/");
            }
            
            // enemy config repository - manages enemy configurations
            var enemyConfigRepository = Resources.Load<Game.Configs.Enemy.EnemyConfigRepository>("Configs/EnemyConfigRepository");
            if (enemyConfigRepository != null)
            {
                Container.Bind<Game.Configs.Enemy.EnemyConfigRepository>().FromInstance(enemyConfigRepository).AsSingle();
                Debug.Log("[ProjectContextInstaller] ✓ EnemyConfigRepository: Enemy configuration management");
            }
            else
            {
                Debug.LogWarning("[ProjectContextInstaller] EnemyConfigRepository not found in Resources/Configs/");
            }

            // Future data services:
            // Container.Bind<IDataService>().To<DataService>().AsSingle().NonLazy();
            // Container.Bind<ILocalizationService>().To<LocalizationService>().AsSingle().NonLazy();
        }
    }
}