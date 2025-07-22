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
            Debug.Log("[GameSceneInstaller] Binding gameplay services...");
            
            // Future services that will be added as the project evolves:
            
            // Battle system - manages combat logic and battle interactions
            // Container.Bind<IBattleService>().To<BattleService>().AsSingle().NonLazy();
            
            // Wave system - manages spawning and behavior of enemy waves
            // Container.Bind<IWaveService>().To<WaveService>().AsSingle().NonLazy();
            
            // Hero system - manages the squad of witches and their abilities
            // Container.Bind<IHeroService>().To<HeroService>().AsSingle().NonLazy();
            
            // Enemy system - manages enemy behavior and characteristics
            // Container.Bind<IEnemyService>().To<EnemyService>().AsSingle().NonLazy();
            
            // Tower system - manages tower placement and upgrades
            // Container.Bind<ITowerService>().To<TowerService>().AsSingle().NonLazy();
            
            // Spell system - manages magical abilities and spells
            // Container.Bind<ISpellService>().To<SpellService>().AsSingle().NonLazy();
            
            Debug.Log("[GameSceneInstaller] No gameplay services bound yet - waiting for implementation");
        }
    }
}