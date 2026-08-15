using System.IO;
using System.Text.Json;

namespace DeviceBattery.Presentation.Wpf;

public sealed record WidgetSettings(double? Left = null, double? Top = null, bool IsTopmost = false);

public sealed class JsonWidgetSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string path;

    public JsonWidgetSettingsStore(string? path = null)
    {
        this.path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DeviceBatteryWidget",
            "settings.json");
    }

    public WidgetSettings Load()
    {
        try
        {
            if (!File.Exists(path)) return new();
            return JsonSerializer.Deserialize<WidgetSettings>(File.ReadAllText(path), JsonOptions) ?? new();
        }
        catch (IOException) { return new(); }
        catch (UnauthorizedAccessException) { return new(); }
        catch (JsonException) { return new(); }
    }

    public void Save(WidgetSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        string temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(temporaryPath, path, true);
    }
}
