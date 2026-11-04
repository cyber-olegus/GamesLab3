using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GamesLab3.Core.Common;
using GamesLab3.Core.Timed;
using GamesLab3.WpfShared;

namespace TimedClicker.Wpf;

public sealed class MainWindow : Window
{
    private readonly TimedController controller = new();
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(33) };
    private readonly Stopwatch watch = Stopwatch.StartNew();
    private TimeSpan previous;
    private readonly Canvas scene = new() { Background = Brushes.White, ClipToBounds = true, MinHeight = 220 };
    private readonly TextBlock timeText, scoreText, cooldownText, effectsText;
    private readonly TextBlock status = UiKit.Text("Выберите режим и нажмите «Начать раунд».");
    private readonly CheckBox bonuses = new() { Content = "Бонусные сферы и перезарядка", IsChecked = true,
        VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
    private readonly ComboBox duration = new() { Width = 110, Margin = new Thickness(0, 0, 12, 0) };

    public MainWindow()
    {
        Title = "Лаба 3 · Кликер на время"; Width = 1050; Height = 760; MinWidth = 820; MinHeight = 650;
        Background = UiKit.Brush("#F1F5F9"); FontFamily = new FontFamily("Segoe UI");
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var root = new DockPanel { Margin = new Thickness(24) };
        Content = root;
        var header = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
        header.Children.Add(UiKit.Text("Поймай сферу", 30));
        header.Children.Add(UiKit.Text("Задания 1–2 · Абстракция, наследование и полиморфизм", 14, "#64748B"));
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        var bar = new WrapPanel { Margin = new Thickness(0, 0, 0, 18) };
        foreach (double value in new[] { 30d, 60d, 90d })
            duration.Items.Add(new ComboBoxItem { Content = $"{value:0} секунд", Tag = value });
        duration.SelectedIndex = 0;
        bar.Children.Add(duration); bar.Children.Add(UiKit.Button("Начать раунд", Start_Click)); bar.Children.Add(bonuses);
        DockPanel.SetDock(bar, Dock.Top); root.Children.Add(bar);
        var footer = UiKit.Card(status, new Thickness(0, 16, 0, 0));
        DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
        root.Children.Add(grid);
        grid.Children.Add(UiKit.Card(scene, new Thickness(0, 0, 18, 0)));
        var sidebar = new StackPanel();
        timeText = UiKit.Stat(sidebar, "Осталось времени");
        scoreText = UiKit.Stat(sidebar, "Очки");
        cooldownText = UiKit.Stat(sidebar, "Перезарядка клика");
        sidebar.Children.Add(UiKit.Text("БОНУСЫ", 13, "#64748B"));
        effectsText = UiKit.Text(""); effectsText.Margin = new Thickness(0, 8, 0, 16); sidebar.Children.Add(effectsText);
        sidebar.Children.Add(UiKit.Text("Фиолетовая · обычные очки\nЖёлтая · +100 очков\nГолубая · быстрый клик\nЗелёная · дольше живут сферы\nОранжевая · чаще появляются сферы", 13, "#64748B"));
        var right = UiKit.Card(new ScrollViewer { Content = sidebar, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        Grid.SetColumn(right, 1); grid.Children.Add(right);
        scene.SizeChanged += (_, _) => ResizeScene();
        scene.MouseLeftButtonDown += Scene_Click;
        timer.Tick += Tick;
        Closed += (_, _) => { timer.Stop(); watch.Stop(); };
        Loaded += (_, _) => { ResizeScene(); previous = watch.Elapsed; timer.Start(); RefreshUi(); };
    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        double seconds = (double)((ComboBoxItem)duration.SelectedItem).Tag;
        controller.Start(seconds, bonuses.IsChecked == true);
        previous = watch.Elapsed;
        RefreshUi();
    }
    private void ResizeScene()
    {
        if (scene.ActualWidth > 0 && scene.ActualHeight > 0)
            controller.ResizeScene(scene.ActualWidth, scene.ActualHeight);
    }
    private void Scene_Click(object sender, MouseButtonEventArgs e)
    {
        AdvanceClock();
        Point point = e.GetPosition(scene);
        controller.TryClick(new ScenePoint(point.X, point.Y));
        e.Handled = true; RefreshUi();
    }
    private void AdvanceClock()
    {
        TimeSpan now = watch.Elapsed;
        controller.Update((now - previous).TotalSeconds);
        previous = now;
    }
    private void Tick(object? sender, EventArgs e) { AdvanceClock(); RefreshUi(); }
    private void RefreshUi()
    {
        timeText.Text = $"{controller.RemainingTime:0.0} с";
        scoreText.Text = $"{controller.Player.Points:0.0}";
        cooldownText.Text = controller.BonusesEnabled ? $"{controller.Player.ClickTimer.Remaining:0.00} с" : "Без перезарядки";
        effectsText.Text = UiKit.Effects(controller);
        status.Text = controller.LastAction;
        scene.Children.Clear();
        if (!controller.IsRunning)
        {
            var caption = UiKit.Text(controller.RemainingTime <= 0 && controller.Duration > 0
                ? "Раунд завершён. Начните новый!" : "Нажмите «Начать раунд».", 21, "#64748B");
            Canvas.SetLeft(caption, 20); Canvas.SetTop(caption, 20); scene.Children.Add(caption);
        }
        SceneRenderer.Spheres(scene, controller);
    }
}
