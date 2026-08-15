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
        if (cards.TryGetValue(snapshot.Key, out DeviceCardViewModel? card)) { card.Apply(snapshot); return; }
        card = new(snapshot);
        cards.Add(snapshot.Key, card);
        Devices.Add(card);
        RaiseCollectionState();
    }

    private void Remove(DeviceKey key)
    {
        if (!cards.Remove(key, out DeviceCardViewModel? card)) return;
        Devices.Remove(card);
        RaiseCollectionState();
    }
    private void RaiseCollectionState() { RaisePropertyChanged(nameof(HasDevices)); RaisePropertyChanged(nameof(IsEmpty)); }
}
