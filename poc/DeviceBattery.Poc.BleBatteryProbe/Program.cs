using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

internal static class Program
{
    private sealed record Subscription(
        GattCharacteristic Characteristic,
        GattClientCharacteristicConfigurationDescriptorValue CccdValue);

    private static readonly List<GattDeviceService> OpenServices = [];
    private static readonly List<Subscription> Subscriptions = [];

    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Gate 4 POC-B02 — BLE GATT Battery Probe");
        Console.WriteLine(new string('-', 100));
        Console.WriteLine($"Battery Service UUID       = {GattServiceUuids.Battery}");
        Console.WriteLine($"Battery Level UUID         = {GattCharacteristicUuids.BatteryLevel}");
        Console.WriteLine();

        try
        {
            string selector = GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.Battery);
            DeviceInformationCollection serviceInfos = await DeviceInformation.FindAllAsync(selector);

            Console.WriteLine($"Battery Service DeviceInformation count: {serviceInfos.Count}");
            Console.WriteLine();

            if (serviceInfos.Count == 0)
            {
                Console.WriteLine("[RESULT] No BLE Battery Service instance was returned by Windows.");
                Console.WriteLine("         This does not prove that every Bluetooth device lacks a battery.");
                Console.WriteLine("         Record this path as NEED ALTERNATIVE and continue with other providers.");
                return;
            }

            int serviceIndex = 0;

