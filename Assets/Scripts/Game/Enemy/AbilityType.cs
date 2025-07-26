namespace Game.Enemy
{
    /// <summary>
    /// Типы способностей врагов
    /// </summary>
    public enum AbilityType
    {
        None = 0,
        
        // Поддержка союзников
        HealAllies = 1,
        BuffDamage = 2,
        BuffDefense = 3,
        RemoveDebuffs = 4,
        
        // Атакующие способности
        AreaAttack = 5,
        MagicAttack = 6,
        RangedAttack = 7,
        
        // Пассивные способности
        Aura = 8,
        FastMovement = 9,
        Stealth = 10
    }
}