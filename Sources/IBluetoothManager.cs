
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
    Task<int> GetPower();
    bool AsCadence { get; }
    Task<int> GetCadence();
}
