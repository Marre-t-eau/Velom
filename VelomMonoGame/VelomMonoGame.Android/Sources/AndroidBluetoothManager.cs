using Android.OS;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VelomMonoGame.Core.Sources.Bluetooth;
using VelomMonoGame.Core.Sources.Bluetooth.Interfaces;
using Android;
using Android.App;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace VelomMonoGame.Android.Sources;

internal class AndroidBluetoothManager : IBluetoothManager
{
    public ObservableCollection<IDeviceManager> DiscoveredDevices { get; } = [];

    private IBluetoothLE bluetoothLE;
    private IAdapter adapter;

    private List<EventHandler<ushort>> powerUpdatedHandlers = new List<EventHandler<ushort>>();
    private List<EventHandler<ushort>> cadenceUpdatedHandlers = new List<EventHandler<ushort>>();
    private List<EventHandler<ushort>> heartRateUpdatedHandlers = new List<EventHandler<ushort>>();

    public AndroidBluetoothManager()
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
        foreach (EventHandler<ushort> handler in powerUpdatedHandlers)
        {
            androidDevice.PowerUpdated += handler;
        }
        foreach (EventHandler<ushort> handler in cadenceUpdatedHandlers)
        {
            androidDevice.CadenceUpdated += handler;
        }
        foreach (EventHandler<ushort> handler in heartRateUpdatedHandlers)
        {
            androidDevice.HeartRateUpdated += handler;
        }
    }

    public async void StartScan()
    {
        if (!IsBluetoothEnabled() && !IsScanning)
        {
            return;
        }

        await adapter.StartScanningForDevicesAsync([BluetoothServices.FitnessMachineServiceUuid, BluetoothServices.HeartRateServiceUuid]);
    }

    public bool IsScanning
    {
        get
        {
            return adapter.IsScanning;
        }
    }

    public async void StopScan()
    {
        await adapter.StopScanningForDevicesAsync();
    }

    public async Task<IBluetoothManager.PermissionStatus> CheckAndRequestBluetoothPermissions()
    {
        Activity activity = MainActivity.CurrentActivity ?? Application.Context as Activity;
        if (activity == null)
            return IBluetoothManager.PermissionStatus.Unknown;

        // Permissions requises selon la version Android
        string[] permissions;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            permissions = new[]
            {
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothConnect,
                    Manifest.Permission.BluetoothAdvertise,
                    Manifest.Permission.AccessFineLocation // parfois nécessaire pour le scan
                };
        }
        else
        {
            permissions = new[]
            {
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothAdmin,
                    Manifest.Permission.AccessFineLocation
                };
        }

        // Vérifier si toutes les permissions sont déjà accordées
        bool allGranted = true;
        foreach (var perm in permissions)
        {
            if (ContextCompat.CheckSelfPermission(activity, perm) != Permission.Granted)
            {
                allGranted = false;
                break;
            }
        }

        if (allGranted)
            return IBluetoothManager.PermissionStatus.Granted;

        // Demander les permissions manquantes
        TaskCompletionSource<IBluetoothManager.PermissionStatus> tcs = new();
        ActivityCompat.RequestPermissions(activity, permissions, 1001);

        // Gérer la réponse dans OnRequestPermissionsResult de l'activité principale
        // Ici, on suppose que vous stockez le tcs dans une propriété statique pour le récupérer dans MainActivity
        MainActivity.BluetoothPermissionTcs = tcs;

        return await tcs.Task;
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

    public bool AsHeartRate
    {
        get
        {
            return DiscoveredDevices.Any(d => d.AsHeartRate);
        }
    }

    public event EventHandler<ushort> PowerUpdated
    {
        add
        {
            powerUpdatedHandlers.Add(value);
            foreach (IDeviceManager device in DiscoveredDevices)
            {
                device.PowerUpdated += value;
            }
        }
        remove
        {
            powerUpdatedHandlers.Remove(value);
            foreach (IDeviceManager device in DiscoveredDevices)
            {
                device.PowerUpdated -= value;
            }
        }
    }

    public event EventHandler<ushort> CadenceUpdated
    {
        add
        {
            cadenceUpdatedHandlers.Add(value);
            foreach (IDeviceManager device in DiscoveredDevices)
            {
                device.CadenceUpdated += value;
            }
        }
        remove
        {
            cadenceUpdatedHandlers.Remove(value);
            foreach (IDeviceManager device in DiscoveredDevices)
            {
                device.CadenceUpdated -= value;
            }
        }
    }

    public event EventHandler<ushort> HeartRateUpdated
    {
        add
        {
            heartRateUpdatedHandlers.Add(value);
            foreach (IDeviceManager device in DiscoveredDevices)
            {
                device.HeartRateUpdated += value;
            }
        }
        remove
        {
            heartRateUpdatedHandlers.Remove(value);
            foreach (IDeviceManager device in DiscoveredDevices)
            {
                device.HeartRateUpdated -= value;
            }
        }
    }

    public bool CanSetPower
    {
        get
        {
            return DiscoveredDevices.Any(d => d.CanSetPower);
        }
    }

    public async Task SetPower(ushort power)
    {
        foreach (IDeviceManager device in DiscoveredDevices)
        {
            if (device.CanSetPower)
                await device.SetPower(power);
        }
    }

    public async Task StartControllingPower()
    {
        foreach (IDeviceManager device in DiscoveredDevices)
        {
            if (device.CanSetPower)
                await device.StartControllingPower();
        }
    }

    public async Task StopControllingPower()
    {
        foreach (IDeviceManager device in DiscoveredDevices)
        {
            if (device.CanSetPower)
                await device.StopControllingPower();
        }
    }
}