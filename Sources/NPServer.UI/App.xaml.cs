using NPServer.UI.Enums;
using NPServer.Application.Main;

namespace NPServer.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private Theme _theme = Theme.Light;
    private readonly string _title = $"NPServer ({ServiceController.VersionInfo})";

    public void Initialize()
    {
        Current.MainWindow.Title = _title;
    }

    public void ChangeTheme(Theme theme)
    {
        _theme = theme;
        string themeFile = $"{theme}Theme.xaml";
        System.Windows.ResourceDictionary newTheme = new()
        {
            Source = new System.Uri($"pack://siteoforigin:,,,/Resources/Xaml/{themeFile}")
        };

        Current.Resources.MergedDictionaries.Clear();
        Current.Resources.MergedDictionaries.Add(newTheme);
    }

    public Theme GetCurrentTheme() => _theme;
}
