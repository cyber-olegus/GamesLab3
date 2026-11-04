using GamesLab3.Core.Numbers;
namespace GamesLab3.Core.Models;

public sealed class Enemy
{
    public string Name { get; private set; }
    public string Icon { get; private set; }
    public BigNumber MaxHitPoints { get; private set; }
    public BigNumber CurrentHitPoints { get; private set; }
    public BigNumber GoldReward { get; private set; }
    public bool IsDead { get; private set; }

    public Enemy(string name, BigNumber hitPoints, BigNumber goldReward, string icon)
    {
        ArgumentNullException.ThrowIfNull(hitPoints);
        ArgumentNullException.ThrowIfNull(goldReward);
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(icon) || hitPoints <= BigNumber.Zero)
            throw new ArgumentException("Некорректный противник.");
        Name = name; Icon = icon; MaxHitPoints = CurrentHitPoints = hitPoints; GoldReward = goldReward;
    }

    public bool TakeDamage(BigNumber damage, out BigNumber goldReward)
    {
        ArgumentNullException.ThrowIfNull(damage);
        goldReward = BigNumber.Zero;
        if (IsDead) return false;
        if (damage >= CurrentHitPoints)
        {
            CurrentHitPoints = BigNumber.Zero; IsDead = true; goldReward = GoldReward; return true;
        }
        CurrentHitPoints = CurrentHitPoints - damage;
        return false;
    }
}
