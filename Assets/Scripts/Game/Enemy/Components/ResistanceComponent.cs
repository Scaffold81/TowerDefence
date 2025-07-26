using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy.Components
{
    /// <summary>
    /// Компонент сопротивлений врага
    /// </summary>
    public class ResistanceComponent : MonoBehaviour
    {
        [System.Serializable]
        public class ResistanceData
        {
            public ResistanceType type;
            [Range(0f, 1f)] public float value; // 0 = нет сопротивления, 1 = полная невосприимчивость
        }
        
        [Header("Resistances")]
        [SerializeField] private List<ResistanceData> _resistances = new();
        
        private Dictionary<ResistanceType, float> _resistanceMap = new();
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        public void Initialize(Dictionary<ResistanceType, float> resistances)
        {
            _resistanceMap.Clear();
            
            if (resistances != null)
            {
                foreach (var kvp in resistances)
                {
                    _resistanceMap[kvp.Key] = Mathf.Clamp01(kvp.Value);
                }
            }
            
            // Обновляем SerializedField для отображения в инспекторе
            UpdateSerializedResistances();
        }
        
        /// <summary>
        /// Получить значение сопротивления к определенному типу
        /// </summary>
        public float GetResistance(ResistanceType type)
        {
            return _resistanceMap.TryGetValue(type, out float value) ? value : 0f;
        }
        
        /// <summary>
        /// Установить сопротивление к определенному типу
        /// </summary>
        public void SetResistance(ResistanceType type, float value)
        {
            _resistanceMap[type] = Mathf.Clamp01(value);
            UpdateSerializedResistances();
        }
        
        /// <summary>
        /// Добавить сопротивление (например, от баффа)
        /// </summary>
        public void AddResistance(ResistanceType type, float value)
        {
            float currentValue = GetResistance(type);
            SetResistance(type, currentValue + value);
        }
        
        /// <summary>
        /// Убрать сопротивление
        /// </summary>
        public void RemoveResistance(ResistanceType type, float value)
        {
            float currentValue = GetResistance(type);
            SetResistance(type, currentValue - value);
        }
        
        /// <summary>
        /// Проверить, есть ли иммунитет к определенному типу
        /// </summary>
        public bool IsImmuneTo(ResistanceType type)
        {
            return GetResistance(type) >= 1f;
        }
        
        /// <summary>
        /// Получить все сопротивления
        /// </summary>
        public Dictionary<ResistanceType, float> GetAllResistances()
        {
            return new Dictionary<ResistanceType, float>(_resistanceMap);
        }
        
        /// <summary>
        /// Очистить все сопротивления
        /// </summary>
        public void ClearResistances()
        {
            _resistanceMap.Clear();
            UpdateSerializedResistances();
        }
        
        /// <summary>
        /// Обновить сериализованные данные для инспектора
        /// </summary>
        private void UpdateSerializedResistances()
        {
            _resistances.Clear();
            
            foreach (var kvp in _resistanceMap)
            {
                _resistances.Add(new ResistanceData
                {
                    type = kvp.Key,
                    value = kvp.Value
                });
            }
        }
        
        /// <summary>
        /// Инициализация из сериализованных данных (для тестирования в инспекторе)
        /// </summary>
        [ContextMenu("Initialize From Inspector")]
        private void InitializeFromInspector()
        {
            _resistanceMap.Clear();
            
            foreach (var resistance in _resistances)
            {
                _resistanceMap[resistance.type] = Mathf.Clamp01(resistance.value);
            }
        }
        
        private void OnValidate()
        {
            // Ограничиваем значения сопротивлений в инспекторе
            for (int i = 0; i < _resistances.Count; i++)
            {
                _resistances[i].value = Mathf.Clamp01(_resistances[i].value);
            }
        }
    }
}