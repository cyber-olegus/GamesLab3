namespace GamesLab3.Core.Common;

public enum EffectKind { Damage, Cooldown, SphereLifetime, SpawnInterval }

public sealed class TimedEffect
{
    public EffectKind Kind { get; }
    public double Multiplier { get; }
    public double Remaining { get; private set; }
    public string Name { get; }
    internal TimedEffect(EffectKind kind, double multiplier, double seconds, string name)
    {
        Kind = kind;
        Multiplier = Guard.Positive(multiplier, nameof(multiplier));
        Remaining = Guard.Positive(seconds, nameof(seconds));
        Name = name;
    }
    internal void Update(double delta) => Remaining = Math.Max(0, Remaining - delta);
}

/// <summary>Каждое усиление имеет собственный срок. Один бонус не отменяет другой.</summary>
public sealed class BonusEffects
{
    private readonly List<TimedEffect> effects = new();
    public IReadOnlyList<TimedEffect> Active => effects.AsReadOnly();
    public void Add(EffectKind kind, double multiplier, double seconds, string name) =>
        effects.Add(new TimedEffect(kind, multiplier, seconds, name));

    public double Multiplier(EffectKind kind)
    {
        double result = 1;
        foreach (var effect in effects.Where(effect => effect.Kind == kind))
            result *= effect.Multiplier;
        return result;
    }
    public void Update(double delta)
    {
        Guard.Delta(delta);
        foreach (var effect in effects) effect.Update(delta);
        effects.RemoveAll(effect => effect.Remaining <= 0);
    }
    public void Clear() => effects.Clear();
}
