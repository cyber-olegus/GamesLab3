using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GamesLab3.Core.Assets;

namespace EnemyClicker.Wpf;

internal sealed class EnemyIconLoader
{
    private sealed record CachedIcon(ImageSource? Image, string Message, DateTime RetryAfter);
    private readonly EnemyIconAssets assets = new(AppContext.BaseDirectory);
    private readonly Dictionary<string, CachedIcon> cache = new(StringComparer.OrdinalIgnoreCase);
    public string LastError { get; private set; } = "";
    public string StorageDirectory => assets.StorageDirectory;
    public bool SavesToProject => assets.ProjectDirectory is not null;

    public void Clear()
    {
        cache.Clear();
        LastError = "";
    }
    public void SetTemplateFile(string jsonPath)
    {
        assets.SetTemplateFile(jsonPath);
        Clear();
    }
    public int Import(string sourceDirectory)
    {
        try { return assets.Import(sourceDirectory); }
        finally { Clear(); } // После копирования не оставляем старую картинку или отрицательный результат.
    }

    public ImageSource? Load(string name)
    {
        if (cache.TryGetValue(name, out var saved) &&
            (saved.Image is not null || DateTime.UtcNow < saved.RetryAfter))
        {
            LastError = saved.Message;
            return saved.Image;
        }

        ImageSource? result = null;
        string message = "";
        try
        {
            string? path = assets.Find(name);
            if (path is null)
                message = $"Не найдена «{name}». Нажмите «Импортировать иконки» и выберите папку из Лабы 2.";
            else
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.DecodePixelWidth = 256;
                image.UriSource = new Uri(Path.GetFullPath(path), UriKind.Absolute);
                image.EndInit();
                image.Freeze(); // Изображение в памяти; PNG не остаётся заблокированной.
                result = image;
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException
            or NotSupportedException or InvalidOperationException or FormatException)
        {
            message = $"Не удалось открыть «{name}»: {e.Message}";
        }
        LastError = message;
        // Неудачу кешируем ненадолго: добавленная во время игры PNG тоже будет найдена.
        cache[name] = new CachedIcon(result, message, DateTime.UtcNow.AddSeconds(1));
        return result;
    }
}
