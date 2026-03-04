using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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
                if (ServiceLocator.IsFullMode)
                {
                    var res = Application.Current!.Resources;
                    if (res["BrushAccent"] is SolidColorBrush a) a.Color = Color.Parse("#3b6fa0");
                    if (res["BrushAccentHover"] is SolidColorBrush ah) ah.Color = Color.Parse("#2d5a87");
                    if (res["BrushAccentLight"] is SolidColorBrush al) al.Color = Color.Parse("#edf3f9");
                }

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
