using System.Text;
using Windows.Devices.Enumeration;
using Windows.Devices.Power;

internal static class Program
{
    private static readonly List<Battery> OpenBatteries = [];

    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Gate 4 POC-B01~B04 — Battery / Charging Probe");
        Console.WriteLine(new string('-', 90));

        PrintAggregateBattery();

        Console.WriteLine();
        Console.WriteLine("Enumerating individual battery controllers...");

        var selector = Battery.GetDeviceSelector();
        DeviceInformationCollection batteryDevices =
            await DeviceInformation.FindAllAsync(selector);

        Console.WriteLine($"Battery controller DeviceInformation count: {batteryDevices.Count}");
        Console.WriteLine();

        foreach (var device in batteryDevices)
        {
            try
            {
                var battery = await Battery.FromIdAsync(device.Id);

                if (battery is null)
                {
                    Console.WriteLine($"[NULL BATTERY] Name={device.Name} / Id={device.Id}");
                    continue;
                }

                OpenBatteries.Add(battery);
                battery.ReportUpdated += OnReportUpdated;

                Console.WriteLine($"[BATTERY CONTROLLER] Name={device.Name}");
                Console.WriteLine($"DeviceInformation.Id={device.Id}");
                Console.WriteLine($"Battery.DeviceId={battery.DeviceId}");

                PrintReport(battery.GetReport(), "  ");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Name={device.Name}");
                Console.WriteLine($"        {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine();
            }
        }

        Console.WriteLine(new string('-', 90));
        Console.WriteLine("ReportUpdated listeners are active.");
        Console.WriteLine("When you have real hardware, charge/discharge/connect/disconnect it and watch the log.");
        Console.WriteLine("Press ENTER to stop.");
        Console.ReadLine();

        foreach (var battery in OpenBatteries)
            battery.ReportUpdated -= OnReportUpdated;

        OpenBatteries.Clear();
    }

    private static void PrintAggregateBattery()
    {
        try
        {
            Battery aggregate = Battery.AggregateBattery;

            Console.WriteLine("[AGGREGATE BATTERY]");
            Console.WriteLine($"DeviceId={aggregate.DeviceId}");
            PrintReport(aggregate.GetReport(), "  ");
            Console.WriteLine("  NOTE: AggregateBattery is a system aggregate and is not automatically a peripheral device.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AGGREGATE ERROR] {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void OnReportUpdated(Battery sender, object args)
    {
        try
        {
            Console.WriteLine();
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] [REPORT UPDATED] DeviceId={sender.DeviceId}");
            PrintReport(sender.GetReport(), "  ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[REPORT UPDATED ERROR] {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void PrintReport(BatteryReport report, string indent)
    {
        var percentage = CalculatePercentage(report);

        Console.WriteLine($"{indent}Status={report.Status}");
        Console.WriteLine($"{indent}ChargeRateInMilliwatts={NullableText(report.ChargeRateInMilliwatts)}");
        Console.WriteLine($"{indent}DesignCapacityInMilliwattHours={NullableText(report.DesignCapacityInMilliwattHours)}");
        Console.WriteLine($"{indent}FullChargeCapacityInMilliwattHours={NullableText(report.FullChargeCapacityInMilliwattHours)}");
        Console.WriteLine($"{indent}RemainingCapacityInMilliwattHours={NullableText(report.RemainingCapacityInMilliwattHours)}");
        Console.WriteLine($"{indent}CalculatedPercent={percentage}");
        Console.WriteLine($"{indent}ProductChargingStateCandidate={MapChargingState(report)}");
    }

    private static string CalculatePercentage(BatteryReport report)
    {
        var full = report.FullChargeCapacityInMilliwattHours;
        var remaining = report.RemainingCapacityInMilliwattHours;

        if (!full.HasValue || !remaining.HasValue || full.Value <= 0)
            return "N/A";

        var raw = (remaining.Value / (double)full.Value) * 100.0;
        return $"{raw:F2}%";
    }

    private static string MapChargingState(BatteryReport report)
    {
        return report.Status.ToString() switch
        {
            "Charging" => "Charging",
            "Discharging" => "Not Charging",
            "Idle" => "Not Charging",
            "NotPresent" => "Unknown",
            _ => "Unknown"
        };
    }

    private static string NullableText(int? value)
        => value.HasValue ? value.Value.ToString() : "N/A";
}
