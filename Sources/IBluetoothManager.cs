
using System.Collections.ObjectModel;

namespace Velom.Source;

internal interface IBluetoothManager
{
    ObservableCollection<IDeviceManager> DiscoveredDevices { get; }

    bool IsBluetoothEnabled();
    void StartScan();
    void StopScan();
    Task<PermissionStatus> CheckAndRequestBluetoothPermissions();

    bool AsPower { get; }
    event EventHandler<ushort> PowerUpdated;
    bool AsCadence { get; }
    event EventHandler<ushort> CadenceUpdated;
    bool AsHeartRate { get; }
    event EventHandler<ushort> HeartRateUpdated;
}
