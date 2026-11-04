using GamesLab3.Core.Collectables;
using GamesLab3.Core.Common;
using GamesLab3.Core.Models;
using GamesLab3.Core.Timed;

namespace GamesLab3.Checks;

internal static class TimedChecks
{
    [Check("Редкие и частые обновления дают одинаковое состояние сцены")]
    public static void FrameRateIndependence()
    {
        var one = new TimedController(new Random(42));
        var many = new TimedController(new Random(42));
        one.Start(60, false); many.Start(60, false);
        one.Update(20.1);
        for (int i = 0; i < 201; i++) many.Update(.1);
        Verify.Equal(one.Objects.Count, many.Objects.Count);
        for (int i = 0; i < one.Objects.Count; i++)
        {
            Verify.Equal(one.Objects[i].Title, many.Objects[i].Title);
            Verify.Near(one.Objects[i].RemainingLifetime, many.Objects[i].RemainingLifetime);
            Verify.Near(one.Objects[i].Position.X, many.Objects[i].Position.X);
        }
    }

    [Check("Бонус времени жизни влияет на новые сферы")]
    public static void LongerLifetime()
    {
        var normal = new TimedController(new Random(5));
        var boosted = new TimedController(new Random(5));
        normal.Start(30, false); boosted.Start(30, false);
        boosted.Effects.Add(EffectKind.SphereLifetime, 2, 10, "Время жизни");
        normal.Update(1.1); boosted.Update(1.1);
        Verify.Near(normal.Objects[0].RemainingLifetime * 2, boosted.Objects[0].RemainingLifetime);
    }

    [Check("Бонус появления уменьшает интервал создания сфер")]
    public static void FasterSpawning()
    {
        var game = new TimedController(); game.Start(30, false);
        game.Effects.Add(EffectKind.SpawnInterval, .5, 10, "Больше сфер");
        game.Update(1.1); Verify.Equal(2, game.Objects.Count);
    }

    private static ScenePoint Center(CCollectable sphere) =>
        new(sphere.Position.X + sphere.Size / 2, sphere.Position.Y + sphere.Size / 2);

