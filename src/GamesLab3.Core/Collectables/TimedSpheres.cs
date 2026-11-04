using GamesLab3.Core.Common;
using GamesLab3.Core.Models;
using GamesLab3.Core.Numbers;

namespace GamesLab3.Core.Collectables;

public sealed class CLifetimeExtender : CCollectable
{
    public CLifetimeExtender(ScenePoint p, double size, double lifetime) : base(p, size, lifetime) { }
    public override string Title => "Долгая жизнь сфер";
    public override string Color => "#22C55E";
    public override string Label => "L";
    public override string Description => "Новые сферы живут в 1,5 раза дольше в течение 10 секунд";
    public override bool OnClick(Player player, CController controller, ScenePoint mousePosition) =>
        Activate(mousePosition, () => controller.Effects.Add(EffectKind.SphereLifetime, 1.5, 10, Title));
}

public sealed class CSpawnAccelerator : CCollectable
{
    public CSpawnAccelerator(ScenePoint p, double size, double lifetime) : base(p, size, lifetime) { }
    public override string Title => "Больше сфер";
    public override string Color => "#F97316";
    public override string Label => "S";
    public override string Description => "Интервал появления новых сфер ×0,65 на 10 секунд";
    public override bool OnClick(Player player, CController controller, ScenePoint mousePosition) =>
        Activate(mousePosition, () => controller.Effects.Add(EffectKind.SpawnInterval, 0.65, 10, Title));
}

