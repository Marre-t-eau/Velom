using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace VelomMonoGame.Core.Sources.Bluetooth.Interfaces;

public interface IBluetoothManager
{
    ObservableCollection<IDeviceManager> DiscoveredDevices { get; }

    bool IsBluetoothEnabled();
    void StartScan();
    bool IsScanning { get; }
    void StopScan();
    Task<PermissionStatus> CheckAndRequestBluetoothPermissions();

    bool AsPower { get; }
    event EventHandler<ushort> PowerUpdated;
    bool AsCadence { get; }
    event EventHandler<ushort> CadenceUpdated;
    bool AsHeartRate { get; }
    event EventHandler<ushort> HeartRateUpdated;
    bool CanSetPower { get; }
    Task SetPower(ushort power);
    Task StartControllingPower();
    Task StopControllingPower();

    public enum PermissionStatus
    {
        Unknown,
        Denied,
        Disabled,
        Granted,
        Restricted,
        Limited
    }
}
