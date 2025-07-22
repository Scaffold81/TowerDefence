using Game.Services;
using UnityEngine;

namespace Game.Objects
{
    public class Projectile : PoolableObject
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float lifeTime = 5f;
        
        private Vector3 direction;
        private float currentLifeTime;
        private Transform target;

        public override void OnGetFromPool()
        {
            base.OnGetFromPool();
            currentLifeTime = lifeTime;
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            target = null;
            direction = Vector3.zero;
        }

        public void Launch(Vector3 startPosition, Vector3 targetDirection, float projectileSpeed = -1, float projectileDamage = -1)
        {
            transform.position = startPosition;
            direction = targetDirection.normalized;
            
            if (projectileSpeed > 0) speed = projectileSpeed;
            if (projectileDamage > 0) damage = projectileDamage;
        }

        public void Launch(Vector3 startPosition, Transform targetTransform, float projectileSpeed = -1, float projectileDamage = -1)
        {
            transform.position = startPosition;
            target = targetTransform;
            
            if (projectileSpeed > 0) speed = projectileSpeed;
            if (projectileDamage > 0) damage = projectileDamage;
        }

        private void Update()
        {
            Move();
            CheckLifeTime();
        }

        private void Move()
        {
            if (target != null)
            {
                // Target tracking
                direction = (target.position - transform.position).normalized;
            }

            transform.position += direction * speed * Time.deltaTime;
            
            // Rotate projectile towards movement direction
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void CheckLifeTime()
        {
            currentLifeTime -= Time.deltaTime;
            if (currentLifeTime <= 0)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if hit target
            if (other.CompareTag("Enemy"))
            {
                // Deal damage to enemy
                var enemy = other.GetComponent<IDamageable>();
                enemy?.TakeDamage(damage);
                
                // Return projectile to pool
                ReturnToPool();
            }
        }

        public float GetDamage() => damage;
        public float GetSpeed() => speed;
    }

    // Interface for objects that can take damage
    public interface IDamageable
    {
        void TakeDamage(float damage);
        bool IsAlive { get; }
    }
}