using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using DeviceBattery.Application;
using DeviceBattery.Infrastructure.Windows;
using DeviceBattery.Presentation.Wpf;

namespace DeviceBattery.App;

public partial class App : System.Windows.Application
{
    private readonly CancellationTokenSource lifetime = new();
    private readonly DeviceStateReducer reducer = new();
    private readonly WidgetViewModel viewModel = new();
    private readonly JsonWidgetSettingsStore settingsStore = new();
    private readonly IAutoStartService autoStartService = new RegistryRunAutoStartService(Environment.ProcessPath ?? throw new InvalidOperationException("Executable path is unavailable."));
    private DeviceStateCoordinator? coordinator;
    private IBatteryProvider[] providers = [];
    private WidgetWindow? window;
    private TrayIconController? tray;
    private Task? coordinatorTask;
    private Task[] providerTasks = [];
    private int shutdownStarted;
    private bool allowWindowClose;
    private bool windowPlacementRestored;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        WidgetSettings settings = settingsStore.Load();
        viewModel.IsTopmost = settings.IsTopmost;
        viewModel.SetHiddenDeviceKeys(settings.HiddenDeviceKeys);
        window = new WidgetWindow { DataContext = viewModel };
        window.ContentRendered += (_, _) => RestoreWindowPlacement(settings);
        window.Closing += OnWindowClosing;
        window.StateChanged += OnWindowStateChanged;
        tray = new TrayIconController(
            ShowWidget,
            ToggleTopmost,
            ToggleDeviceVisibility,
            GetAutoStart,
            ToggleAutoStart,
            () => _ = ShutdownAsync("Tray Exit"));

        coordinator = new DeviceStateCoordinator(
            reducer,
            result => new ValueTask(Dispatcher.InvokeAsync(() =>
            {
                viewModel.Apply(result);
                tray?.SetDevices(viewModel.GetDeviceCatalog());
            }).Task),
            (_, error) =>
            {
                System.Diagnostics.Trace.WriteLine($"Coordinator event error: {error.GetType().Name}");
                return ValueTask.CompletedTask;
            });
        providers = [new DualSenseHidProvider(), new BleGattBatteryProvider(), new GamingInputBatteryProvider()];
        coordinatorTask = coordinator.RunAsync();
        providerTasks = providers.Select(activeProvider => RunProviderAsync(activeProvider, coordinator)).ToArray();

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        tray.SetTopmost(viewModel.IsTopmost);
        tray.SetAutoStart(GetAutoStart());
        window.Show();

        if (TryGetSmokeDuration(e.Args, out TimeSpan duration))
        {
            var timer = new DispatcherTimer { Interval = duration };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                _ = ShutdownAsync("Timed Smoke");
            };
            timer.Start();
        }
    }

    private async Task RunProviderAsync(IBatteryProvider activeProvider, DeviceStateCoordinator activeCoordinator)
    {
        try
        {
            await activeProvider.RunAsync(activeCoordinator.Events, lifetime.Token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"Provider stopped: {ex.GetType().Name}");
            System.Diagnostics.Trace.WriteLine($"Provider {activeProvider.ProviderId} isolated after failure.");
        }
    }

    private async Task ShutdownAsync(string reason)
    {
        _ = reason;
        if (Interlocked.CompareExchange(ref shutdownStarted, 1, 0) != 0)
            return;

        lifetime.Cancel();

        if (providerTasks.Length > 0)
            await Task.WhenAll(providerTasks);
        coordinator?.Complete();
        if (coordinatorTask is not null)
            await coordinatorTask;
        foreach (IBatteryProvider activeProvider in providers)
            await activeProvider.DisposeAsync();
        providers = [];

        tray?.Dispose();
        tray = null;

        viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        if (window is not null)
        {
            SaveSettings();
            window.Closing -= OnWindowClosing;
            window.StateChanged -= OnWindowStateChanged;
            allowWindowClose = true;
            window.Close();
            window = null;
        }

        lifetime.Dispose();
        Shutdown();
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (allowWindowClose)
            return;
        e.Cancel = true;
        window?.Hide();
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (window?.WindowState == WindowState.Minimized)
            window.Hide();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WidgetViewModel.IsTopmost))
        {
            tray?.SetTopmost(viewModel.IsTopmost);
            SaveSettings();
        }
    }

    private void ShowWidget()
    {
        if (window is null)
            return;
        window.Show();
        window.WindowState = WindowState.Normal;
        window.Activate();
    }

    private void ToggleTopmost() => viewModel.IsTopmost = !viewModel.IsTopmost;

    private bool GetAutoStart()
    {
        try { return autoStartService.IsEnabled(); }
        catch (Exception error) when (error is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            System.Diagnostics.Trace.WriteLine($"Auto-start read failed: {error.GetType().Name}");
            return false;
        }
    }

    private void ToggleAutoStart()
    {
        try
        {
            bool enabled = !autoStartService.IsEnabled();
            autoStartService.SetEnabled(enabled);
            tray?.SetAutoStart(enabled);
        }
        catch (Exception error) when (error is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            System.Diagnostics.Trace.WriteLine($"Auto-start update failed: {error.GetType().Name}");
            tray?.SetAutoStart(GetAutoStart());
        }
    }

    private void ToggleDeviceVisibility(string key)
    {
        viewModel.ToggleDeviceVisibility(key);
        SaveSettings();
    }

    private void RestoreWindowPlacement(WidgetSettings settings)
    {
        if (window is null || windowPlacementRestored) return;
        windowPlacementRestored = true;
        var areas = System.Windows.Forms.Screen.AllScreens
            .Select(screen => new Rect(screen.WorkingArea.X, screen.WorkingArea.Y, screen.WorkingArea.Width, screen.WorkingArea.Height))
            .ToArray();
        System.Windows.Point position = WindowPositionPolicy.Restore(
            settings.Left,
            settings.Top,
            new System.Windows.Size(window.ActualWidth, window.ActualHeight),
            areas);
        window.Left = position.X;
        window.Top = position.Y;
    }

    private void SaveSettings()
    {
        if (window is null || !windowPlacementRestored) return;
        try { settingsStore.Save(new(window.Left, window.Top, viewModel.IsTopmost, viewModel.HiddenDeviceKeys.Order().ToArray())); }
        catch (IOException error) { System.Diagnostics.Trace.WriteLine($"Settings save failed: {error.GetType().Name}"); }
        catch (UnauthorizedAccessException error) { System.Diagnostics.Trace.WriteLine($"Settings save failed: {error.GetType().Name}"); }
    }

    private static bool TryGetSmokeDuration(string[] args, out TimeSpan duration)
    {
        duration = default;
        if (args.Length != 2 || !string.Equals(args[0], "--smoke-seconds", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!int.TryParse(args[1], out int seconds) || seconds is < 3 or > 600)
            throw new ArgumentException("--smoke-seconds must be between 3 and 600.");
        duration = TimeSpan.FromSeconds(seconds);
        return true;
    }
}
