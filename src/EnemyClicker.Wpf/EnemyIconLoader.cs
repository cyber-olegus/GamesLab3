using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EnemyClicker.Wpf;

internal sealed class EnemyIconLoader
{
    private readonly Dictionary<string, ImageSource?> cache = new(StringComparer.OrdinalIgnoreCase);
    public void Clear() => cache.Clear();
    public ImageSource? Load(string name)
    {
        if (cache.TryGetValue(name, out var saved)) return saved;
        ImageSource? result = null;
        try
        {
            string root = Path.Combine(AppContext.BaseDirectory, "Assets", "Icons");
            if (Path.GetFileName(name) == name && Directory.Exists(root))
            {
                string? path = Directory.EnumerateFiles(root, "*.png", SearchOption.AllDirectories)
                    .FirstOrDefault(p => string.Equals(Path.GetFileName(p), name, StringComparison.OrdinalIgnoreCase));
                if (path is not null)
                {
                    var image = new BitmapImage();
                    image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad;
                    image.DecodePixelWidth = 256; image.UriSource = new Uri(Path.GetFullPath(path), UriKind.Absolute);
                    image.EndInit(); image.Freeze(); result = image;
                }
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException
            or NotSupportedException or InvalidOperationException or FormatException) { /* Встроенный рисунок заменяет недоступную PNG. */ }
        cache[name] = result;
        return result;
    }
}
