using System.Windows;
using System.Windows.Controls;

namespace TimedClicker.Wpf;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "Лабораторная работа №3 — общая основа";
        Width = 740; Height = 400; WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Content = new TextBlock { Text = "Общая основа готова. Добавьте свою часть из пакета лабораторной работы.",
            FontSize = 22, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(32), VerticalAlignment = VerticalAlignment.Center };
    }
}
