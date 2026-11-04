using GamesLab3.Core.Common;
using GamesLab3.Core.Models;

namespace GamesLab3.Core.Collectables;

public class CPointGiver : CCollectable
{
    public double PointsValue { get; }
    public override string Title => "Обычная сфера";
    public override string Color => "#7C3AED";
    public override string Label => "+";
    public override string Description => $"Очки: +{PointsValue:0.0}";

    // Перегрузка конструкторов — полиморфизм времени компиляции.
    public CPointGiver(ScenePoint position, double size, double lifetime)
        : this(position, size, lifetime, Math.Max(1, 1000 / (size * lifetime))) { }
    public CPointGiver(ScenePoint position, double size, double lifetime, double points)
        : base(position, size, lifetime)
    {
        if (!double.IsFinite(points) || points < 0) throw new ArgumentOutOfRangeException(nameof(points));
        PointsValue = points;
    }
    public override bool OnClick(Player player, CController controller, ScenePoint mousePosition) =>
        Activate(mousePosition, () => player.AddPoints(PointsValue));
}

public class CPointBonus : CPointGiver
{
    public CPointBonus(ScenePoint position, double size, double lifetime)
        : base(position, size, lifetime, 100) { }
    public override string Title => "Бонус очков";
    public override string Color => "#EAB308";
    public override string Label => "100";
    // Запрещаем менять обещание этого бонуса в более глубоком наследнике.
    public sealed override string Description => "Мгновенно +100 очков";
}
