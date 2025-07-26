using UnityEngine;
using R3;
using Zenject;
using Game.Services;
using Game.Enemy.Components;
using Game.Path;
using PoolableInterface = Game.Services.IPoolable;
using Game.Configs.Enemy;

namespace Game.Enemy
{
    /// <summary>
    /// Основной класс врага - универсальная реализация для всех типов врагов
    /// </summary>
    public class Enemy : MonoBehaviour, IEnemy, PoolableInterface
    {
        [Header("Components")]
        [SerializeField] private HealthComponent _healthComponent;
        [SerializeField] private MovementComponent _movementComponent;
        [SerializeField] private ResistanceComponent _resistanceComponent;
        [SerializeField] private AbilityComponent _abilityComponent;
        
        // Инжектируемые сервисы
        [Inject] private IPoolService _poolService;
        
        private EnemyConfig _config;
        private bool _isInitialized = false;
        private System.IDisposable _healthSubscription;
        
        // Реактивные свойства
        public EnemyType EnemyType { get; private set; }
        public EnemyCategory Category { get; private set; }
        
        public ReactiveProperty<float> Health => _healthComponent?.Health ?? new ReactiveProperty<float>(0f);
        public ReactiveProperty<float> MaxHealth => _healthComponent?.MaxHealth ?? new ReactiveProperty<float>(0f);
        public ReactiveProperty<Vector3> Position => _movementComponent?.Position ?? new ReactiveProperty<Vector3>(Vector3.zero);
        public ReactiveProperty<bool> IsAlive => _healthComponent?.IsAlive ?? new ReactiveProperty<bool>(false);
        public ReactiveProperty<bool> IsMoving => _movementComponent?.IsMoving ?? new ReactiveProperty<bool>(false);
        
        private void Awake()
        {
            // Получаем компоненты, если они не назначены
            if (_healthComponent == null)
                _healthComponent = GetComponent<HealthComponent>();
            if (_movementComponent == null)
                _movementComponent = GetComponent<MovementComponent>();
            if (_resistanceComponent == null)
                _resistanceComponent = GetComponent<ResistanceComponent>();
            if (_abilityComponent == null)
                _abilityComponent = GetComponent<AbilityComponent>();
        }
        
        private void Start()
        {
            // Подписываемся на смерть врага
            if (_healthComponent != null)
            {
                _healthSubscription = _healthComponent.IsAlive.Subscribe(isAlive =>
                {
                    if (!isAlive && _isInitialized)
                    {
                        OnEnemyDied();
                    }
                });
            }
        }
        
        /// <summary>
        /// Инициализация врага с конфигурацией
        /// </summary>
        public void Initialize(EnemyConfig config, Vector3 spawnPosition)
        {
            if (config == null)
            {
                Debug.LogError($"Cannot initialize enemy {gameObject.name} with null config!");
                return;
            }
            
            _config = config;
            EnemyType = config.enemyType;
            Category = config.category;
            
            // Устанавливаем позицию
            transform.position = spawnPosition;
            
            // Инициализируем компоненты
            InitializeComponents();
            
            _isInitialized = true;
            
            Debug.Log($"Enemy {config.enemyType} initialized at {spawnPosition}");
        }
        
        /// <summary>
        /// Запуск движения по пути
        /// </summary>
        public void StartMovement(LevelMap levelMap)
        {
            if (!_isInitialized)
            {
                Debug.LogError($"Cannot start movement for uninitialized enemy {gameObject.name}");
                return;
            }
            
            if (_movementComponent != null)
            {
                _movementComponent.StartMovement(levelMap);
            }
        }
        
        /// <summary>
        /// Остановка движения
        /// </summary>
        public void StopMovement()
        {
            if (_movementComponent != null)
            {
                _movementComponent.StopMovement();
            }
        }
        
        /// <summary>
        /// Получение текущей позиции
        /// </summary>
        public Vector3 GetPosition()
        {
            return transform.position;
        }
        
        /// <summary>
        /// Получение направления движения
        /// </summary>
        public Vector3 GetMovementDirection()
        {
            return _movementComponent?.MovementDirection.Value ?? Vector3.zero;
        }
        
        /// <summary>
        /// Инициализация всех компонентов
        /// </summary>
        private void InitializeComponents()
        {
            // Инициализация здоровья
            if (_healthComponent != null)
            {
                _healthComponent.Initialize(_config.maxHealth);
            }
            
            // Инициализация движения
            if (_movementComponent != null)
            {
                _movementComponent.Initialize(_config.speed);
            }
            
            // Инициализация сопротивлений
            if (_resistanceComponent != null && _config.resistances != null)
            {
                var resistanceDict = new System.Collections.Generic.Dictionary<ResistanceType, float>();
                foreach (var resistance in _config.resistances)
                {
                    resistanceDict[resistance.type] = resistance.value;
                }
                _resistanceComponent.Initialize(resistanceDict);
            }
            
            // Инициализация способностей
            if (_abilityComponent != null && _config.abilities != null)
            {
                _abilityComponent.Initialize(_config.abilities);
            }
        }
        
        /// <summary>
        /// Вызывается при смерти врага
        /// </summary>
        private void OnEnemyDied()
        {
            Debug.Log($"Enemy {EnemyType} died!");
            
            // Останавливаем движение
            StopMovement();
            
            // Возвращаем в пул через небольшую задержку
            Invoke(nameof(ReturnToPool), 1f);
        }
        
        #region IPoolable Implementation
        
        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
            _isInitialized = false;
        }
        
        public void OnReturnToPool()
        {
            // Сброс состояния
            StopMovement();
            _config = null;
            _isInitialized = false;
            
            gameObject.SetActive(false);
        }
        
        public void ReturnToPool()
        {
            if (_poolService != null)
            {
                _poolService.Return(this);
            }
            else
            {
                // Fallback - просто деактивируем объект
                gameObject.SetActive(false);
            }
        }
        
        #endregion
        
        /// <summary>
        /// Получить конфигурацию врага
        /// </summary>
        public EnemyConfig GetConfig()
        {
            return _config;
        }
        
        /// <summary>
        /// Получить компонент здоровья
        /// </summary>
        public HealthComponent GetHealthComponent()
        {
            return _healthComponent;
        }
        
        /// <summary>
        /// Получить компонент движения
        /// </summary>
        public MovementComponent GetMovementComponent()
        {
            return _movementComponent;
        }
        
        /// <summary>
        /// Получить компонент сопротивлений
        /// </summary>
        public ResistanceComponent GetResistanceComponent()
        {
            return _resistanceComponent;
        }
        
        /// <summary>
        /// Получить компонент способностей
        /// </summary>
        public AbilityComponent GetAbilityComponent()
        {
            return _abilityComponent;
        }
        
        private void OnDestroy()
        {
            _healthSubscription?.Dispose();
        }
    }
}