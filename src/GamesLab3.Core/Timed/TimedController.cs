using GamesLab3.Core.Collectables;
using GamesLab3.Core.Common;

namespace GamesLab3.Core.Timed;

public sealed class TimedController : CController
{
    public double RemainingTime { get; private set; }
    public double Duration { get; private set; }
    public override double BaseSpawnInterval => 1.1;
    public bool BonusesEnabled { get; private set; }

    public TimedController(Random? random = null) : base(random) { }

    public void Start(double duration = 30, bool bonusesEnabled = true)
    {
        Guard.Positive(duration, nameof(duration));
        ResetScene();
        Duration = RemainingTime = duration;
        BonusesEnabled = bonusesEnabled;
        IsRunning = true;
        LastAction = "Раунд начался. Нажимайте на сферы!";
    }

    public override void Update(double delta)
    {
        Guard.Delta(delta);
        if (!IsRunning) return;
        // Последний tick не может продлить раунд за установленный предел.
        double step = Math.Min(delta, RemainingTime);
        RemainingTime = Math.Max(0, RemainingTime - step);
        if (RemainingTime <= 0)
        {
            IsRunning = false; ClearObjects();
            LastAction = $"Время вышло. Набрано очков: {Player.Points:0.0}.";
            return; // В конце раунда не создаём новые сферы.
        }
        UpdateScene(step);
    }

    public override bool TryClick(ScenePoint point)
    {
        if (!IsRunning) return false;
        return TryCollect(point, usesCooldown: BonusesEnabled);
    }

    protected override CCollectable CreateCollectable(ScenePoint p, double size, double life)
    {
        if (!BonusesEnabled) return new CPointGiver(p, size, life);
        return Random.Next(5) switch
        {
            0 => new CCooldownReducer(p, size, life),
            1 => new CLifetimeExtender(p, size, life),
            2 => new CSpawnAccelerator(p, size, life),
            3 => new CPointBonus(p, size, life),
            _ => new CPointGiver(p, size, life)
        };
    }
}
