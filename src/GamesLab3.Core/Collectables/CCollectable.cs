using GamesLab3.Core.Common;
using GamesLab3.Core.Models;

namespace GamesLab3.Core.Collectables;

/// <summary>Общие положение, размер, время жизни и проверка клика.
/// Самого «бонуса вообще» не существует, поэтому класс и OnClick абстрактные.</summary>
public abstract class CCollectable
{
    public ScenePoint Position { get; }
    public double Size { get; }
    public double RemainingLifetime { get; private set; }
    public bool IsExpired => RemainingLifetime <= 0;
    public abstract string Title { get; }
    public abstract string Color { get; }
    public virtual string Description => Title;
    public virtual string Label => "";

    protected CCollectable(ScenePoint position, double size, double lifetime)
    {
        if (!double.IsFinite(position.X) || !double.IsFinite(position.Y))
            throw new ArgumentOutOfRangeException(nameof(position));
        Position = position;
        Size = Guard.Positive(size, nameof(size));
        RemainingLifetime = Guard.Positive(lifetime, nameof(lifetime));
    }

    public bool Contains(ScenePoint point)
    {
        double dx = point.X - (Position.X + Size / 2);
        double dy = point.Y - (Position.Y + Size / 2);
        return dx * dx + dy * dy <= Size * Size / 4;
    }
    // Перегрузка метода: компилятор выбирает версию по типам аргументов.
    public bool Contains(double x, double y) => Contains(new ScenePoint(x, y));

    public void Update(double delta) =>
        RemainingLifetime = Math.Max(0, RemainingLifetime - Guard.Delta(delta));

    // Сигнатура одинакова для всех наследников. Контроллер не проверяет тип сферы.
    public abstract bool OnClick(Player player, CController controller, ScenePoint mousePosition);

    protected bool Activate(ScenePoint point, Action effect)
    {
        if (IsExpired || !Contains(point)) return false;
        effect();
        RemainingLifetime = 0; // Собранный объект нельзя активировать повторно.
        return true;
    }
}
