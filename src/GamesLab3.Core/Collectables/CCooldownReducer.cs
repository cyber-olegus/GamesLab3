using GamesLab3.Core.Common;
using GamesLab3.Core.Models;
using GamesLab3.Core.Numbers;

namespace GamesLab3.Core.Collectables;

public sealed class CCooldownReducer : CCollectable
{
    public CCooldownReducer(ScenePoint p, double size, double lifetime) : base(p, size, lifetime) { }
    public override string Title => "Ускорение клика";
    public override string Color => "#06B6D4";
    public override string Label => "CD";
    public override string Description => "Следующие клики: перезарядка ×0,6 на 10 секунд";
    public override bool OnClick(Player player, CController controller, ScenePoint mousePosition) =>
        Activate(mousePosition, () => player.Effects.Add(EffectKind.Cooldown, 0.6, 10, Title));
}

