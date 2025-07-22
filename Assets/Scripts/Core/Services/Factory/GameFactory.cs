using UnityEngine;

namespace Game.Services
{
    public class GameFactory : IGameFactory
    {
        public T Create<T>(T prefab, Vector3 position, Transform parent) where T : UnityEngine.Object
        {
            return Object.Instantiate(prefab, position, Quaternion.identity, parent);
        }
    }
}