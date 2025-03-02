using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Collections.ObjectModel;
using Velom.Source;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Velom.Platforms.Android.Sources;

internal class AndroidBluetoothManager : IBluetoothManager
{
    public ObservableCollection<IDeviceManager> DiscoveredDevices { get; } = [];

    private IBluetoothLE bluetoothLE;
    private IAdapter adapter;

    private Guid ftmsServiceUuid = new Guid("00001826-0000-1000-8000-00805f9b34fb"); // FTMS Service

    internal AndroidBluetoothManager()
    {
        bluetoothLE = CrossBluetoothLE.Current;
        adapter = CrossBluetoothLE.Current.Adapter;

        adapter.ScanTimeout = 10000; // 10 seconds
        adapter.DeviceDiscovered += OnDeviceDiscovered;
    }

    private async void OnDeviceDiscovered(object? sender, DeviceEventArgs e)
    {
        AndroidDeviceManager androidDevice = new AndroidDeviceManager(e.Device);
        DiscoveredDevices.Add(androidDevice);
        await adapter.ConnectToDeviceAsync(e.Device);
        await androidDevice.Initialize();
    }

    public async void StartScan()
    {
        if (!IsBluetoothEnabled())
        {
            return;
        }

        await adapter.StartScanningForDevicesAsync([ftmsServiceUuid]);
    }

    public async void StopScan()
    {
        await adapter.StopScanningForDevicesAsync();
    }

    public async Task<PermissionStatus> CheckAndRequestBluetoothPermissions()
    {
        if (DeviceInfo.Platform != DevicePlatform.Android)
            return PermissionStatus.Unknown;

        PermissionStatus status;
        if (DeviceInfo.Version.Major >= 12)
        {
            status = await Permissions.CheckStatusAsync<MyBluetoothPermission>();

            if (status != PermissionStatus.Granted)
            {
                if (Permissions.ShouldShowRationale<MyBluetoothPermission>())
                {
                    await Shell.Current.DisplayAlert("Permissions nécessaires", "L'application a besoin des permissions Bluetooth pour fonctionner.", "OK");
                }
            }

            status = await Permissions.RequestAsync<MyBluetoothPermission>();
        }
        else
        {
            status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {

                if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
                {
                    await Shell.Current.DisplayAlert("Permissions nécessaires", "L'application a besoin des permissions de localisation pour fonctionner.", "OK");
                }
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
            await Shell.Current.DisplayAlert("Permission requise",
                "La permission de localisation est requise pour le scan Bluetooth. Nous ne stockons ni n'utilisons votre localisation.", "OK");

        return status;
    }

    public bool IsBluetoothEnabled()
    {
        return bluetoothLE.IsOn;
    }

    public bool AsPower
    {
        get
        {
            return DiscoveredDevices.Any(d => d.AsPower);
        }
    }

    public bool AsCadence
    {
        get
        {
            return DiscoveredDevices.Any(d => d.AsCadence);
        }
    }

    public Task<int> GetPower()
    {
        if (!AsPower)
            return new Task<int>(() => 0);
        return DiscoveredDevices.First(d => d.AsPower).GetPower();
    }

    public Task<int> GetCadence()
    {
        if (!AsCadence)
            return new Task<int>(() => 0);
        return DiscoveredDevices.First(d => d.AsPower).GetCadence();
    }

    internal class MyBluetoothPermission : BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new List<(string permission, bool isRuntime)>
            {
            ("android.permission.BLUETOOTH_SCAN", true),
            ("android.permission.BLUETOOTH_CONNECT", true)
            }.ToArray();
    }
}