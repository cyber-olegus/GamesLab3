namespace GamesLab3.Core.Common;

/// <summary>Только отсчёт времени. Не знает ни о золоте, ни об игроке, ни об окне.</summary>
public sealed class CCountdownTimer
{
    public double Remaining { get; private set; }
    public bool IsReady => Remaining <= 0;
    public void Start(double seconds) => Remaining = Guard.Positive(seconds, nameof(seconds));
    public void Reset() => Remaining = 0;
    public void Update(double delta) => Remaining = Math.Max(0, Remaining - Guard.Delta(delta));
}
