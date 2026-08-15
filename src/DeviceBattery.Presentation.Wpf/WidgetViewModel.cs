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
        ShowBestCard(card);
    }

    private void Remove(DeviceKey key)
    {
        if (!cards.Remove(key, out DeviceCardViewModel? card)) return;
        Devices.Remove(card);
        if (Devices.Count == 0 && cards.Count > 0)
            Devices.Add(cards.Values.OrderByDescending(Priority).First());
        RaiseCollectionState();
    }

    private void ShowBestCard(DeviceCardViewModel candidate)
    {
        DeviceCardViewModel? current = Devices.FirstOrDefault();
        if (current == candidate) return;
        if (current is not null && Priority(current) > Priority(candidate)) return;
        Devices.Clear();
        Devices.Add(candidate);
        RaiseCollectionState();
    }

    private static int Priority(DeviceCardViewModel card) =>
        card.Key.StableId.StartsWith("USB-", StringComparison.Ordinal) ? 2 : 1;
    private void RaiseCollectionState() { RaisePropertyChanged(nameof(HasDevices)); RaisePropertyChanged(nameof(IsEmpty)); }
}
