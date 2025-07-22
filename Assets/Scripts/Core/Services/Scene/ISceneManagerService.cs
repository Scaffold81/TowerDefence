
using Game.Enums;

namespace Game.Services
{
    public interface ISceneManagerService
    {
        SceneId TargetSceneId { get; }

        void LoadSceneAsync(SceneId scene);
    }
}
