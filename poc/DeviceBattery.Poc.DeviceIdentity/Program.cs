using System.Text;
using Windows.Devices.Enumeration;

internal static class Program
{
    private static readonly string[] RequestedProperties =
    [
        "System.Devices.ContainerId",
        "System.Devices.DeviceInstanceId",
        "System.Devices.FriendlyName",
        "System.Devices.DeviceManufacturer",
        "System.Devices.ModelName",
        "System.Devices.DiscoveryMethod",
        "System.Devices.Connected"
    ];

    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var label = GetArgument(args, "--label") ?? DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var artifacts = Path.Combine(AppContext.BaseDirectory, "artifacts");
        Directory.CreateDirectory(artifacts);

        Console.WriteLine("Gate 4 POC-A05/A06 — Device Identity");
        Console.WriteLine($"Snapshot label: {label}");
        Console.WriteLine();

        var aqs = DeviceInformation.GetAqsFilterFromDeviceClass(DeviceClass.All);

        DeviceInformationCollection devices =
            await DeviceInformation.FindAllAsync(
                aqs,
                RequestedProperties,
                DeviceInformationKind.DeviceInterface);

        var rows = devices
            .Select(ToRow)
            .OrderBy(x => x.ContainerId)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.InterfaceId)
            .ToList();

        var csvPath = Path.Combine(artifacts, $"identity-{Sanitize(label)}.csv");
        WriteCsv(csvPath, rows);

        Console.WriteLine($"Device interfaces: {rows.Count}");
        Console.WriteLine($"CSV: {csvPath}");
        Console.WriteLine();

        var groups = rows
            .GroupBy(x => string.IsNullOrWhiteSpace(x.ContainerId)
                ? "(NO_CONTAINER_ID)"
                : x.ContainerId)
            .OrderByDescending(g => g.Count());

        foreach (var group in groups)
        {
            Console.WriteLine($"Container: {group.Key} / Interfaces: {group.Count()}");

            foreach (var row in group.Take(8))
            {
                Console.WriteLine($"  Name={row.Name}");
                Console.WriteLine($"  Manufacturer={row.Manufacturer} / Model={row.ModelName}");
                Console.WriteLine($"  DeviceInstanceId={row.DeviceInstanceId}");
                Console.WriteLine($"  InterfaceId={row.InterfaceId}");
            }

            if (group.Count() > 8)
            {
                Console.WriteLine($"  ... {group.Count() - 8} more interface(s)");
            }

            Console.WriteLine();
        }

        Console.WriteLine("POC interpretation:");
        Console.WriteLine("- ContainerId is a PHYSICAL-DEVICE GROUPING CANDIDATE, not yet the final app ID.");
        Console.WriteLine("- Run this program before/after reconnect using different --label values.");
        Console.WriteLine("- Compare ContainerId, DeviceInstanceId and InterfaceId across snapshots.");
    }

    private static IdentityRow ToRow(DeviceInformation device)
    {
        return new IdentityRow(
            Name: FirstNonEmpty(
                GetString(device, "System.Devices.FriendlyName"),
                device.Name,
                "(empty)"),
            InterfaceId: device.Id,
            ContainerId: GetValue(device, "System.Devices.ContainerId"),
            DeviceInstanceId: GetValue(device, "System.Devices.DeviceInstanceId"),
            Manufacturer: GetValue(device, "System.Devices.DeviceManufacturer"),
            ModelName: GetValue(device, "System.Devices.ModelName"),
            DiscoveryMethod: GetValue(device, "System.Devices.DiscoveryMethod"),
            Connected: GetValue(device, "System.Devices.Connected"));
    }

    private static string GetString(DeviceInformation d, string key)
    {
        return d.Properties.TryGetValue(key, out var value)
            ? value?.ToString() ?? string.Empty
            : string.Empty;
    }

    private static string GetValue(DeviceInformation d, string key)
        => GetString(d, key);

    private static string FirstNonEmpty(params string[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;

    private static void WriteCsv(string path, IReadOnlyCollection<IdentityRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Name,ContainerId,DeviceInstanceId,InterfaceId,Manufacturer,ModelName,DiscoveryMethod,Connected");

        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(",",
                Csv(r.Name),
                Csv(r.ContainerId),
                Csv(r.DeviceInstanceId),
                Csv(r.InterfaceId),
                Csv(r.Manufacturer),
                Csv(r.ModelName),
                Csv(r.DiscoveryMethod),
                Csv(r.Connected)));
        }

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
    }

    private static string Csv(string? value)
        => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";

    private static string Sanitize(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');

        return value;
    }

    private static string? GetArgument(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return null;
    }

    private sealed record IdentityRow(
        string Name,
        string InterfaceId,
        string ContainerId,
        string DeviceInstanceId,
        string Manufacturer,
        string ModelName,
        string DiscoveryMethod,
        string Connected);
}
