using UnityEngine;

namespace Game.Services
{
    public abstract class PoolableObject : MonoBehaviour, IPoolable
    {
        protected IPoolService poolService;
        
        protected virtual void Awake()
        {
            // Will be injected through Zenject if needed
        }

        public void Initialize(IPoolService poolService)
        {
            this.poolService = poolService;
        }

        public virtual void OnGetFromPool()
        {
            // Override in derived classes for initialization when getting from pool
        }

        public virtual void OnReturnToPool()
        {
            // Override in derived classes for cleanup when returning to pool
        }

        public void ReturnToPool()
        {
            OnReturnToPool();
            poolService?.Return(this);
        }

        protected virtual void OnEnable()
        {
            OnGetFromPool();
        }

        protected virtual void OnDisable()
        {
            OnReturnToPool();
        }
    }
}