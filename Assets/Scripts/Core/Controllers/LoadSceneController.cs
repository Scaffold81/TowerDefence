using Cysharp.Threading.Tasks;
using Game.Services;
using UnityEngine;
using Zenject;

namespace Game.Controllers.SceneMenegment
{
    public class LoadSceneController : MonoBehaviour
    {
        private ISceneManagerService sceneManagerService;

        [Inject]
        private void Construct(ISceneManagerService sceneManagerService)
        {
            this.sceneManagerService = sceneManagerService;
        }

        private void Start()
        {
            LoadTargetSceneAsynk().Forget();
        }

        private async UniTask LoadTargetSceneAsynk()
        {
            var delay = 1000;
            await UniTask.Delay(delay);

            sceneManagerService.LoadSceneAsync(sceneManagerService.TargetSceneId);
        }
    }
}
