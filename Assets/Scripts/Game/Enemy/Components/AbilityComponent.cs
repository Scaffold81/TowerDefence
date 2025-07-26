using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Game.Enemy.Components
{
    /// <summary>
    /// Компонент способностей врага
    /// </summary>
    public class AbilityComponent : MonoBehaviour
    {
        [System.Serializable]
        public class AbilityData
        {
            public AbilityType type;
            public float cooldown = 5f;
            public float range = 5f;
            public float power = 1f;
            [TextArea(2, 4)]
            public string description;
        }
        
        [Header("Abilities")]
        [SerializeField] private List<AbilityData> _abilities = new();
        
        private Dictionary<AbilityType, AbilityData> _abilityMap = new();
        private Dictionary<AbilityType, float> _abilityCooldowns = new();
        private bool _isInitialized = false;
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        public void Initialize(List<AbilityData> abilities)
        {
            _abilityMap.Clear();
            _abilityCooldowns.Clear();
            
            if (abilities != null)
            {
                foreach (var ability in abilities)
                {
                    _abilityMap[ability.type] = ability;
                    _abilityCooldowns[ability.type] = 0f;
                }
            }
            
            _abilities = new List<AbilityData>(abilities ?? new List<AbilityData>());
            _isInitialized = true;
            
            // Запускаем обновление кулдаунов
            UpdateCooldowns().Forget();
        }
        
        /// <summary>
        /// Проверить, доступна ли способность
        /// </summary>
        public bool IsAbilityReady(AbilityType type)
        {
            return _abilityMap.ContainsKey(type) && _abilityCooldowns.TryGetValue(type, out float cooldown) && cooldown <= 0f;
        }
        
        /// <summary>
        /// Использовать способность
        /// </summary>
        public bool TryUseAbility(AbilityType type, Vector3 targetPosition = default)
        {
            if (!IsAbilityReady(type))
                return false;
            
            var abilityData = _abilityMap[type];
            
            // Запускаем кулдаун
            _abilityCooldowns[type] = abilityData.cooldown;
            
            // Выполняем способность
            ExecuteAbility(type, abilityData, targetPosition);
            
            return true;
        }
        
        /// <summary>
        /// Получить данные способности
        /// </summary>
        public AbilityData GetAbilityData(AbilityType type)
        {
            return _abilityMap.TryGetValue(type, out AbilityData data) ? data : null;
        }
        
        /// <summary>
        /// Получить оставшееся время кулдауна
        /// </summary>
        public float GetRemainingCooldown(AbilityType type)
        {
            return _abilityCooldowns.TryGetValue(type, out float cooldown) ? Mathf.Max(0f, cooldown) : 0f;
        }
        
        /// <summary>
        /// Проверить, есть ли способность определенного типа
        /// </summary>
        public bool HasAbility(AbilityType type)
        {
            return _abilityMap.ContainsKey(type);
        }
        
        /// <summary>
        /// Получить все способности
        /// </summary>
        public List<AbilityType> GetAllAbilities()
        {
            return new List<AbilityType>(_abilityMap.Keys);
        }
        
        /// <summary>
        /// Выполнение конкретной способности
        /// </summary>
        private void ExecuteAbility(AbilityType type, AbilityData data, Vector3 targetPosition)
        {
            switch (type)
            {
                case AbilityType.HealAllies:
                    ExecuteHealAllies(data);
                    break;
                    
                case AbilityType.BuffDamage:
                    ExecuteBuffDamage(data);
                    break;
                    
                case AbilityType.BuffDefense:
                    ExecuteBuffDefense(data);
                    break;
                    
                case AbilityType.RemoveDebuffs:
                    ExecuteRemoveDebuffs(data);
                    break;
                    
                case AbilityType.AreaAttack:
                    ExecuteAreaAttack(data, targetPosition);
                    break;
                    
                case AbilityType.MagicAttack:
                    ExecuteMagicAttack(data, targetPosition);
                    break;
                    
                case AbilityType.RangedAttack:
                    ExecuteRangedAttack(data, targetPosition);
                    break;
                    
                default:
                    Debug.Log(gameObject.name + " used ability: " + type);
                    break;
            }
        }
        
        #region Ability Implementations
        
        private void ExecuteHealAllies(AbilityData data)
        {
            Debug.Log($"{gameObject.name} heals allies with power {data.power}");
            // TODO: Найти союзников в радиусе и вылечить их
        }
        
        private void ExecuteBuffDamage(AbilityData data)
        {
            Debug.Log($"{gameObject.name} buffs damage of allies with power {data.power}");
            // TODO: Усилить урон союзников
        }
        
        private void ExecuteBuffDefense(AbilityData data)
        {
            Debug.Log($"{gameObject.name} buffs defense of allies with power {data.power}");
            // TODO: Усилить защиту союзников
        }
        
        private void ExecuteRemoveDebuffs(AbilityData data)
        {
            Debug.Log($"{gameObject.name} removes debuffs from allies");
            // TODO: Снять дебаффы с союзников
        }
        
        private void ExecuteAreaAttack(AbilityData data, Vector3 targetPosition)
        {
            Debug.Log($"{gameObject.name} performs area attack at {targetPosition} with power {data.power}");
            // TODO: Атака по области
        }
        
        private void ExecuteMagicAttack(AbilityData data, Vector3 targetPosition)
        {
            Debug.Log($"{gameObject.name} performs magic attack at {targetPosition} with power {data.power}");
            // TODO: Магическая атака
        }
        
        private void ExecuteRangedAttack(AbilityData data, Vector3 targetPosition)
        {
            Debug.Log($"{gameObject.name} performs ranged attack at {targetPosition} with power {data.power}");
            // TODO: Дальняя атака
        }
        
        #endregion
        
        /// <summary>
        /// Обновление кулдаунов способностей
        /// </summary>
        private async UniTaskVoid UpdateCooldowns()
        {
            while (_isInitialized && this != null)
            {
                var keys = new List<AbilityType>(_abilityCooldowns.Keys);
                
                foreach (var key in keys)
                {
                    if (_abilityCooldowns[key] > 0f)
                    {
                        _abilityCooldowns[key] -= Time.deltaTime;
                    }
                }
                
                await UniTask.Yield();
            }
        }
        
        private void OnDestroy()
        {
            _isInitialized = false;
        }
        
        /// <summary>
        /// Инициализация из инспектора (для тестирования)
        /// </summary>
        [ContextMenu("Initialize From Inspector")]
        private void InitializeFromInspector()
        {
            Initialize(_abilities);
        }
    }
}