    [Check("Таймер: отдельный отсчёт и готовность после истечения")]
    public static void Timer()
    {
        var timer = new CCountdownTimer();
        Verify.True(timer.IsReady);
        timer.Start(.8); timer.Update(.3); Verify.Near(.5, timer.Remaining);
        Verify.True(!timer.IsReady);
        timer.Update(2); Verify.True(timer.IsReady); Verify.Near(0, timer.Remaining);
        Verify.Throws<ArgumentOutOfRangeException>(() => timer.Update(-.1));
    }
    [Check("Сфера: проверяется окружность, а не квадрат вокруг неё")]
    public static void Geometry()
    {
        var sphere = new CPointGiver(new(10, 20), 40, 5);
        Verify.True(sphere.Contains(new(30, 40)));
        Verify.True(sphere.Contains(30, 40));
        Verify.True(!sphere.Contains(new(10, 20)));
        Verify.True(sphere.Contains(new(50, 40)));
    }
    [Check("Сфера: сбор один раз и исчезновение по времени жизни")]
    public static void CollectOnce()
    {
        var controller = new TimedController();
        var sphere = new CPointGiver(new(0, 0), 40, 5, 10);
        Verify.True(sphere.OnClick(controller.Player, controller, new(20, 20)));
        Verify.True(!sphere.OnClick(controller.Player, controller, new(20, 20)));
        Verify.Near(10, controller.Player.Points);
        var expired = new CPointBonus(new(0, 0), 40, 1);
        expired.Update(1);
        Verify.True(!expired.OnClick(controller.Player, controller, new(20, 20)));
    }
    [Check("Режим задания 1: очки без перезарядки")]
    public static void BasicMode()
    {
        var game = new TimedController(new Random(10));
        game.Start(30, bonusesEnabled: false); game.Update(2.3);
        Verify.Equal(2, game.Objects.Count);
        var target = game.Objects[^1];
        Verify.True(game.TryClick(Center(target)));
        Verify.True(game.Player.Points > 0);
        Verify.True(game.Player.ClickTimer.IsReady);
        Verify.Equal(1, game.Objects.Count);
    }
    [Check("Режим задания 2: повторный клик блокируется таймером")]
    public static void Cooldown()
    {
        var game = new TimedController(new Random(2));
        game.Start(30, true); game.Update(2.3);
        Verify.True(game.TryClick(Center(game.Objects[^1])));
        Verify.True(!game.Player.ClickTimer.IsReady);
        int count = game.Objects.Count;
        Verify.True(!game.TryClick(Center(game.Objects[0])));
        Verify.Equal(count, game.Objects.Count);
        game.Update(.81);
        Verify.True(game.Player.ClickTimer.IsReady);
    }
    [Check("Сферы появляются полностью внутри сцены")]
    public static void SpawnBounds()
    {
        var game = new TimedController(new Random(24));
        game.ResizeScene(240, 180); game.Start(60);
        for (int i = 0; i < 400; i++)
        {
            game.Update(.1);
            foreach (var sphere in game.Objects)
            {
                Verify.True(sphere.Position.X >= 0 && sphere.Position.Y >= 0);
                Verify.True(sphere.Position.X + sphere.Size <= 240.000001);
                Verify.True(sphere.Position.Y + sphere.Size <= 180.000001);
            }
        }
        game.ResizeScene(20, 20);
        Verify.Equal(0, game.Objects.Count);
        game.Update(2); Verify.Equal(0, game.Objects.Count);
    }
    [Check("Конец раунда: клики и появление сфер прекращаются")]
    public static void RoundEnd()
    {
        var game = new TimedController(); game.Start(3); game.Update(1.2);
        Verify.True(game.Objects.Count > 0);
        game.Update(10);
        Verify.True(!game.IsRunning); Verify.Near(0, game.RemainingTime);
        Verify.Equal(0, game.Objects.Count); Verify.True(!game.TryClick(new(0, 0)));
        game.Update(100); Verify.Equal(0, game.Objects.Count);
    }
    [Check("Новый раунд сбрасывает очки, таймер и эффекты")]
    public static void Restart()
    {
        var game = new TimedController(); game.Start();
        game.Player.AddPoints(100); game.Player.Effects.Add(EffectKind.Cooldown, .5, 10, "Бонус");
        game.Player.TryBeginClick(); game.Start(60, false);
        Verify.Near(0, game.Player.Points); Verify.True(game.Player.ClickTimer.IsReady);
        Verify.Equal(0, game.Player.Effects.Active.Count); Verify.Near(60, game.RemainingTime);
    }
    [Check("Полиморфизм: пять наследников обрабатываются общим типом")]
    public static void AllSpheres()
    {
        var game = new TimedController();
        CCollectable[] spheres =
        [
            new CPointGiver(new(0,0),40,5,10), new CPointBonus(new(0,0),40,5),
            new CCooldownReducer(new(0,0),40,5), new CLifetimeExtender(new(0,0),40,5),
            new CSpawnAccelerator(new(0,0),40,5)
        ];
        foreach (CCollectable sphere in spheres)
            Verify.True(sphere.OnClick(game.Player, game, new(20,20)));
        Verify.Near(110, game.Player.Points);
        Verify.Near(.48, game.Player.EffectiveCooldown);
        Verify.Near(.715, game.SpawnInterval);
        Verify.Equal(2, game.Effects.Active.Count);
        Verify.Equal(1, game.Player.Effects.Active.Count);
    }
    [Check("Временные эффекты истекают независимо друг от друга")]
    public static void IndependentEffects()
    {
        var effects = new BonusEffects();
        effects.Add(EffectKind.Cooldown, .5, 3, "A");
        effects.Add(EffectKind.Damage, 2, 8, "B");
        effects.Update(4); Verify.Equal(1, effects.Active.Count);
        Verify.Near(1, effects.Multiplier(EffectKind.Cooldown));
        Verify.Near(2, effects.Multiplier(EffectKind.Damage));
        effects.Update(4); Verify.Equal(0, effects.Active.Count);
    }
    [Check("Однотипные эффекты складываются умножением и истекают отдельно")]
    public static void StackedEffects()
    {
        var p = new Player();
        p.Effects.Add(EffectKind.Damage, 2, 3, "A"); p.Effects.Add(EffectKind.Damage, 2, 8, "B");
        Verify.Equal("40", p.DealDamage().ToString());
        p.Update(4); Verify.Equal("20", p.DealDamage().ToString());
        p.Update(4); Verify.Equal("10", p.DealDamage().ToString());
    }
}
