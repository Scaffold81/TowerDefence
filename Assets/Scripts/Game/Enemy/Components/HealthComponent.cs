using UnityEngine;
using R3;

namespace Game.Enemy.Components
{
    /// <summary>
    /// Компонент управления здоровьем врага
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        private readonly ReactiveProperty<float> _health = new(100f);
        private readonly ReactiveProperty<float> _maxHealth = new(100f);
        private readonly ReactiveProperty<bool> _isAlive = new(true);
        
        public ReactiveProperty<float> Health => _health;
        public ReactiveProperty<float> MaxHealth => _maxHealth;
        public ReactiveProperty<bool> IsAlive => _isAlive;
        
        public float HealthPercentage => _maxHealth.Value > 0 ? _health.Value / _maxHealth.Value : 0f;
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        public void Initialize(float maxHealth)
        {
            _maxHealth.Value = maxHealth;
            _health.Value = maxHealth;
            _isAlive.Value = true;
        }
        
        /// <summary>
        /// Восстановление здоровья
        /// </summary>
        public void Heal(float amount)
        {
            if (!_isAlive.Value || amount <= 0f)
                return;
                
            _health.Value = Mathf.Min(_health.Value + amount, _maxHealth.Value);
        }
        
        /// <summary>
        /// Установка максимального здоровья
        /// </summary>
        public void SetMaxHealth(float maxHealth)
        {
            _maxHealth.Value = maxHealth;
            
            // Если текущее здоровье больше нового максимума, урезаем его
            if (_health.Value > _maxHealth.Value)
            {
                _health.Value = _maxHealth.Value;
            }
        }
        
        /// <summary>
        /// Проверка, жив ли враг
        /// </summary>
        public bool CheckIsAlive()
        {
            return _health.Value > 0f;
        }
        
        /// <summary>
        /// Принудительная смерть
        /// </summary>
        public void Kill()
        {
            _health.Value = 0f;
            _isAlive.Value = false;
        }
        
        private void Update()
        {
            // Проверяем состояние жизни
            bool wasAlive = _isAlive.Value;
            bool currentlyAlive = CheckIsAlive();
            
            if (wasAlive && !currentlyAlive)
            {
                _isAlive.Value = false;
            }
        }
        
        private void OnDestroy()
        {
            _health?.Dispose();
            _maxHealth?.Dispose();
            _isAlive?.Dispose();
        }
    }
}