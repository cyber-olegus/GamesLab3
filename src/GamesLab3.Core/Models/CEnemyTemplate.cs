using System.Text.Json.Serialization;

namespace GamesLab3.Core.Models;

/// <summary>Имена свойств и JSON совпадают с редактором из Лабы 1.</summary>
public sealed class CEnemyTemplate
{
    public string Name { get; private set; }
    public string IconName { get; private set; }
    public int BaseLife { get; private set; }
    public int BaseGold { get; private set; }
    public double LifeModifier { get; private set; }
    public double GoldModifier { get; private set; }
    public double SpawnChance { get; private set; }

    [JsonConstructor]
    public CEnemyTemplate(string name, string iconName, int baseLife, double lifeModifier,
        int baseGold, double goldModifier, double spawnChance)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(iconName))
            throw new ArgumentException("Имя и иконка противника обязательны.");
        if (baseLife <= 0 || baseGold < 0) throw new ArgumentException("Некорректные здоровье или награда.");
        if (!double.IsFinite(lifeModifier) || lifeModifier <= 0 ||
            !double.IsFinite(goldModifier) || goldModifier < 0)
            throw new ArgumentException("Некорректные модификаторы.");
        if (!double.IsFinite(spawnChance) || spawnChance < 0 || spawnChance > 1)
            throw new ArgumentException("Шанс должен быть от 0 до 1.");
        Name = name.Trim(); IconName = iconName.Trim(); BaseLife = baseLife; BaseGold = baseGold;
        LifeModifier = lifeModifier; GoldModifier = goldModifier; SpawnChance = spawnChance;
    }
}
