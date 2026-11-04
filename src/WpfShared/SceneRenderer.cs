using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GamesLab3.Core.Common;

namespace GamesLab3.WpfShared;

internal static class SceneRenderer
{
    public static void Spheres(Canvas canvas, CController controller)
    {
        foreach (var sphere in controller.Objects)
        {
            var view = new Grid { Width = sphere.Size, Height = sphere.Size, ToolTip = sphere.Description };
            view.Children.Add(new Ellipse { Fill = UiKit.Brush(sphere.Color), Stroke = Brushes.White, StrokeThickness = 2 });
            view.Children.Add(new TextBlock { Text = sphere.Label, Foreground = Brushes.White,
                FontWeight = FontWeights.Bold, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false });
            Canvas.SetLeft(view, sphere.Position.X); Canvas.SetTop(view, sphere.Position.Y);
            canvas.Children.Add(view);
        }
    }

    public static UIElement EnemySilhouette(double size)
    {
        // Встроенный рисунок позволяет играть и без внешних PNG.
        var grid = new Grid { Width = size, Height = size, Background = Brushes.Transparent };
        grid.Children.Add(new Ellipse { Width = size * .8, Height = size * .8,
            Fill = UiKit.Brush("#818CF8"), Stroke = UiKit.Brush("#4338CA"), StrokeThickness = 3 });
        var eyes = new StackPanel { Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        for (int i = 0; i < 2; i++)
            eyes.Children.Add(new Ellipse { Width = size * .12, Height = size * .16,
                Fill = Brushes.White, Stroke = UiKit.Brush("#312E81"), StrokeThickness = 5, Margin = new Thickness(8) });
        grid.Children.Add(eyes);
        return grid;
    }
}
