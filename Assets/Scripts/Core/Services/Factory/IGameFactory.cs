using UnityEngine;

namespace Game.Services
{
    public interface IGameFactory
    {
        T Create<T>(T prefab, Vector3 position, Transform parent) where T : Object;
    }
}