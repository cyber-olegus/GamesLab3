using GamesLab3.Core.Common;
using GamesLab3.Core.Models;
using GamesLab3.Core.Numbers;

namespace GamesLab3.Core.Collectables;

public sealed class CDamageBooster : CCollectable
{
    public CDamageBooster(ScenePoint p, double size, double lifetime) : base(p, size, lifetime) { }
    public override string Title => "Двойной урон";
    public override string Color => "#EF4444";
    public override string Label => "×2";
    public override string Description => "Урон ×2 на 10 секунд; эффекты могут действовать одновременно";
    public override bool OnClick(Player player, CController controller, ScenePoint mousePosition) =>
        Activate(mousePosition, () => player.Effects.Add(EffectKind.Damage, 2, 10, Title));
}

public sealed class CGoldBonus : CCollectable
{
    public BigNumber Reward { get; }
    public CGoldBonus(ScenePoint p, double size, double lifetime, BigNumber reward) : base(p, size, lifetime)
    {
        ArgumentNullException.ThrowIfNull(reward);
        Reward = reward;
    }
    public override string Title => "Золотая сфера";
    public override string Color => "#EAB308";
    public override string Label => "G";
    public override string Description => $"Мгновенно +{Reward} золота";
    public override bool OnClick(Player player, CController controller, ScenePoint mousePosition) =>
        Activate(mousePosition, () => player.AddGold(Reward));
}
