using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Wolf.Desktop.Services;
using Wolf.Desktop.ViewModels;

namespace Wolf.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();

        // Allow dragging the borderless window by the top area
        var dragRegion = this.FindControl<Border>("DragRegion");
        if (dragRegion is not null)
            dragRegion.PointerPressed += OnDragRegionPressed;

        // Window control buttons
        var minimizeButton = this.FindControl<Button>("MinimizeButton");
        if (minimizeButton is not null)
            minimizeButton.Click += (_, _) => WindowState = WindowState.Minimized;

        var closeButton = this.FindControl<Button>("CloseButton");
        if (closeButton is not null)
            closeButton.Click += (_, _) => Close();

        // Apply current mode colors
        ApplyModeColors();

        // Shift+DoubleClick on logo toggles Full mode
        var logoBorder = this.FindControl<Border>("LogoBorder");
        if (logoBorder is not null)
        {
            logoBorder.PointerPressed += (s, e) =>
            {
                if (e.ClickCount == 2 && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                {
                    ServiceLocator.IsFullMode = !ServiceLocator.IsFullMode;
                    ApplyModeColors();
                }
            };
        }

        DataContextChanged += OnDataContextChanged;
    }

    private void ApplyModeColors()
    {
        var isFullMode = ServiceLocator.IsFullMode;
        var accent = Color.Parse(isFullMode ? "#3b6fa0" : "#c45d2c");
        var glowColor = Color.Parse(isFullMode ? "#443b6fa0" : "#44c45d2c");
        var brush = new SolidColorBrush(accent);

        var logoBorder = this.FindControl<Border>("LogoBorder");
        if (logoBorder is not null) logoBorder.Background = new SolidColorBrush(accent);

        var loginButton = this.FindControl<Button>("LoginButton");
        if (loginButton is not null) loginButton.Background = new SolidColorBrush(accent);

        var splashLogo = this.FindControl<Border>("SplashLogo");
        if (splashLogo is not null) splashLogo.Background = new SolidColorBrush(accent);

        var glowRing = this.FindControl<Border>("GlowRing");
        if (glowRing is not null) glowRing.Background = new SolidColorBrush(glowColor);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.LoginFailed += TriggerShake;
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(LoginViewModel.ShowSplash) && vm.ShowSplash)
                    ActivateSplash();
            };
        }
    }

    private void OnDragRegionPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void ActivateSplash()
    {
        // Add "fade-out" to login content
        var loginContent = this.FindControl<StackPanel>("LoginContent");
        loginContent?.Classes.Add("fade-out");

        // Activate splash animations by adding "active" class
        var splashPanel = this.FindControl<StackPanel>("SplashPanel");
        splashPanel?.Classes.Add("active");

        var splashLogo = this.FindControl<Border>("SplashLogo");
        splashLogo?.Classes.Add("active");

        var glowRing = this.FindControl<Border>("GlowRing");
        glowRing?.Classes.Add("active");

        var splashTitle = this.FindControl<TextBlock>("SplashTitle");
        splashTitle?.Classes.Add("active");
    }

    private void TriggerShake()
    {
        var card = this.FindControl<Border>("LoginCard");
        if (card is null) return;

        // Toggle shake class off then on to re-trigger animation
        card.Classes.Remove("shake");
        card.Classes.Add("shake");

        // Remove class after animation completes so it can be re-triggered
        var timer = new System.Timers.Timer(450) { AutoReset = false };
        timer.Elapsed += (_, _) =>
            Avalonia.Threading.Dispatcher.UIThread.Post(() => card.Classes.Remove("shake"));
        timer.Start();
    }
}
