using System.Text;
using Windows.Devices.Power;
using Windows.Gaming.Input;
using Windows.System.Power;

internal static class Program
{
    private static readonly object ConsoleLock = new();

    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Gate 4 POC-B03 — Windows.Gaming.Input Battery Probe");
        Console.WriteLine(new string('-', 100));
        Console.WriteLine("Purpose: test the public Windows game-controller battery path before using raw/undocumented HID parsing.");
        Console.WriteLine();

        RawGameController.RawGameControllerAdded += OnRawGameControllerAdded;
        RawGameController.RawGameControllerRemoved += OnRawGameControllerRemoved;
        Gamepad.GamepadAdded += OnGamepadAdded;
        Gamepad.GamepadRemoved += OnGamepadRemoved;

        try
        {
            // Microsoft documents that RawGameControllers may be empty briefly during initialization.
            // Touch both collections and wait a short time before the first snapshot.
            _ = RawGameController.RawGameControllers.Count;
            _ = Gamepad.Gamepads.Count;

            Console.WriteLine("Initializing Windows.Gaming.Input device lists...");
            await Task.Delay(2500);

            PrintSnapshot();

            Console.WriteLine();
            Console.WriteLine(new string('-', 100));
            Console.WriteLine("Commands: R = refresh battery snapshot, Q = quit");
            Console.WriteLine("Keep the controller connected while testing. This probe does not modify controller state.");

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                {
                    break;
                }

                if (key.Key == ConsoleKey.R)
                {
                    Console.WriteLine();
                    PrintSnapshot();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"[FATAL] {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            RawGameController.RawGameControllerAdded -= OnRawGameControllerAdded;
            RawGameController.RawGameControllerRemoved -= OnRawGameControllerRemoved;
            Gamepad.GamepadAdded -= OnGamepadAdded;
            Gamepad.GamepadRemoved -= OnGamepadRemoved;
        }
    }

