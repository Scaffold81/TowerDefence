using UnityEngine;

namespace Game.Services
{
    public interface IPoolService
    {
        T Get<T>(T prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null) where T : Component;
        T Get<T>(T prefab, Transform parent) where T : Component;
        void Return<T>(T instance) where T : Component;
        void Return(GameObject instance);
        void Prewarm<T>(T prefab, int count) where T : Component;
        void Clear();
        void ClearPool<T>(T prefab) where T : Component;
        int GetActiveCount<T>(T prefab) where T : Component;
        int GetInactiveCount<T>(T prefab) where T : Component;
    }
}