using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Wolf.Desktop.Services;
using Wolf.Desktop.ViewModels;
using Wolf.Desktop.Views;

namespace Wolf.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceLocator.Initialize();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var loginVm = new LoginViewModel();
            var loginWindow = new LoginWindow { DataContext = loginVm };

            loginVm.LoginSucceeded += () =>
            {
                var mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };
                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                loginWindow.Close();
            };

            desktop.MainWindow = loginWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}
