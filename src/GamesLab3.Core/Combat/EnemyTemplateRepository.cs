using System.Text.Json;
using GamesLab3.Core.Models;

namespace GamesLab3.Core.Combat;

public sealed class EnemyTemplateRepository
{
    private readonly Random random;
    public IReadOnlyList<CEnemyTemplate> Templates { get; private set; } = Array.AsReadOnly(Array.Empty<CEnemyTemplate>());
    public IReadOnlyList<double> NormalizedChances { get; private set; } = Array.AsReadOnly(Array.Empty<double>());
    public EnemyTemplateRepository(Random? random = null) => this.random = random ?? new Random();

    public void LoadFromJson(string path) => LoadFromJsonText(File.ReadAllText(path));
    public void LoadFromJsonText(string json)
    {
        using (JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip
        }))
        {
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                throw new JsonException("Корень JSON должен быть массивом.");
            string[] required = ["Name", "IconName", "BaseLife", "BaseGold", "LifeModifier", "GoldModifier", "SpawnChance"];
            foreach (JsonElement item in document.RootElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    throw new JsonException("Каждый шаблон должен быть объектом.");
                foreach (string field in required)
                    if (!item.EnumerateObject().Any(p => string.Equals(p.Name, field, StringComparison.OrdinalIgnoreCase)
                        && p.Value.ValueKind != JsonValueKind.Null))
                        throw new JsonException($"Не заполнено обязательное поле {field}.");
            }
        }
        List<CEnemyTemplate>? loaded;
        try
        {
            loaded = JsonSerializer.Deserialize<List<CEnemyTemplate>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });
        }
        catch (ArgumentException exception)
        {
            throw new JsonException("Некорректные поля шаблона.", exception);
        }
        if (loaded is null || loaded.Count == 0 || loaded.Any(t => t is null))
            throw new JsonException("Ожидается непустой массив шаблонов противников.");
        if (loaded.Select(t => t.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != loaded.Count)
            throw new JsonException("Имена противников не должны повторяться.");
        double sum = loaded.Sum(t => t.SpawnChance);
        if (!double.IsFinite(sum) || sum <= 0)
            throw new JsonException("Сумма шансов должна быть больше нуля.");
        double[] chances = loaded.Select(t => t.SpawnChance / sum).ToArray();
        Templates = loaded.AsReadOnly(); NormalizedChances = Array.AsReadOnly(chances);
    }

    public CEnemyTemplate SelectRandom() => FindByChance(random.NextDouble());
    public CEnemyTemplate FindByChance(double chance)
    {
        if (!double.IsFinite(chance) || chance < 0 || chance >= 1) throw new ArgumentOutOfRangeException(nameof(chance));
        if (Templates.Count == 0) throw new InvalidOperationException("Сначала загрузите шаблоны.");
        double cumulative = 0;
        int last = -1;
        for (int i = 0; i < Templates.Count; i++)
        {
            if (NormalizedChances[i] <= 0) continue;
            last = i; cumulative += NormalizedChances[i];
            if (chance < cumulative) return Templates[i];
        }
        return Templates[last]; // Закрываем возможный зазор округления double.
    }
}
