using System.Collections.ObjectModel;
using DeviceBattery.Application;
using DeviceBattery.Domain;

namespace DeviceBattery.Presentation.Wpf;

public sealed class WidgetViewModel : ObservableObject
{
    private readonly Dictionary<DeviceKey, DeviceCardViewModel> cards = [];
    private bool isTopmost;
    public ObservableCollection<DeviceCardViewModel> Devices { get; } = [];
    public bool HasDevices => Devices.Count > 0;
    public bool IsEmpty => !HasDevices;
    public bool IsTopmost { get => isTopmost; set => SetProperty(ref isTopmost, value); }

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
        if (dualSense is not null) projected = projected.Prepend(dualSense);
        DeviceCardViewModel[] result = projected.ToArray();

        if (Devices.SequenceEqual(result)) return;
        Devices.Clear();
        foreach (DeviceCardViewModel card in result) Devices.Add(card);
        RaiseCollectionState();
    }

    private static int Priority(DeviceCardViewModel card) =>
        card.Key.StableId.StartsWith("USB-", StringComparison.Ordinal) ? 2 : 1;
    private const string BleGattBatteryProviderId = "BleGattBattery";
    private const string GamingInputBatteryProviderId = "WindowsGamingInputBattery";
    private void RaiseCollectionState() { RaisePropertyChanged(nameof(HasDevices)); RaisePropertyChanged(nameof(IsEmpty)); }
}
