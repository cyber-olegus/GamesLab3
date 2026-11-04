using GamesLab3.Core.Collectables;
using GamesLab3.Core.Common;
using GamesLab3.Core.Models;
using GamesLab3.Core.Numbers;

namespace GamesLab3.Core.Combat;

/// <summary>Адаптация механик TimedController к игре с противниками Лабы 2.</summary>
public sealed class BattleController : CController
{
    private EnemyTemplateRepository? repository;
    private readonly EnemyFactory factory = new();
    public Enemy? CurrentEnemy { get; private set; }
    public int Victories { get; private set; }
    public int TemplateCount => repository?.Templates.Count ?? 0;
    public override double BaseSpawnInterval => 3.0;
    public double EnemySize => Math.Min(180, Math.Min(SceneWidth, SceneHeight));
    public ScenePoint EnemyPosition => new((SceneWidth - EnemySize) / 2, (SceneHeight - EnemySize) / 2);
    public BattleController(Random? random = null) : base(random) { }

    public void LoadFromJson(string path)
    {
        var candidate = new EnemyTemplateRepository(Random);
        candidate.LoadFromJson(path);
        Start(candidate);
    }
    public void LoadFromJsonText(string json)
    {
        var candidate = new EnemyTemplateRepository(Random);
        candidate.LoadFromJsonText(json);
        Start(candidate);
    }
    public void Restart()
    {
        if (repository is null) throw new InvalidOperationException("Сначала выберите JSON.");
        Start(repository);
    }
    private void Start(EnemyTemplateRepository source)
    {
        Enemy first = factory.Create(source.SelectRandom(), 0);
        ResetScene();
        repository = source; CurrentEnemy = first; Victories = 0; IsRunning = true;
        LastAction = "Игра началась. Атакуйте противника и собирайте бонусы.";
    }

    public override void Update(double delta)
    {
        Guard.Delta(delta);
        if (IsRunning) UpdateScene(delta);
    }
    public override bool TryClick(ScenePoint point)
    {
        if (!IsRunning) return false;
        // Сферы поверх противника имеют приоритет. Сбор не наносит скрытый удар
        // и не тратит перезарядку атаки: бонус можно взять даже во время ожидания.
        if (TryCollect(point, usesCooldown: false)) return true;
        ScenePoint p = EnemyPosition;
        if (point.X < p.X || point.X > p.X + EnemySize || point.Y < p.Y || point.Y > p.Y + EnemySize)
            return false;
        return TryAttackEnemy();
    }

    public bool TryAttackEnemy()
    {
        if (!IsRunning || CurrentEnemy is null || repository is null) return false;
        if (!Player.ClickTimer.IsReady)
        {
            LastAction = $"Перезарядка: ещё {Player.ClickTimer.Remaining:0.00} с.";
            return false;
        }
        BigNumber damage = Player.DealDamage();
        Enemy? next = null;
        if (damage >= CurrentEnemy.CurrentHitPoints)
            next = factory.Create(repository.SelectRandom(), checked(Victories + 1));
        if (!Player.TryBeginClick()) return false;
        string name = CurrentEnemy.Name;
        if (CurrentEnemy.TakeDamage(damage, out BigNumber reward))
        {
            Player.AddGold(reward); Victories++; CurrentEnemy = next!;
            LastAction = $"Победа над «{name}»: +{reward} золота.";
        }
        else LastAction = $"Удар: {damage} урона.";
        return true;
    }

    public bool TryUpgradeDamage()
    {
        bool success = IsRunning && Player.TryUpgrade();
        LastAction = success ? "Урон улучшен." : "Недостаточно золота для улучшения урона.";
        return success;
    }
    public bool TryUpgradeCooldown()
    {
        bool success = IsRunning && Player.TryUpgradeCooldown();
        LastAction = success ? "Перезарядка следующих атак уменьшена." :
            Player.CanUpgradeCooldown ? "Недостаточно золота для ускорения." : "Достигнута минимальная базовая перезарядка.";
        return success;
    }

    protected override CCollectable CreateCollectable(ScenePoint p, double size, double life) =>
        Random.Next(3) switch
        {
            0 => new CDamageBooster(p, size, life),
            1 => new CCooldownReducer(p, size, life),
            _ => new CGoldBonus(p, size, life, new BigNumber(10) + (CurrentEnemy?.GoldReward ?? BigNumber.Zero))
        };
}
