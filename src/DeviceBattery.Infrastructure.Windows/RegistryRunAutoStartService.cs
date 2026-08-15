using Microsoft.Win32;
using DeviceBattery.Application;

namespace DeviceBattery.Infrastructure.Windows;

public sealed class RegistryRunAutoStartService(string executablePath) : IAutoStartService
{
    public const string ValueName = "DeviceBatteryWidget";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private readonly string command = FormatCommand(executablePath);

    public bool IsEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return string.Equals(key?.GetValue(ValueName) as string, command, StringComparison.OrdinalIgnoreCase);
    }

    public void SetEnabled(bool enabled)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
            ?? throw new InvalidOperationException("Unable to open the current-user Run registry key.");
        if (enabled) key.SetValue(ValueName, command, RegistryValueKind.String);
        else key.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    public static string FormatCommand(string executablePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        if (executablePath.Contains('"')) throw new ArgumentException("Executable path cannot contain a quote.", nameof(executablePath));
        return $"\"{Path.GetFullPath(executablePath)}\" --autostart";
    }
}
