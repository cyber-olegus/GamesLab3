using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GamesLab3.Core.Common;

namespace GamesLab3.WpfShared;

internal static class UiKit
{
    public static Brush Brush(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;
    public static TextBlock Text(string value, double size = 14, string color = "#0F172A") =>
        new() { Text = value, FontSize = size, Foreground = Brush(color), TextWrapping = TextWrapping.Wrap };
    public static Border Card(UIElement child, Thickness? margin = null) =>
        new() { Child = child, Background = Brushes.White, CornerRadius = new CornerRadius(14),
            Padding = new Thickness(20), Margin = margin ?? new Thickness(0) };
    public static Button Button(string text, RoutedEventHandler action)
    {
        var button = new Button { Content = text, Padding = new Thickness(16, 9, 16, 9),
            Margin = new Thickness(0, 0, 10, 0), FontSize = 14, Background = Brush("#4F46E5"),
            Foreground = Brushes.White, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand };
        button.Click += action;
        return button;
    }
    public static TextBlock Stat(Panel panel, string caption)
    {
        panel.Children.Add(Text(caption, 13, "#64748B"));
        var value = Text("—", 23);
        value.FontWeight = FontWeights.SemiBold;
        value.Margin = new Thickness(0, 3, 0, 13);
        panel.Children.Add(value);
        return value;
    }
    public static string Effects(CController controller)
    {
        var effects = controller.Player.Effects.Active.Concat(controller.Effects.Active).ToArray();
        return effects.Length == 0 ? "Активных бонусов пока нет." :
            string.Join(Environment.NewLine, effects.Select(e => $"{e.Name} · {e.Remaining:0.0} с"));
    }
}