    private static void PrintSnapshot()
    {
        lock (ConsoleLock)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            IReadOnlyList<RawGameController> rawControllers = RawGameController.RawGameControllers;
            IReadOnlyList<Gamepad> gamepads = Gamepad.Gamepads;

            Console.WriteLine();
            Console.WriteLine(new string('=', 100));
            Console.WriteLine($"[{now:HH:mm:ss.fff}] SNAPSHOT");
            Console.WriteLine($"RawGameController count = {rawControllers.Count}");
            Console.WriteLine($"Gamepad count           = {gamepads.Count}");

            if (rawControllers.Count == 0)
            {
                Console.WriteLine("No RawGameController instance is currently visible to Windows.Gaming.Input.");
            }

            for (int i = 0; i < rawControllers.Count; i++)
            {
                PrintRawController(i + 1, rawControllers[i]);
            }

            if (gamepads.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("[GAMEPAD VIEW]");
                Console.WriteLine("The Gamepad list can overlap the RawGameController list; it is printed only as an additional Windows view.");

                for (int i = 0; i < gamepads.Count; i++)
                {
                    PrintGamepad(i + 1, gamepads[i]);
                }
            }
        }
    }

    private static void PrintRawController(int index, RawGameController controller)
    {
        Console.WriteLine();
        Console.WriteLine($"[RAW GAME CONTROLLER #{index}]");
        Console.WriteLine($"DisplayName       = {EmptyText(controller.DisplayName)}");
        Console.WriteLine($"VendorId          = 0x{controller.HardwareVendorId:X4}");
        Console.WriteLine($"ProductId         = 0x{controller.HardwareProductId:X4}");
        Console.WriteLine($"IsWireless        = {controller.IsWireless}");
        Console.WriteLine($"NonRoamableId     = {EmptyText(controller.NonRoamableId)}");
        Console.WriteLine($"ButtonCount       = {controller.ButtonCount}");
        Console.WriteLine($"AxisCount         = {controller.AxisCount}");
        Console.WriteLine($"SwitchCount       = {controller.SwitchCount}");

        try
        {
            BatteryReport? report = controller.TryGetBatteryReport();
            PrintBatteryReport(report, "TryGetBatteryReport");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TryGetBatteryReport ERROR = {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void PrintGamepad(int index, Gamepad gamepad)
    {
        Console.WriteLine();
        Console.WriteLine($"  [GAMEPAD #{index}]");
        Console.WriteLine($"  IsWireless = {gamepad.IsWireless}");

        try
        {
            RawGameController? raw = RawGameController.FromGameController(gamepad);
            if (raw is not null)
            {
                Console.WriteLine($"  Raw.DisplayName = {EmptyText(raw.DisplayName)}");
                Console.WriteLine($"  Raw.VID/PID     = 0x{raw.HardwareVendorId:X4}/0x{raw.HardwareProductId:X4}");
            }

            BatteryReport? report = gamepad.TryGetBatteryReport();
            PrintBatteryReport(report, "  TryGetBatteryReport");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  TryGetBatteryReport ERROR = {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void PrintBatteryReport(BatteryReport? report, string prefix)
    {
        if (report is null)
        {
            Console.WriteLine($"{prefix} = null");
            Console.WriteLine("  RESULT = Battery information is not exposed through Windows.Gaming.Input for this controller instance.");
            return;
        }

        Console.WriteLine($"{prefix} = report returned");
        Console.WriteLine($"  Status                                = {report.Status}");
        Console.WriteLine($"  ChargeRateInMilliwatts                = {NullableInt(report.ChargeRateInMilliwatts)}");
        Console.WriteLine($"  DesignCapacityInMilliwattHours        = {NullableInt(report.DesignCapacityInMilliwattHours)}");
        Console.WriteLine($"  FullChargeCapacityInMilliwattHours    = {NullableInt(report.FullChargeCapacityInMilliwattHours)}");
        Console.WriteLine($"  RemainingCapacityInMilliwattHours     = {NullableInt(report.RemainingCapacityInMilliwattHours)}");

        int? percentage = CalculatePercentage(
            report.RemainingCapacityInMilliwattHours,
            report.FullChargeCapacityInMilliwattHours);

        Console.WriteLine($"  CalculatedPercent                     = {(percentage.HasValue ? $"{percentage.Value}%" : "N/A")}");
        Console.WriteLine($"  ProductChargingStateCandidate         = {MapChargingState(report.Status)}");

        if (!percentage.HasValue)
        {
            Console.WriteLine("  NOTE: A non-null BatteryReport does not guarantee that Windows exposes enough capacity data for an exact percentage.");
        }
    }

    private static int? CalculatePercentage(int? remaining, int? full)
    {
        if (!remaining.HasValue || !full.HasValue || full.Value <= 0)
        {
            return null;
        }

        double percentage = remaining.Value * 100.0 / full.Value;
        return Math.Clamp((int)Math.Round(percentage, MidpointRounding.AwayFromZero), 0, 100);
    }

    private static string MapChargingState(BatteryStatus status) => status switch
    {
        BatteryStatus.Charging => "Charging",
        BatteryStatus.Discharging => "Not Charging",
        BatteryStatus.Idle => "Not Charging",
        BatteryStatus.NotPresent => "Unknown",
        _ => "Unknown"
    };

    private static void OnRawGameControllerAdded(object? sender, RawGameController controller)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine();
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] [RAW CONTROLLER ADDED] {EmptyText(controller.DisplayName)} " +
                              $"VID=0x{controller.HardwareVendorId:X4} PID=0x{controller.HardwareProductId:X4}");
        }
    }

    private static void OnRawGameControllerRemoved(object? sender, RawGameController controller)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine();
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] [RAW CONTROLLER REMOVED] {EmptyText(controller.DisplayName)} " +
                              $"VID=0x{controller.HardwareVendorId:X4} PID=0x{controller.HardwareProductId:X4}");
        }
    }

    private static void OnGamepadAdded(object? sender, Gamepad gamepad)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine();
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] [GAMEPAD ADDED] IsWireless={gamepad.IsWireless}");
        }
    }

    private static void OnGamepadRemoved(object? sender, Gamepad gamepad)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine();
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] [GAMEPAD REMOVED] IsWireless={gamepad.IsWireless}");
        }
    }

    private static string NullableInt(int? value) => value?.ToString() ?? "N/A";

    private static string EmptyText(string? value) => string.IsNullOrWhiteSpace(value) ? "(empty)" : value;
}
