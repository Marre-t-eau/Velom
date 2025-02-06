
namespace Velom.Source;

internal interface IBluetoothManager
{
    IEnumerable<IDeviceManager> Devices { get; }

    void StartScan();
    void StopScan();
}
