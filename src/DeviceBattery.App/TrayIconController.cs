using System.Drawing;
using System.Windows.Forms;

namespace DeviceBattery.App;

public sealed class TrayIconController : IDisposable
{
    private readonly NotifyIcon notifyIcon;
    private readonly ToolStripMenuItem topmostItem;
    private bool disposed;

    public TrayIconController(Action show, Action toggleTopmost, Action exit)
    {
        ArgumentNullException.ThrowIfNull(show);
        ArgumentNullException.ThrowIfNull(toggleTopmost);
        ArgumentNullException.ThrowIfNull(exit);

        var menu = new ContextMenuStrip();
        menu.Items.Add("위젯 표시", null, (_, _) => show());
        topmostItem = new ToolStripMenuItem("항상 위", null, (_, _) => toggleTopmost()) { CheckOnClick = false };
        menu.Items.Add(topmostItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("숨긴 장치 관리") { Enabled = false });
        menu.Items.Add(new ToolStripMenuItem("Windows 로그인 시 실행") { Enabled = false });
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

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        notifyIcon.Visible = false;
        notifyIcon.ContextMenuStrip?.Dispose();
        notifyIcon.Dispose();
    }
}
