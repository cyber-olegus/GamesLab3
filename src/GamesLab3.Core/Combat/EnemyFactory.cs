using GamesLab3.Core.Models;
using GamesLab3.Core.Numbers;
namespace GamesLab3.Core.Combat;

public sealed class EnemyFactory
{
    public Enemy Create(CEnemyTemplate template, int victories)
    {
        ArgumentNullException.ThrowIfNull(template);
        if (victories < 0) throw new ArgumentOutOfRangeException(nameof(victories));
        BigNumber life = new(template.BaseLife), gold = new(template.BaseGold);
        for (int i = 0; i < victories; i++)
        {
            life = life * template.LifeModifier;
            gold = gold * template.GoldModifier;
        }
        if (life == BigNumber.Zero) life = BigNumber.One;
        return new Enemy(template.Name, life, gold, template.IconName);
    }
}