            foreach (DeviceInformation serviceInfo in serviceInfos)
            {
                serviceIndex++;
                await ProbeServiceAsync(serviceIndex, serviceInfo);
            }

            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"Open GATT services : {OpenServices.Count}");
            Console.WriteLine($"Active subscriptions: {Subscriptions.Count}");

            if (Subscriptions.Count > 0)
            {
                Console.WriteLine("Battery Level notifications/indications are active.");
                Console.WriteLine("Leave the program running and observe whether ValueChanged is raised.");
            }
            else
            {
                Console.WriteLine("No Battery Level notification/indication subscription is active.");
                Console.WriteLine("If Read succeeded, polling may still be possible for this BLE device.");
            }

            Console.WriteLine("Press ENTER to stop and release GATT resources.");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"[FATAL] {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            await CleanupAsync();
        }
    }

    private static async Task ProbeServiceAsync(int serviceIndex, DeviceInformation serviceInfo)
    {
        Console.WriteLine(new string('=', 100));
        Console.WriteLine($"[BATTERY SERVICE #{serviceIndex}]");
        Console.WriteLine($"DeviceInformation.Name = {EmptyText(serviceInfo.Name)}");
        Console.WriteLine($"DeviceInformation.Id   = {serviceInfo.Id}");

        try
        {
            GattDeviceService? service = await GattDeviceService.FromIdAsync(serviceInfo.Id);

            if (service is null)
            {
                Console.WriteLine("GattDeviceService.FromIdAsync -> null");
                Console.WriteLine("Result=NEED ALTERNATIVE / ACCESS OR CONNECTION NOT AVAILABLE");
                Console.WriteLine();
                return;
            }

            OpenServices.Add(service);

            Console.WriteLine($"Service.Uuid            = {service.Uuid}");
            Console.WriteLine($"Service.DeviceId        = {service.DeviceId}");
            Console.WriteLine($"Service.AttributeHandle = 0x{service.AttributeHandle:X4}");

            GattCharacteristicsResult characteristicsResult =
                await service.GetCharacteristicsForUuidAsync(
                    GattCharacteristicUuids.BatteryLevel,
                    BluetoothCacheMode.Uncached);

            Console.WriteLine($"GetCharacteristics Status = {characteristicsResult.Status}");
            Console.WriteLine($"Battery Level count        = {characteristicsResult.Characteristics.Count}");

            if (characteristicsResult.Status != GattCommunicationStatus.Success)
            {
                Console.WriteLine("Battery Level characteristic discovery failed.");
                Console.WriteLine();
                return;
            }

            if (characteristicsResult.Characteristics.Count == 0)
            {
                Console.WriteLine("Battery Service exists, but Battery Level (0x2A19) was not returned.");
                Console.WriteLine();
                return;
            }

            int characteristicIndex = 0;

            foreach (GattCharacteristic characteristic in characteristicsResult.Characteristics)
            {
                characteristicIndex++;
                await ProbeCharacteristicAsync(serviceIndex, characteristicIndex, characteristic);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"[ACCESS DENIED] {ex.Message}");
            Console.WriteLine("Windows/device access policy prevented opening this GATT service.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SERVICE ERROR] {ex.GetType().Name}: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task ProbeCharacteristicAsync(
        int serviceIndex,
        int characteristicIndex,
        GattCharacteristic characteristic)
    {
        GattCharacteristicProperties properties = characteristic.CharacteristicProperties;

        Console.WriteLine();
        Console.WriteLine($"  [BATTERY LEVEL #{serviceIndex}.{characteristicIndex}]");
        Console.WriteLine($"  UUID       = {characteristic.Uuid}");
        Console.WriteLine($"  Handle     = 0x{characteristic.AttributeHandle:X4}");
        Console.WriteLine($"  Properties = {properties}");

        if (properties.HasFlag(GattCharacteristicProperties.Read))
        {
            await ReadBatteryLevelAsync(characteristic, "  INITIAL READ");
        }
        else
        {
            Console.WriteLine("  Read is not supported by this characteristic.");
        }

        GattClientCharacteristicConfigurationDescriptorValue cccdValue =
            GetPreferredCccdValue(properties);

        if (cccdValue == GattClientCharacteristicConfigurationDescriptorValue.None)
        {
            Console.WriteLine("  Notify/Indicate is not supported.");
            return;
        }

        characteristic.ValueChanged += OnBatteryLevelValueChanged;

        try
        {
            GattCommunicationStatus subscribeStatus =
                await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

            Console.WriteLine($"  Subscribe({cccdValue}) Status = {subscribeStatus}");

            if (subscribeStatus == GattCommunicationStatus.Success)
            {
                Subscriptions.Add(new Subscription(characteristic, cccdValue));
            }
            else
            {
                characteristic.ValueChanged -= OnBatteryLevelValueChanged;
            }
        }
        catch (Exception ex)
        {
            characteristic.ValueChanged -= OnBatteryLevelValueChanged;
            Console.WriteLine($"  [SUBSCRIBE ERROR] {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static async Task ReadBatteryLevelAsync(GattCharacteristic characteristic, string label)
    {
        try
        {
            GattReadResult readResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);

            Console.WriteLine($"  {label} Status = {readResult.Status}");

            if (readResult.Status != GattCommunicationStatus.Success)
            {
                Console.WriteLine($"  {label} ProtocolError = {NullableByte(readResult.ProtocolError)}");
                return;
            }

            if (TryParseBatteryLevel(readResult.Value, out byte percentage, out int byteLength))
            {
                Console.WriteLine($"  {label} Value  = {percentage}%");
                Console.WriteLine($"  {label} Bytes  = {byteLength}");
            }
            else
            {
                Console.WriteLine($"  {label} Value  = INVALID/EMPTY");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [{label} ERROR] {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void OnBatteryLevelValueChanged(
        GattCharacteristic sender,
        GattValueChangedEventArgs args)
    {
        try
        {
            string timestamp = DateTimeOffset.Now.ToString("HH:mm:ss.fff");

            if (TryParseBatteryLevel(args.CharacteristicValue, out byte percentage, out int byteLength))
            {
                Console.WriteLine();
                Console.WriteLine($"[{timestamp}] [BLE BATTERY VALUE CHANGED]");
                Console.WriteLine($"  UUID  = {sender.Uuid}");
                Console.WriteLine($"  Value = {percentage}%");
                Console.WriteLine($"  Bytes = {byteLength}");
            }
            else
            {
                Console.WriteLine($"[{timestamp}] [BLE BATTERY VALUE CHANGED] INVALID/EMPTY");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VALUE CHANGED ERROR] {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool TryParseBatteryLevel(IBuffer buffer, out byte percentage, out int byteLength)
    {
        percentage = 0;
        byteLength = checked((int)buffer.Length);

        if (buffer.Length < 1)
            return false;

        using DataReader reader = DataReader.FromBuffer(buffer);
        percentage = reader.ReadByte();

        return percentage <= 100;
    }

    private static GattClientCharacteristicConfigurationDescriptorValue GetPreferredCccdValue(
        GattCharacteristicProperties properties)
    {
        if (properties.HasFlag(GattCharacteristicProperties.Notify))
            return GattClientCharacteristicConfigurationDescriptorValue.Notify;

        if (properties.HasFlag(GattCharacteristicProperties.Indicate))
            return GattClientCharacteristicConfigurationDescriptorValue.Indicate;

        return GattClientCharacteristicConfigurationDescriptorValue.None;
    }

    private static async Task CleanupAsync()
    {
        if (Subscriptions.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Cleaning up BLE subscriptions...");
        }

        foreach (Subscription subscription in Subscriptions)
        {
            try
            {
                subscription.Characteristic.ValueChanged -= OnBatteryLevelValueChanged;

                GattCommunicationStatus status =
                    await subscription.Characteristic
                        .WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.None);

                Console.WriteLine($"  CCCD disable -> {status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [CLEANUP WARNING] {ex.GetType().Name}: {ex.Message}");
            }
        }

        Subscriptions.Clear();

        foreach (GattDeviceService service in OpenServices)
        {
            try
            {
                service.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [DISPOSE WARNING] {ex.GetType().Name}: {ex.Message}");
            }
        }

        OpenServices.Clear();
    }

    private static string EmptyText(string? value)
        => string.IsNullOrWhiteSpace(value) ? "(empty)" : value;

    private static string NullableByte(byte? value)
        => value.HasValue ? $"0x{value.Value:X2}" : "N/A";
}
