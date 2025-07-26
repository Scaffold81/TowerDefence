using Game.Services;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    /// <summary>
    /// Installer for scene-level services.
    /// Contains services that are created anew for each scene.
    /// </summary>
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Debug.Log("[GameSceneInstaller] Starting scene-level services initialization...");
            
            BindGameplayServices();
            
            Debug.Log("[GameSceneInstaller] Scene-level services initialization completed!");
        }

        /// <summary>
        /// Binding gameplay services specific to the current scene.
        /// </summary>
        private void BindGameplayServices()
        {
            // Camera System - улучшенная архитектура с четким разделением ответственности
            Container.Bind<ICameraService>().To<CameraService>().AsSingle().NonLazy();
            Container.Bind<ICameraController>().FromComponentInHierarchy().AsSingle();
            Debug.Log("[GameSceneInstaller] ✓ Camera System: Service for calculations, Controller for execution");

            // Level service - manages current level state and configuration
            Container.Bind<ILevelService>().To<LevelService>().AsSingle().NonLazy();
            Debug.Log("[GameSceneInstaller] ✓ LevelService: Level management and configuration");
            
            // Wave service - manages enemy wave spawning
            Container.Bind<IWaveService>().To<WaveService>().AsSingle().NonLazy();
            Debug.Log("[GameSceneInstaller] ✓ WaveService: Enemy wave spawning management");
            
            // Future services that will be added as the project evolves:
            
            // Battle system - manages combat logic and battle interactions
            // Container.Bind<IBattleService>().To<BattleService>().AsSingle().NonLazy();
            
            // Hero system - manages the squad of witches and their abilities
            // Container.Bind<IHeroService>().To<HeroService>().AsSingle().NonLazy();
            
            // Enemy system - manages enemy behavior and characteristics
            // Container.Bind<IEnemyService>().To<EnemyService>().AsSingle().NonLazy();
            
            // Tower system - manages tower placement and upgrades
            // Container.Bind<ITowerService>().To<TowerService>().AsSingle().NonLazy();
            
            // Spell system - manages magical abilities and spells
            // Container.Bind<ISpellService>().To<SpellService>().AsSingle().NonLazy();
        }
    }
}