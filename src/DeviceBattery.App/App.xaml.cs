using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
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
    private readonly object resumeSync = new();
    private readonly Stopwatch startupClock = new();
    private DeviceStateCoordinator? coordinator;
    private IBatteryProvider[] providers = [];
    private WidgetWindow? window;
    private TrayIconController? tray;
    private Task? coordinatorTask;
    private Task[] providerTasks = [];
    private CancellationTokenSource? resumeRefreshCancellation;
    private Task resumeRefreshTask = Task.CompletedTask;
    private BoundedFileTraceListener? traceListener;
    private int firstAvailableLogged;
    private int shutdownStarted;
    private bool allowWindowClose;
    private bool windowPlacementRestored;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        startupClock.Start();

        try
        {
            traceListener = new BoundedFileTraceListener();
            Trace.Listeners.Add(traceListener);
            Trace.WriteLine($"APP_START pid={Environment.ProcessId}");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            traceListener = null;
            Trace.WriteLine($"Diagnostics initialization failed: {error.GetType().Name}");
        }

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
                LogReduction(result);
            }).Task),
            (_, error) =>
            {
                System.Diagnostics.Trace.WriteLine($"Coordinator event error: {error.GetType().Name}");
                return ValueTask.CompletedTask;
            });
        providers = CreateProviders(e.Args);
        Trace.WriteLine($"PROVIDERS mode={GetOption(e.Args, "--providers") ?? "all"} count={providers.Length}");
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        coordinatorTask = coordinator.RunAsync();
        providerTasks = providers.Select(activeProvider => ProviderRunner.RunIsolatedAsync(
            activeProvider,
            coordinator.Events,
            lifetime.Token,
            (failedProvider, error) =>
            {
                Trace.WriteLine($"PROVIDER_ISOLATED provider={failedProvider.ProviderId} error={error.GetType().Name}");
                return ValueTask.CompletedTask;
            })).ToArray();

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        tray.SetTopmost(viewModel.IsTopmost);
        tray.SetAutoStart(GetAutoStart());
        window.Show();
        Trace.WriteLine($"WIDGET_VISIBLE pid={Environment.ProcessId} elapsedMs={startupClock.Elapsed.TotalMilliseconds:F1}");

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

    private async Task ShutdownAsync(string reason)
    {
        _ = reason;
        if (Interlocked.CompareExchange(ref shutdownStarted, 1, 0) != 0)
            return;

        lifetime.Cancel();
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        CancellationTokenSource? refreshCancellation;
        Task refreshTask;
        lock (resumeSync)
        {
            refreshCancellation = resumeRefreshCancellation;
            refreshTask = resumeRefreshTask;
            resumeRefreshCancellation = null;
            resumeRefreshTask = Task.CompletedTask;
        }
        refreshCancellation?.Cancel();
        await refreshTask;
        refreshCancellation?.Dispose();

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
        Trace.WriteLine("APP_STOP");
        if (traceListener is not null)
        {
            Trace.Listeners.Remove(traceListener);
            traceListener.Dispose();
            traceListener = null;
        }
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

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode != PowerModes.Resume || lifetime.IsCancellationRequested) return;
        lock (resumeSync)
        {
            resumeRefreshCancellation?.Cancel();
            resumeRefreshCancellation?.Dispose();
            resumeRefreshCancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
            resumeRefreshTask = RefreshProvidersAfterResumeAsync(resumeRefreshCancellation.Token);
        }
    }

    private async Task RefreshProvidersAfterResumeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            foreach (IRefreshableBatteryProvider provider in providers.OfType<IRefreshableBatteryProvider>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                try { await provider.RefreshAsync(cancellationToken); }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    System.Diagnostics.Trace.WriteLine($"Resume refresh failed for {((IBatteryProvider)provider).ProviderId}: {error.GetType().Name}");
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

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
        string? value = GetOption(args, "--smoke-seconds");
        if (value is null)
            return false;
        if (!int.TryParse(value, out int seconds) || seconds is < 3 or > 600)
            throw new ArgumentException("--smoke-seconds must be between 3 and 600.");
        duration = TimeSpan.FromSeconds(seconds);
        return true;
    }

    private static IBatteryProvider[] CreateProviders(string[] args)
    {
        string selection = (GetOption(args, "--providers") ?? "all").Trim().ToLowerInvariant();
        return selection switch
        {
            "none" => [],
            "dualsense" => [new DualSenseHidProvider()],
            "dualsense+ble" => [new DualSenseHidProvider(), new BleGattBatteryProvider()],
            "all" => [new DualSenseHidProvider(), new BleGattBatteryProvider(), new GamingInputBatteryProvider()],
            _ => throw new ArgumentException("--providers must be one of: none, dualsense, dualsense+ble, all.")
        };
    }

    private static string? GetOption(string[] args, string name)
    {
        for (int index = 0; index < args.Length - 1; index++)
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
                return args[index + 1];
        return null;
    }

    private void LogReduction(ReductionResult result)
    {
        if (result.Snapshot is { } snapshot)
        {
            Trace.WriteLine($"STATE outcome={result.Outcome} device={snapshot.Key} availability={snapshot.Battery.Availability} percent={snapshot.Battery.Percent?.ToString() ?? "null"} charging={snapshot.Battery.Charging}");
            if (snapshot.Battery.Availability == DeviceBattery.Domain.BatteryAvailability.Available &&
                Interlocked.CompareExchange(ref firstAvailableLogged, 1, 0) == 0)
                Trace.WriteLine($"FIRST_DEVICE_AVAILABLE pid={Environment.ProcessId} elapsedMs={startupClock.Elapsed.TotalMilliseconds:F1}");
            return;
        }
        if (result.RemovedKey is { } removedKey)
            Trace.WriteLine($"STATE outcome={result.Outcome} device={removedKey}");
    }
}
