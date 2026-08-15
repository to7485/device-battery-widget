using System.Collections.ObjectModel;
using DeviceBattery.Application;
using DeviceBattery.Domain;

namespace DeviceBattery.Presentation.Wpf;

public sealed class WidgetViewModel : ObservableObject
{
    private readonly Dictionary<DeviceKey, DeviceCardViewModel> cards = [];
    private readonly HashSet<string> hiddenDeviceKeys = new(StringComparer.Ordinal);
    private bool isTopmost;
    public ObservableCollection<DeviceCardViewModel> Devices { get; } = [];
    public bool HasDevices => Devices.Count > 0;
    public bool IsEmpty => !HasDevices;
    public bool IsTopmost { get => isTopmost; set => SetProperty(ref isTopmost, value); }
    public IReadOnlyCollection<string> HiddenDeviceKeys => hiddenDeviceKeys;

    public void SetHiddenDeviceKeys(IEnumerable<string>? keys)
    {
        hiddenDeviceKeys.Clear();
        if (keys is not null)
            foreach (string key in keys.Where(value => !string.IsNullOrWhiteSpace(value)))
                hiddenDeviceKeys.Add(key);
        RebuildProjection();
    }

    public bool ToggleDeviceVisibility(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        bool hidden = !hiddenDeviceKeys.Remove(key);
        if (hidden) hiddenDeviceKeys.Add(key);
        RebuildProjection();
        return hidden;
    }

    public IReadOnlyList<DeviceCatalogItem> GetDeviceCatalog() => GetProjectionCandidates()
        .OrderBy(card => card.DisplayName, StringComparer.CurrentCultureIgnoreCase)
        .Select(card => new DeviceCatalogItem(card.Key.ToString(), card.DisplayName, hiddenDeviceKeys.Contains(card.Key.ToString())))
        .ToArray();

    public void Apply(ReductionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Outcome == ReductionOutcome.Removed && result.RemovedKey is not null) { Remove(result.RemovedKey); return; }
        DeviceSnapshot? snapshot = result.Snapshot;
        if (result.Outcome != ReductionOutcome.Applied || snapshot is null) return;
        if (!snapshot.IsVisible) { Remove(snapshot.Key); return; }
        if (cards.TryGetValue(snapshot.Key, out DeviceCardViewModel? card)) card.Apply(snapshot);
        else { card = new(snapshot); cards.Add(snapshot.Key, card); }
        RebuildProjection();
    }

    private void Remove(DeviceKey key)
    {
        if (!cards.Remove(key, out DeviceCardViewModel? card)) return;
        RebuildProjection();
    }

    private void RebuildProjection()
    {
        DeviceCardViewModel[] result = GetProjectionCandidates()
            .Where(card => !hiddenDeviceKeys.Contains(card.Key.ToString()))
            .ToArray();

        if (Devices.SequenceEqual(result)) return;
        Devices.Clear();
        foreach (DeviceCardViewModel card in result) Devices.Add(card);
        RaiseCollectionState();
    }

    private IEnumerable<DeviceCardViewModel> GetProjectionCandidates()
    {
        DeviceCardViewModel? dualSense = cards.Values
            .Where(card => card.Key.ProviderId == "DualSenseHid")
            .OrderByDescending(Priority)
            .FirstOrDefault();
        HashSet<string> bleNames = cards.Values
            .Where(card => card.Key.ProviderId == BleGattBatteryProviderId)
            .Select(card => card.DisplayName)
            .ToHashSet(StringComparer.CurrentCultureIgnoreCase);
        IEnumerable<DeviceCardViewModel> projected = cards.Values
            .Where(card => card.Key.ProviderId != "DualSenseHid")
            .Where(card => card.Key.ProviderId != GamingInputBatteryProviderId || !bleNames.Contains(card.DisplayName))
            .OrderBy(card => card.DisplayName, StringComparer.CurrentCultureIgnoreCase);
        return dualSense is null ? projected : projected.Prepend(dualSense);
    }

    private static int Priority(DeviceCardViewModel card) =>
        card.Key.StableId.StartsWith("USB-", StringComparison.Ordinal) ? 2 : 1;
    private const string BleGattBatteryProviderId = "BleGattBattery";
    private const string GamingInputBatteryProviderId = "WindowsGamingInputBattery";
    private void RaiseCollectionState() { RaisePropertyChanged(nameof(HasDevices)); RaisePropertyChanged(nameof(IsEmpty)); }
}

public sealed record DeviceCatalogItem(string Key, string DisplayName, bool IsHidden);
