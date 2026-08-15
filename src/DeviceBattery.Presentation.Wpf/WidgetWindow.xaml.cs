using System.Windows;
using System.Windows.Input;

namespace DeviceBattery.Presentation.Wpf;

public partial class WidgetWindow : Window
{
    public WidgetWindow() => InitializeComponent();
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
