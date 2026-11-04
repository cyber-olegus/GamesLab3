using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GamesLab3.Core.Common;
using GamesLab3.Core.Combat;
using GamesLab3.WpfShared;
using Microsoft.Win32;

namespace EnemyClicker.Wpf;

public sealed class MainWindow : Window
{
    private readonly BattleController controller = new();
    private readonly EnemyIconLoader icons = new();
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(33) };
    private readonly Stopwatch watch = new();
    private TimeSpan previous;
    private readonly Canvas scene = new() { Background = Brushes.White, ClipToBounds = true, MinHeight = 220 };
    private readonly TextBlock enemyText = UiKit.Text("Откройте JSON с противниками.", 21);
    private readonly TextBlock status = UiKit.Text("");
    private readonly TextBlock source = UiKit.Text("JSON ещё не выбран.", 12, "#64748B");
    private readonly TextBlock goldText, damageText, healthText, cooldownText, levelText, effectsText;
    private readonly Button damageButton, cooldownButton;
    private string? loadedPath;

    public MainWindow()
    {
        Title = "Лаба 3 · Противники, перезарядка и бонусы"; Width = 1100; Height = 850;
        MinWidth = 850; MinHeight = 700; Background = UiKit.Brush("#F1F5F9");
        FontFamily = new FontFamily("Segoe UI"); WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var root = new DockPanel { Margin = new Thickness(24) }; Content = root;
        var header = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
        header.Children.Add(UiKit.Text("Битва с противниками", 30));
        header.Children.Add(UiKit.Text("Задание 3 · Перезарядка, улучшения и независимые бонусы", 14, "#64748B"));
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        var bar = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
        var buttons = new WrapPanel();
        buttons.Children.Add(UiKit.Button("Открыть JSON · новая игра", (_, _) => ChooseFile()));
        buttons.Children.Add(UiKit.Button("Начать заново", (_, _) => Run(() => { AdvanceClock(); controller.Restart(); })));
        bar.Children.Add(buttons); bar.Children.Add(source); source.Margin = new Thickness(0, 8, 0, 0);
        DockPanel.SetDock(bar, Dock.Top); root.Children.Add(bar);
        DockPanel.SetDock(status, Dock.Bottom); status.Margin = new Thickness(0, 16, 0, 0); root.Children.Add(status);
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(310) });
        root.Children.Add(grid);
        var arena = new DockPanel();
        enemyText.Margin = new Thickness(0, 0, 0, 12); DockPanel.SetDock(enemyText, Dock.Top); arena.Children.Add(enemyText);
        var legend = UiKit.Text("Красная сфера: урон ×2 · Голубая: ускорение · Жёлтая: золото\nБонус можно собрать во время перезарядки. Сам сбор не атакует противника.", 13, "#64748B");
        legend.Margin = new Thickness(0, 12, 0, 0); DockPanel.SetDock(legend, Dock.Bottom); arena.Children.Add(legend);
        arena.Children.Add(scene); grid.Children.Add(UiKit.Card(arena, new Thickness(0, 0, 18, 0)));
        var panel = new StackPanel();
        healthText = UiKit.Stat(panel, "Здоровье противника");
        goldText = UiKit.Stat(panel, "Золото");
        damageText = UiKit.Stat(panel, "Урон за удар");
        levelText = UiKit.Stat(panel, "Уровень и победы");
        cooldownText = UiKit.Stat(panel, "Перезарядка / ожидание");
        damageButton = UiKit.Button("Улучшить урон", (_, _) => Run(() => { AdvanceClock(); controller.TryUpgradeDamage(); }));
        cooldownButton = UiKit.Button("Ускорить атаку", (_, _) => Run(() => { AdvanceClock(); controller.TryUpgradeCooldown(); }));
        damageButton.Margin = new Thickness(0, 0, 0, 10); cooldownButton.Margin = new Thickness(0, 0, 0, 16);
        panel.Children.Add(damageButton); panel.Children.Add(cooldownButton);
        panel.Children.Add(UiKit.Text("АКТИВНЫЕ БОНУСЫ", 13, "#64748B"));
        effectsText = UiKit.Text(""); effectsText.Margin = new Thickness(0, 8, 0, 0); panel.Children.Add(effectsText);
        var right = UiKit.Card(new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        Grid.SetColumn(right, 1); grid.Children.Add(right);
        scene.SizeChanged += (_, _) => ResizeScene(); scene.MouseLeftButtonDown += Scene_Click;
        timer.Tick += (_, _) => { AdvanceClock(); RefreshUi(); };
        Closed += (_, _) => { timer.Stop(); watch.Stop(); };
        Loaded += (_, _) =>
        {
            ResizeScene(); watch.Start(); previous = watch.Elapsed; timer.Start(); RefreshUi(); ChooseFile();
        };
    }

    private void ChooseFile()
    {
        AdvanceClock(); timer.Stop(); watch.Stop();
        try
        {
            string data = Path.Combine(AppContext.BaseDirectory, "Data");
            var dialog = new OpenFileDialog { Title = "Шаблоны из редактора Лабы 1",
                Filter = "Шаблоны противников (*.json)|*.json", CheckFileExists = true,
                InitialDirectory = Directory.Exists(data) ? data : AppContext.BaseDirectory,
                FileName = "enemies.demo.json" };
            if (dialog.ShowDialog(this) == true)
                Run(() => { controller.LoadFromJson(dialog.FileName); loadedPath = dialog.FileName; icons.Clear(); });
        }
        finally { watch.Start(); previous = watch.Elapsed; timer.Start(); RefreshUi(); }
    }
    private void Run(Action action)
    {
        try { action(); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException
            or ArgumentException or InvalidOperationException or OverflowException)
        {
            MessageBox.Show(this, e.Message, "Не удалось выполнить действие", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { RefreshUi(); }
    }
    private void ResizeScene()
    {
        if (scene.ActualWidth > 0 && scene.ActualHeight > 0)
            controller.ResizeScene(scene.ActualWidth, scene.ActualHeight);
    }
    private void AdvanceClock()
    {
        TimeSpan now = watch.Elapsed;
        controller.Update((now - previous).TotalSeconds);
        previous = now;
    }
    private void Scene_Click(object sender, MouseButtonEventArgs e)
    {
        Run(() =>
        {
            AdvanceClock();
            Point p = e.GetPosition(scene);
            controller.TryClick(new ScenePoint(p.X, p.Y));
        });
        e.Handled = true;
    }
    private void RefreshUi()
    {
        var player = controller.Player;
        var enemy = controller.CurrentEnemy;
        enemyText.Text = enemy is null ? "Откройте JSON с противниками." : $"{enemy.Name} · награда {enemy.GoldReward}";
        source.Text = loadedPath is null ? "JSON ещё не выбран." : $"{Path.GetFileName(loadedPath)} · шаблонов: {controller.TemplateCount}";
        healthText.Text = enemy is null ? "—" : $"{enemy.CurrentHitPoints} / {enemy.MaxHitPoints}";
        goldText.Text = player.Gold.ToString(); damageText.Text = player.DealDamage().ToString();
        levelText.Text = $"{player.Level} · побед: {controller.Victories}";
        cooldownText.Text = $"{player.EffectiveCooldown:0.00} с / {player.ClickTimer.Remaining:0.00} с";
        damageButton.Content = $"Урон · {player.UpgradeCost} золота";
        cooldownButton.Content = player.CanUpgradeCooldown ? $"Скорость · {player.CooldownUpgradeCost} золота" : "Скорость улучшена до предела";
        damageButton.IsEnabled = controller.IsRunning && player.Gold >= player.UpgradeCost;
        cooldownButton.IsEnabled = controller.IsRunning && player.CanUpgradeCooldown && player.Gold >= player.CooldownUpgradeCost;
        effectsText.Text = UiKit.Effects(controller); status.Text = controller.LastAction;
        scene.Children.Clear();
        if (enemy is not null)
        {
            double size = controller.EnemySize;
            ImageSource? image = icons.Load(enemy.Icon);
            UIElement view = image is null ? SceneRenderer.EnemySilhouette(size) :
                new Image { Source = image, Width = size, Height = size, Stretch = Stretch.Uniform };
            var target = new Border { Width = size, Height = size, Background = UiKit.Brush("#EEF2FF"),
                CornerRadius = new CornerRadius(18), Child = view, ToolTip = "Нажмите, чтобы атаковать" };
            Canvas.SetLeft(target, controller.EnemyPosition.X); Canvas.SetTop(target, controller.EnemyPosition.Y);
            scene.Children.Add(target);
        }
        SceneRenderer.Spheres(scene, controller);
    }
}
