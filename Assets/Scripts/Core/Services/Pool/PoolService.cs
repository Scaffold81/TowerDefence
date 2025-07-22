using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Object pool service for performance optimization.
    /// Allows object reuse instead of creating new ones, reducing GC pressure.
    /// </summary>
    public class PoolService : MonoBehaviour, IPoolService
    {
        private readonly Dictionary<int, Queue<GameObject>> pools = new Dictionary<int, Queue<GameObject>>();
        private readonly Dictionary<int, Transform> poolParents = new Dictionary<int, Transform>();
        private readonly Dictionary<GameObject, int> instanceToPoolId = new Dictionary<GameObject, int>();
        
        private Transform poolContainer;
        private IGameFactory gameFactory;

        [Inject]
        private void Construct(IGameFactory gameFactory)
        {
            Debug.Log("[PoolService] Initializing object pool service...");
            
            this.gameFactory = gameFactory;
            CreatePoolContainer();
            
            Debug.Log("[PoolService] Pool service initialized successfully!");
        }

        private void CreatePoolContainer()
        {
            Debug.Log("[PoolService] Creating pool container...");
            
            poolContainer = new GameObject("[POOLS]").transform;
            poolContainer.SetParent(transform);
            DontDestroyOnLoad(poolContainer.gameObject);
            
            Debug.Log("[PoolService] ✓ Pool container created successfully");
        }

        public T Get<T>(T prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null) where T : Component
        {
            var poolId = prefab.GetInstanceID();
            
            if (!pools.ContainsKey(poolId))
            {
                CreatePool(prefab, poolId);
            }

            GameObject instance;
            T component;
            
            if (pools[poolId].Count > 0)
            {
                instance = pools[poolId].Dequeue();
                instance.transform.SetParent(parent);
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.SetActive(true);
                component = instance.GetComponent<T>();
            }
            else
            {
                instance = gameFactory.Create(prefab.gameObject, position, parent);
                instance.transform.rotation = rotation;
                instanceToPoolId[instance] = poolId;
                component = instance.GetComponent<T>();
                
                // Initialize PoolableObject if necessary
                if (component is PoolableObject poolableObject)
                {
                    poolableObject.Initialize(this);
                }
            }

            // Call OnGetFromPool if object supports IPoolable
            if (component is IPoolable poolable)
            {
                poolable.OnGetFromPool();
            }

            return component;
        }

        public T Get<T>(T prefab, Transform parent) where T : Component
        {
            return Get(prefab, parent.position, parent.rotation, parent);
        }

        public void Return<T>(T instance) where T : Component
        {
            if (instance == null) return;
            Return(instance.gameObject);
        }

        public void Return(GameObject instance)
        {
            if (instance == null) return;
            
            if (instanceToPoolId.TryGetValue(instance, out var poolId))
            {
                // Call OnReturnToPool if object supports IPoolable
                var poolable = instance.GetComponent<IPoolable>();
                poolable?.OnReturnToPool();
                
                instance.SetActive(false);
                instance.transform.SetParent(poolParents[poolId]);
                pools[poolId].Enqueue(instance);
            }
            else
            {
                // Object not from pool, just destroy it
                Destroy(instance);
            }
        }

        public void Prewarm<T>(T prefab, int count) where T : Component
        {
            var poolId = prefab.GetInstanceID();
            
            if (!pools.ContainsKey(poolId))
            {
                CreatePool(prefab, poolId);
            }

            for (int i = 0; i < count; i++)
            {
                var instance = gameFactory.Create(prefab.gameObject, Vector3.zero, poolParents[poolId]);
                instance.SetActive(false);
                instanceToPoolId[instance] = poolId;
                pools[poolId].Enqueue(instance);
            }
        }

        public void Clear()
        {
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    var instance = pool.Dequeue();
                    if (instance != null)
                    {
                        instanceToPoolId.Remove(instance);
                        Destroy(instance);
                    }
                }
            }
            
            pools.Clear();
            poolParents.Clear();
            instanceToPoolId.Clear();
        }

        public void ClearPool<T>(T prefab) where T : Component
        {
            var poolId = prefab.GetInstanceID();
            
            if (pools.TryGetValue(poolId, out var pool))
            {
                while (pool.Count > 0)
                {
                    var instance = pool.Dequeue();
                    if (instance != null)
                    {
                        instanceToPoolId.Remove(instance);
                        Destroy(instance);
                    }
                }
                
                pools.Remove(poolId);
                
                if (poolParents.TryGetValue(poolId, out var parent))
                {
                    Destroy(parent.gameObject);
                    poolParents.Remove(poolId);
                }
            }
        }

        public int GetActiveCount<T>(T prefab) where T : Component
        {
            var poolId = prefab.GetInstanceID();
            
            if (!poolParents.TryGetValue(poolId, out var parent))
                return 0;
                
            int activeCount = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).gameObject.activeInHierarchy)
                    activeCount++;
            }
            
            return activeCount;
        }

        public int GetInactiveCount<T>(T prefab) where T : Component
        {
            var poolId = prefab.GetInstanceID();
            return pools.TryGetValue(poolId, out var pool) ? pool.Count : 0;
        }

        private void CreatePool<T>(T prefab, int poolId) where T : Component
        {
            Debug.Log($"[PoolService] Creating new pool for: {prefab.name} (ID: {poolId})");
            
            pools[poolId] = new Queue<GameObject>();
            
            var poolParent = new GameObject($"Pool_{prefab.name}").transform;
            poolParent.SetParent(poolContainer);
            poolParents[poolId] = poolParent;
            
            Debug.Log($"[PoolService] ✓ Pool created for {prefab.name}");
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}