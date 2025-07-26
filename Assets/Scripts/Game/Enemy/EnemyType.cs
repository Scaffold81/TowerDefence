namespace Game.Enemy
{
    /// <summary>
    /// Типы врагов из дизайн документа
    /// </summary>
    public enum EnemyType
    {
        // Животные
        Animal = 0,
        
        // Магические твари
        MagicalCreature = 1,
        
        // Базовые враги
        Bandit = 2,
        Warrior = 3,
        Knight = 4,
        Mercenary = 5,
        
        // Отряды поддержки
        Bard = 6,
        Alchemist = 7,
        Priest = 8,
        
        // Высокоуровневые враги
        Monster = 9,
        Succubus = 10
    }
}