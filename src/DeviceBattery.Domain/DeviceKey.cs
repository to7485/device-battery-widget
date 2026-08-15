namespace DeviceBattery.Domain;

public sealed record DeviceKey
{
    public DeviceKey(string providerId, string stableId)
    {
        ProviderId = RequireValue(providerId, nameof(providerId));
        StableId = RequireValue(stableId, nameof(stableId));
    }

    public string ProviderId { get; }
    public string StableId { get; }

    public override string ToString() => $"{ProviderId}:{StableId}";

    private static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value must not be empty.", parameterName)
            : value.Trim();
}
