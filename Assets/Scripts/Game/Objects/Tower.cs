using Game.Services;
using UnityEngine;
using Zenject;

namespace Game.Objects
{
    public class Tower : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float attackDamage = 25f;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private LayerMask enemyLayer = 1;
        
        [Header("Projectile Settings")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        
        private IPoolService poolService;
        private float lastAttackTime;
        private Transform currentTarget;
        
        [Inject]
        private void Construct(IPoolService poolService)
        {
            this.poolService = poolService;
        }

        private void Start()
        {
            // Pre-create projectiles in pool for optimization
            if (projectilePrefab != null)
            {
                poolService.Prewarm(projectilePrefab, 10);
            }
        }

        private void Update()
        {
            FindTarget();
            AttackTarget();
        }

        private void FindTarget()
        {
            // Find closest enemy within attack range
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
            
            float closestDistance = float.MaxValue;
            Transform closestEnemy = null;
            
            foreach (var enemy in enemiesInRange)
            {
                var distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
            
            currentTarget = closestEnemy;
        }

        private void AttackTarget()
        {
            if (currentTarget == null) return;
            if (Time.time - lastAttackTime < 1f / attackSpeed) return;
            
            // Rotate tower towards target
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(directionToTarget);
            
            // Shoot projectile from pool
            FireProjectile();
            
            lastAttackTime = Time.time;
        }

        private void FireProjectile()
        {
            if (projectilePrefab == null || firePoint == null) return;
            
            // Get projectile from pool
            var projectile = poolService.Get(projectilePrefab, firePoint.position, firePoint.rotation);
            
            // Configure projectile
            Vector3 direction = (currentTarget.position - firePoint.position).normalized;
            projectile.Launch(firePoint.position, direction, 15f, attackDamage);
        }

        private void OnDrawGizmosSelected()
        {
            // Show attack range in editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            if (currentTarget != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, currentTarget.position);
            }
        }

        public float GetAttackRange() => attackRange;
        public float GetAttackDamage() => attackDamage;
        public float GetAttackSpeed() => attackSpeed;
    }
}