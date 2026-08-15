using System.ComponentModel;
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
    private DeviceStateCoordinator? coordinator;
    private DualSenseHidProvider? provider;
    private WidgetWindow? window;
    private TrayIconController? tray;
    private Task? coordinatorTask;
    private Task? providerTask;
    private int shutdownStarted;
    private bool allowWindowClose;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        window = new WidgetWindow { DataContext = viewModel };
        window.Closing += OnWindowClosing;
        window.StateChanged += OnWindowStateChanged;
        tray = new TrayIconController(
            ShowWidget,
            ToggleTopmost,
            () => _ = ShutdownAsync("Tray Exit"));

        coordinator = new DeviceStateCoordinator(
            reducer,
            result => new ValueTask(Dispatcher.InvokeAsync(() => viewModel.Apply(result)).Task),
            (_, error) =>
            {
                System.Diagnostics.Trace.WriteLine($"Coordinator event error: {error.GetType().Name}");
                return ValueTask.CompletedTask;
            });
        provider = new DualSenseHidProvider();
        coordinatorTask = coordinator.RunAsync();
        providerTask = RunProviderAsync(provider, coordinator);

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        tray.SetTopmost(viewModel.IsTopmost);
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

    private async Task RunProviderAsync(DualSenseHidProvider activeProvider, DeviceStateCoordinator activeCoordinator)
    {
        try
        {
            await activeProvider.RunAsync(activeCoordinator.Events, lifetime.Token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"Provider stopped: {ex.GetType().Name}");
            await Dispatcher.InvokeAsync(() => _ = ShutdownAsync("Provider Failure"));
        }
    }

    private async Task ShutdownAsync(string reason)
    {
        _ = reason;
        if (Interlocked.CompareExchange(ref shutdownStarted, 1, 0) != 0)
            return;

        lifetime.Cancel();

        if (providerTask is not null)
            await providerTask;
        coordinator?.Complete();
        if (coordinatorTask is not null)
            await coordinatorTask;
        if (provider is not null)
            await provider.DisposeAsync();

        tray?.Dispose();
        tray = null;

        viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        if (window is not null)
        {
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
        _ = ShutdownAsync("Widget X");
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (window?.WindowState == WindowState.Minimized)
            window.Hide();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WidgetViewModel.IsTopmost))
            tray?.SetTopmost(viewModel.IsTopmost);
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
