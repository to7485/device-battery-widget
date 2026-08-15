using System.Drawing;
using System.Windows.Forms;

namespace DeviceBattery.Poc.SystemTrayLifecycle;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());
    }
}

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly PocWidgetForm _widget;
    private readonly NotifyIcon _trayIcon;
    private readonly ToolStripMenuItem _alwaysOnTopItem;
    private readonly ToolStripMenuItem _autoStartItem;
    private bool _disposed;

    public TrayApplicationContext()
    {
        Console.WriteLine("Gate 4 POC-D — System Tray / Lifecycle");
        Console.WriteLine("POC only: no registry, startup, device, or vendor state is changed.");

        _widget = new PocWidgetForm();
        _widget.FormClosed += OnWidgetClosed;
        _widget.HideRequested += (_, _) => HideWidget("Widget minimize/hide");

        var menu = new ContextMenuStrip();
        menu.Items.Add("Widget 표시", null, (_, _) => ShowWidget("Tray menu"));

        _alwaysOnTopItem = new ToolStripMenuItem("Always On Top", null, ToggleAlwaysOnTop)
        {
            CheckOnClick = true
        };
        menu.Items.Add(_alwaysOnTopItem);
        menu.Items.Add("숨긴 장치 관리", null, (_, _) =>
            MessageBox.Show("POC placeholder — no device setting is changed.", "Hidden Devices"));

        _autoStartItem = new ToolStripMenuItem("Windows 로그인 자동 실행", null, ToggleAutoStart)
        {
            CheckOnClick = true
        };
        menu.Items.Add(_autoStartItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("종료", null, (_, _) => ExitApplication("Tray menu Exit"));

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Device Battery Widget — Gate 4 POC",
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => ShowWidget("Tray double-click");

        _widget.Show();
        Log("TRAY_VISIBLE", "Widget and tray icon created");
    }

    private void ShowWidget(string reason)
    {
        if (!_widget.Visible) _widget.Show();
        if (_widget.WindowState == FormWindowState.Minimized)
            _widget.WindowState = FormWindowState.Normal;
        _widget.Activate();
        Log("WIDGET_SHOWN", reason);
    }

    private void HideWidget(string reason)
    {
        _widget.Hide();
        Log("WIDGET_HIDDEN", reason);
    }

    private void ToggleAlwaysOnTop(object? sender, EventArgs e)
    {
        _widget.TopMost = _alwaysOnTopItem.Checked;
        Log("TOPMOST_CHANGED", _widget.TopMost.ToString());
    }

    private void ToggleAutoStart(object? sender, EventArgs e)
    {
        Log("AUTOSTART_POC_ONLY", $"Checked={_autoStartItem.Checked}; registry unchanged");
    }

    private void OnWidgetClosed(object? sender, FormClosedEventArgs e) =>
        ExitApplication("Widget X / FormClosed");

    private void ExitApplication(string reason)
    {
        if (_disposed) return;
        Log("EXIT_REQUESTED", reason);
        DisposeResources();
        ExitThread();
    }

    protected override void ExitThreadCore()
    {
        DisposeResources();
        base.ExitThreadCore();
    }

    private void DisposeResources()
    {
        if (_disposed) return;
        _disposed = true;

        _trayIcon.Visible = false;
        _trayIcon.ContextMenuStrip?.Dispose();
        _trayIcon.Dispose();
        if (!_widget.IsDisposed)
        {
            _widget.FormClosed -= OnWidgetClosed;
            _widget.Dispose();
        }

        Log("CLEANUP", "Tray icon, menu, handlers, and widget disposed");
    }

    private static void Log(string transition, string detail) =>
        Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] [{transition}] {detail}");
}

internal sealed class PocWidgetForm : Form
{
    public event EventHandler? HideRequested;

    public PocWidgetForm()
    {
        Text = "Device Battery Widget — Gate 4 POC";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(390, 180);

        var label = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "System Tray Lifecycle POC\r\n\r\nMinimize: hide widget, keep tray\r\nX: exit application and remove tray icon"
        };
        Controls.Add(label);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized)
            HideRequested?.Invoke(this, EventArgs.Empty);
    }
}
