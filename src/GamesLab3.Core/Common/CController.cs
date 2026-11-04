using GamesLab3.Core.Collectables;
using GamesLab3.Core.Models;

namespace GamesLab3.Core.Common;

/// <summary>Общее управление сценой для обоих приложений.</summary>
public abstract class CController
{
    // Один список общего базового типа — полиморфизм времени выполнения.
    private readonly List<CCollectable> objects = new();
    protected readonly Random Random;
    private double spawnElapsed;
    public double SceneWidth { get; private set; } = 700;
    public double SceneHeight { get; private set; } = 400;
    public Player Player { get; protected set; } = new();
    public BonusEffects Effects { get; } = new();
    public IReadOnlyList<CCollectable> Objects => objects.AsReadOnly();
    public bool IsRunning { get; protected set; }
    public abstract double BaseSpawnInterval { get; }
    public double SpawnInterval => Math.Max(0.2, BaseSpawnInterval * Effects.Multiplier(EffectKind.SpawnInterval));
    public string LastAction { get; protected set; } = "Нажмите «Начать».";

    protected CController(Random? random = null) => Random = random ?? new Random();
    public void ResizeScene(double width, double height)
    {
        SceneWidth = Guard.Positive(width, nameof(width));
        SceneHeight = Guard.Positive(height, nameof(height));
        // После уменьшения окна удаляем объекты вне сцены: невидимых целей не остаётся.
        objects.RemoveAll(o => o.Position.X + o.Size > width || o.Position.Y + o.Size > height);
    }

    protected void ResetScene()
    {
        objects.Clear(); Effects.Clear(); Player = new Player(); spawnElapsed = 0;
    }

    protected void UpdateScene(double delta)
    {
        // Обрабатываем границы появления и истечения эффекта последовательно.
        // Даже после задержки окна старые сферы успеют прожить свой срок,
        // а не появятся одновременно с полным временем жизни.
        double remaining = Guard.Delta(delta);
        while (remaining > 0)
        {
            double interval = SpawnInterval;
            double untilSpawn = interval - spawnElapsed;
            if (untilSpawn <= 0.000000001)
            {
                spawnElapsed = Math.Max(0, spawnElapsed - interval);
                Spawn();
                continue;
            }
            double step = Math.Min(remaining, untilSpawn);
            if (Effects.Active.Count > 0)
                step = Math.Min(step, Effects.Active.Min(effect => effect.Remaining));
            Player.Update(step);
            Effects.Update(step);
            foreach (var sphere in objects) sphere.Update(step);
            objects.RemoveAll(sphere => sphere.IsExpired);
            spawnElapsed += step;
            remaining = Math.Max(0, remaining - step);
        }
        while (spawnElapsed + 0.000000001 >= SpawnInterval)
        {
            double interval = SpawnInterval;
            spawnElapsed = Math.Max(0, spawnElapsed - interval);
            Spawn();
        }
    }

    private void Spawn()
    {
        if (objects.Count >= 20 || SceneWidth < 42 || SceneHeight < 42) return;
        double size = Math.Min(42 + Random.NextDouble() * 24, Math.Min(SceneWidth, SceneHeight));
        double life = Math.Min(18, (4 + Random.NextDouble() * 2) * Effects.Multiplier(EffectKind.SphereLifetime));
        // Полный диаметр остаётся внутри сцены, включая правую и нижнюю границы.
        var point = new ScenePoint(Random.NextDouble() * (SceneWidth - size), Random.NextDouble() * (SceneHeight - size));
        objects.Add(CreateCollectable(point, size, life));
    }

    protected bool TryCollect(ScenePoint point, bool usesCooldown)
    {
        if (usesCooldown && !Player.ClickTimer.IsReady) return false;
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            CCollectable sphere = objects[i];
            if (!sphere.OnClick(Player, this, point)) continue;
            objects.RemoveAt(i);
            if (usesCooldown) Player.TryBeginClick();
            LastAction = $"Собрано: {sphere.Title}.";
            return true;
        }
        return false;
    }

    protected void ClearObjects() => objects.Clear();
    protected abstract CCollectable CreateCollectable(ScenePoint point, double size, double lifetime);
    public abstract void Update(double delta);
    public abstract bool TryClick(ScenePoint point);
}
