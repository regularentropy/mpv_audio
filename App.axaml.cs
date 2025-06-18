using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using mpv_audio.ViewModels;
using mpv_audio.Views;

namespace mpv_audio;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        BindingPlugins.DataValidators.RemoveAt(0);

        MainWindow mainWindow = new();
        mainWindow.DataContext = new MainWindowViewModel(mainWindow);
        desktop.MainWindow = mainWindow;

        base.OnFrameworkInitializationCompleted();
    }
}