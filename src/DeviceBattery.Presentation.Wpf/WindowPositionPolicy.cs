using System.Windows;

namespace DeviceBattery.Presentation.Wpf;

public static class WindowPositionPolicy
{
    public static Point Restore(double? savedLeft, double? savedTop, Size windowSize, IReadOnlyList<Rect> workingAreas)
    {
        if (workingAreas.Count == 0) return new(0, 0);
        Rect target = workingAreas[0];
        if (savedLeft.HasValue && savedTop.HasValue)
        {
            Point savedPosition = new(savedLeft.Value, savedTop.Value);
            foreach (Rect area in workingAreas)
            {
                if (!area.Contains(savedPosition)) continue;
                target = area;
                break;
            }
        }

        double width = Math.Max(0, windowSize.Width);
        double height = Math.Max(0, windowSize.Height);
        double left = Math.Clamp(savedLeft ?? target.Left, target.Left, Math.Max(target.Left, target.Right - width));
        double top = Math.Clamp(savedTop ?? target.Top, target.Top, Math.Max(target.Top, target.Bottom - height));
        return new(left, top);
    }
}
