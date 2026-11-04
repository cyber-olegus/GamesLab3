using System.Text.Json;
using GamesLab3.Core.Collectables;
using GamesLab3.Core.Common;
using GamesLab3.Core.Combat;
using GamesLab3.Core.Models;
using GamesLab3.Core.Numbers;

namespace GamesLab3.Checks;

internal static class CombatChecks
{
    [Check("JSON: обязательные поля не подменяются значениями по умолчанию")]
    public static void RequiredFields()
    {
        foreach (string field in new[] { "Name", "IconName", "BaseLife", "BaseGold", "LifeModifier", "GoldModifier", "SpawnChance" })
        {
            var document = System.Text.Json.Nodes.JsonNode.Parse(Json())!.AsArray();
            document[0]!.AsObject().Remove(field);
            Verify.Throws<JsonException>(() => new EnemyTemplateRepository().LoadFromJsonText(document.ToJsonString()));
        }
    }

    private static string Json(int health = 20, int gold = 40) => JsonSerializer.Serialize(new[]
    {
        new { Name="A", IconName="a.png", BaseLife=health, BaseGold=gold,
            LifeModifier=2.0, GoldModifier=2.0, SpawnChance=1.0 }
    });

    [Check("Бой: до загрузки JSON атаки и улучшения недоступны")]
    public static void NotStarted()
    {
        var game = new BattleController();
        Verify.True(!game.TryAttackEnemy()); Verify.True(!game.TryUpgradeDamage());
        Verify.True(!game.TryUpgradeCooldown());
    }
    [Check("Бой: перезарядка не даёт наносить повторные мгновенные удары")]
    public static void CooldownBlocksAttack()
    {
        var game = new BattleController(); game.LoadFromJsonText(Json());
        Verify.True(game.TryAttackEnemy());
        Verify.Equal("10", game.CurrentEnemy!.CurrentHitPoints.ToString());
        Verify.True(!game.TryAttackEnemy());
        Verify.Equal("10", game.CurrentEnemy.CurrentHitPoints.ToString());
        game.Update(.8); Verify.True(game.TryAttackEnemy());
        Verify.Equal(1, game.Victories); Verify.Equal("40", game.Player.Gold.ToString());
        Verify.Equal("40", game.CurrentEnemy.CurrentHitPoints.ToString());
    }
    [Check("Бой: награда за противника возвращается один раз")]
    public static void RewardOnce()
    {
        var enemy = new Enemy("A", new BigNumber(10), new BigNumber(40), "a.png");
        Verify.True(enemy.TakeDamage(new BigNumber(99), out var first));
        Verify.Equal("40", first.ToString());
        Verify.True(!enemy.TakeDamage(BigNumber.One, out var second));
        Verify.Equal("0", second.ToString());
    }
    [Check("Ускорение за золото: списание, нижняя граница и отсутствие бесплатной покупки")]
    public static void UpgradeCooldown()
    {
        var player = new Player(); Verify.True(!player.TryUpgradeCooldown());
        player.AddGold(new BigNumber(30)); Verify.True(player.TryUpgradeCooldown());
        Verify.Equal("0", player.Gold.ToString()); Verify.Near(.68, player.BaseCooldown);
        player.AddGold(new BigNumber("100000000000000000000"));
        for (int i = 0; i < 100; i++) player.TryUpgradeCooldown();
        Verify.Near(.12, player.BaseCooldown);
        var before = player.Gold;
        Verify.True(!player.TryUpgradeCooldown()); Verify.Equal(before, player.Gold);
    }
    [Check("Покупка скорости не сбрасывает уже запущенную перезарядку")]
    public static void UpgradeDoesNotResetTimer()
    {
        var player = new Player(); player.TryBeginClick(); player.AddGold(new BigNumber(30));
        player.TryUpgradeCooldown(); Verify.Near(.8, player.ClickTimer.Remaining);
        player.Update(.8); player.TryBeginClick(); Verify.Near(.68, player.ClickTimer.Remaining);
    }
    [Check("Бой: три бонуса используют общую иерархию сфер")]
    public static void ThreeBonuses()
    {
        var game = new BattleController(); game.LoadFromJsonText(Json());
        CCollectable[] spheres =
        [
            new CDamageBooster(new(0,0),40,5), new CCooldownReducer(new(0,0),40,5),
            new CGoldBonus(new(0,0),40,5,new BigNumber(25))
        ];
        foreach (CCollectable sphere in spheres)
            Verify.True(sphere.OnClick(game.Player, game, new(20,20)));
        Verify.Equal("20", game.Player.DealDamage().ToString());
        Verify.Near(.48, game.Player.EffectiveCooldown);
        Verify.Equal("25", game.Player.Gold.ToString());
        Verify.Equal(2, game.Player.Effects.Active.Count);
        Verify.True(game.TryAttackEnemy()); Verify.Equal(1, game.Victories);
    }
    [Check("Время бонуса истекло: урон и скорость восстановлены")]
    public static void BonusExpires()
    {
        var game = new BattleController(); game.LoadFromJsonText(Json());
        game.Player.Effects.Add(EffectKind.Damage, 2, 2, "Урон");
        game.Player.Effects.Add(EffectKind.Cooldown, .5, 5, "Скорость");
        game.Update(3);
        Verify.Equal("10", game.Player.DealDamage().ToString()); Verify.Near(.4, game.Player.EffectiveCooldown);
        game.Update(2); Verify.Near(.8, game.Player.EffectiveCooldown);
    }
    [Check("Ошибка JSON сохраняет начатую игру")]
    public static void FailedLoad()
    {
        var game = new BattleController(); game.LoadFromJsonText(Json());
        game.TryAttackEnemy(); var enemy = game.CurrentEnemy; var player = game.Player;
        foreach (string invalid in new[] { "broken json", "[]", "{}", "null" })
            Verify.Throws<JsonException>(() => game.LoadFromJsonText(invalid));
        Verify.True(ReferenceEquals(enemy, game.CurrentEnemy));
        Verify.True(ReferenceEquals(player, game.Player));
        Verify.Equal("10", game.CurrentEnemy!.CurrentHitPoints.ToString());
    }
    [Check("Шансы нормализуются, нулевые шансы не выбираются")]
    public static void Probabilities()
    {
        var repository = new EnemyTemplateRepository();
        repository.LoadFromJsonText(JsonSerializer.Serialize(new[]
        {
            new { Name="Zero",IconName="a.png",BaseLife=20,BaseGold=7,LifeModifier=1.0,GoldModifier=1.0,SpawnChance=0.0 },
            new { Name="A",IconName="a.png",BaseLife=20,BaseGold=7,LifeModifier=1.0,GoldModifier=1.0,SpawnChance=0.2 },
            new { Name="B",IconName="b.png",BaseLife=20,BaseGold=7,LifeModifier=1.0,GoldModifier=1.0,SpawnChance=0.3 }
        }));
        Verify.Near(.4, repository.NormalizedChances[1]);
        Verify.Equal("A", repository.FindByChance(0).Name);
        Verify.Equal("B", repository.FindByChance(.4).Name);
        Verify.Equal("B", repository.FindByChance(Math.BitDecrement(1.0)).Name);
    }
    [Check("Перезапуск очищает бонусы, золото, победы и перезарядку")]
    public static void Restart()
    {
        var game = new BattleController(); game.LoadFromJsonText(Json(10));
        game.TryAttackEnemy(); game.Player.Effects.Add(EffectKind.Damage, 2, 10, "Бонус");
        game.Update(3.1); Verify.True(game.Objects.Count > 0);
        game.Restart();
        Verify.Equal("0", game.Player.Gold.ToString()); Verify.Equal(0, game.Victories);
        Verify.True(game.Player.ClickTimer.IsReady); Verify.Equal(0, game.Objects.Count);
        Verify.Equal(0, game.Player.Effects.Active.Count);
    }
    [Check("Клик вне противника не тратит перезарядку")]
    public static void MissDoesNotConsume()
    {
        var game = new BattleController(); game.LoadFromJsonText(Json());
        Verify.True(!game.TryClick(new ScenePoint(-100,-100)));
        Verify.True(game.Player.ClickTimer.IsReady);
    }
    [Check("Сбор бонуса во время перезарядки не наносит удар")]
    public static void CollectDuringCooldown()
    {
        var game = new BattleController(new Random(1)); game.LoadFromJsonText(Json(100));
        game.Update(3); Verify.Equal(1, game.Objects.Count);
        game.TryAttackEnemy();
        string health = game.CurrentEnemy!.CurrentHitPoints.ToString();
        var sphere = game.Objects[0];
        Verify.True(game.TryClick(new(sphere.Position.X + sphere.Size/2, sphere.Position.Y + sphere.Size/2)));
        Verify.Equal(0, game.Objects.Count);
        Verify.Equal(health, game.CurrentEnemy.CurrentHitPoints.ToString());
        Verify.True(!game.Player.ClickTimer.IsReady);
    }
}
