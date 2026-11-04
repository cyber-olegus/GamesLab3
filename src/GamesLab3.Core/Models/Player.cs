using GamesLab3.Core.Common;
using GamesLab3.Core.Numbers;

namespace GamesLab3.Core.Models;

/// <summary>Правила игрока из Лабы 2 дополнены отдельным таймером и временными эффектами.</summary>
public sealed class Player
{
    public BigNumber Gold { get; private set; } = BigNumber.Zero;
    public BigNumber Damage { get; private set; } = new(10);
    public BigNumber UpgradeCost { get; private set; } = new(20);
    public int Level { get; private set; } = 1;
    public double DamageModifier { get; private set; } = 1.2;
    public double UpgradeModifier { get; private set; } = 1.2;
    public double Points { get; private set; }
    public double BaseCooldown { get; private set; } = 0.8;
    public int CooldownLevel { get; private set; } = 1;
    public BigNumber CooldownUpgradeCost { get; private set; } = new(30);
    public CCountdownTimer ClickTimer { get; } = new();
    public BonusEffects Effects { get; } = new();
    public double EffectiveCooldown => Math.Max(0.08, BaseCooldown * Effects.Multiplier(EffectKind.Cooldown));
    public bool CanUpgradeCooldown => BaseCooldown > 0.12;

    public void AddGold(BigNumber amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        Gold = Gold + amount;
    }
    public void AddPoints(double points)
    {
        if (!double.IsFinite(points) || points < 0) throw new ArgumentOutOfRangeException(nameof(points));
        Points += points;
    }
    public bool TryBeginClick()
    {
        if (!ClickTimer.IsReady) return false;
        ClickTimer.Start(EffectiveCooldown);
        return true;
    }
    public void Update(double delta)
    {
        Guard.Delta(delta);
        Effects.Update(delta);
        ClickTimer.Update(delta);
    }
    public BigNumber DealDamage()
    {
        BigNumber result = Damage;
        // Каждый множитель применяется BigNumber: здоровье и урон не переводятся в double.
        foreach (var effect in Effects.Active.Where(effect => effect.Kind == EffectKind.Damage))
            result = result * effect.Multiplier;
        return result;
    }
    public bool TryUpgrade()
    {
        if (Gold < UpgradeCost) return false;
        BigNumber nextDamage = CalculateTotalDamage();
        BigNumber nextCost = CalculateNextUpgradeCost();
        int nextLevel = checked(Level + 1);
        if (!TrySpendGold(UpgradeCost)) return false;
        RecalculateStats(nextDamage, nextCost, nextLevel);
        return true;
    }
    public bool TryUpgradeCooldown()
    {
        if (!CanUpgradeCooldown || Gold < CooldownUpgradeCost) return false;
        BigNumber nextCost = CooldownUpgradeCost * 1.6;
        if (!TrySpendGold(CooldownUpgradeCost)) return false;
        BaseCooldown = Math.Max(0.12, BaseCooldown * 0.85);
        CooldownUpgradeCost = nextCost;
        CooldownLevel++;
        // Купленная скорость применяется к следующему клику; текущий таймер не сбрасываем.
        return true;
    }
    private bool TrySpendGold(BigNumber amount)
    {
        if (Gold < amount) return false;
        Gold = Gold - amount;
        return true;
    }
    private BigNumber CalculateTotalDamage()
    {
        BigNumber next = Damage * DamageModifier;
        return next > Damage ? next : Damage + BigNumber.One;
    }
    private BigNumber CalculateNextUpgradeCost() => (UpgradeCost * Level) * UpgradeModifier;
    private void RecalculateStats(BigNumber damage, BigNumber cost, int level)
    {
        Damage = damage; UpgradeCost = cost; Level = level;
    }
}
