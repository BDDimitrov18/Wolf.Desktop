using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;

namespace Wolf.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _showSplash;

    public event Action? LoginSucceeded;
    public event Action? LoginFailed;

    private const int MinSplashMs = 2500;

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Моля, въведете потребителско име и парола.";
            HasError = true;
            LoginFailed?.Invoke();
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        HasError = false;

        try
        {
            var success = await ServiceLocator.Auth.LoginAsync(Username, Password);
            if (success)
            {
                // Switch to splash screen
                ShowSplash = true;
                IsBusy = false;

                // Load data while splash animates
                var sw = Stopwatch.StartNew();
                await ServiceLocator.Cache.LoadAllAsync();
                await ServiceLocator.Realtime.StartAsync();
                sw.Stop();

                // Ensure minimum splash duration
                var remaining = MinSplashMs - (int)sw.ElapsedMilliseconds;
                if (remaining > 0)
                    await Task.Delay(remaining);

                LoginSucceeded?.Invoke();
            }
            else
            {
                ErrorMessage = "Невалидно потребителско име или парола.";
                HasError = true;
                IsBusy = false;
                LoginFailed?.Invoke();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Грешка при свързване: {ex.Message}";
            HasError = true;
            ShowSplash = false;
            IsBusy = false;
            LoginFailed?.Invoke();
        }
    }
}
