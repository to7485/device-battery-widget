using System.Drawing;
using System.Windows.Forms;
using DeviceBattery.Presentation.Wpf;

namespace DeviceBattery.App;

public sealed class TrayIconController : IDisposable
{
    private readonly NotifyIcon notifyIcon;
    private readonly ToolStripMenuItem topmostItem;
    private readonly ToolStripMenuItem devicesItem;
    private readonly ToolStripMenuItem autoStartItem;
    private readonly Action<string> toggleDeviceVisibility;
    private readonly Func<bool> getAutoStart;
    private DeviceCatalogItem[] currentDevices = [];
    private bool keepMenuOpen;
    private bool disposed;

    public TrayIconController(Action show, Action toggleTopmost, Action<string> toggleDeviceVisibility, Func<bool> getAutoStart, Action toggleAutoStart, Action exit)
    {
        ArgumentNullException.ThrowIfNull(show);
        ArgumentNullException.ThrowIfNull(toggleTopmost);
        ArgumentNullException.ThrowIfNull(toggleDeviceVisibility);
        ArgumentNullException.ThrowIfNull(getAutoStart);
        ArgumentNullException.ThrowIfNull(toggleAutoStart);
        ArgumentNullException.ThrowIfNull(exit);
        this.toggleDeviceVisibility = toggleDeviceVisibility;
        this.getAutoStart = getAutoStart;

        var menu = new ContextMenuStrip();
        menu.Closing += KeepOpenForDeviceToggle;
        menu.Items.Add("위젯 표시", null, (_, _) => show());
        topmostItem = new ToolStripMenuItem("항상 위") { CheckOnClick = false };
        topmostItem.MouseDown += (_, _) => keepMenuOpen = true;
        topmostItem.Click += (_, _) =>
        {
            toggleTopmost();
            ResetKeepOpenAfterClick();
        };
        menu.Items.Add(topmostItem);
        menu.Items.Add(new ToolStripSeparator());
        devicesItem = new ToolStripMenuItem("장치 표시") { Enabled = false };
        devicesItem.DropDown.Closing += KeepOpenForDeviceToggle;
        menu.Items.Add(devicesItem);
        autoStartItem = new ToolStripMenuItem("Windows 로그인 시 실행") { CheckOnClick = false };
        autoStartItem.MouseDown += (_, _) => keepMenuOpen = true;
        autoStartItem.Click += (_, _) =>
        {
            toggleAutoStart();
            ResetKeepOpenAfterClick();
        };
        menu.Items.Add(autoStartItem);
        menu.Opening += (_, _) => autoStartItem.Checked = this.getAutoStart();
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("종료", null, (_, _) => exit());

        notifyIcon = new NotifyIcon
        {
            Text = "Device Battery Widget",
            Icon = SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true
        };
        notifyIcon.DoubleClick += (_, _) => show();
    }

    public void SetTopmost(bool value) => topmostItem.Checked = value;
    public void SetAutoStart(bool value) => autoStartItem.Checked = value;

    public void SetDevices(IReadOnlyList<DeviceCatalogItem> devices)
    {
        if (currentDevices.SequenceEqual(devices))
            return;
        currentDevices = devices.ToArray();
        devicesItem.DropDownItems.Clear();
        devicesItem.Enabled = devices.Count > 0;
        foreach (DeviceCatalogItem device in devices)
        {
            var item = new ToolStripMenuItem(device.DisplayName)
            {
                Checked = !device.IsHidden,
                CheckOnClick = false,
                ToolTipText = device.IsHidden ? "위젯에 표시하지 않음" : "위젯에 표시 중"
            };
            string key = device.Key;
            item.MouseDown += (_, _) => keepMenuOpen = true;
            item.Click += (_, _) =>
            {
                toggleDeviceVisibility(key);
                item.Checked = !item.Checked;
                item.ToolTipText = item.Checked ? "위젯에 표시 중" : "위젯에 표시하지 않음";
                currentDevices = currentDevices
                    .Select(current => current.Key == key ? current with { IsHidden = !current.IsHidden } : current)
                    .ToArray();
                ResetKeepOpenAfterClick();
            };
            devicesItem.DropDownItems.Add(item);
        }
    }

    private void KeepOpenForDeviceToggle(object? sender, ToolStripDropDownClosingEventArgs e)
    {
        if (keepMenuOpen && e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            e.Cancel = true;
    }

    private void ResetKeepOpenAfterClick() =>
        notifyIcon.ContextMenuStrip?.BeginInvoke(() => keepMenuOpen = false);

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        notifyIcon.Visible = false;
        notifyIcon.ContextMenuStrip?.Dispose();
        notifyIcon.Dispose();
    }
}
