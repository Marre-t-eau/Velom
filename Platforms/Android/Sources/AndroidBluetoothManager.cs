using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Velom.Source;

namespace Velom.Platforms.Android.Sources;

internal class AndroidBluetoothManager : IBluetoothManager
{
    public IEnumerable<IDeviceManager> Devices { get; } = new List<AndroidDeviceManager>();

    private IBluetoothLE bluetoothLE;
    private IAdapter adapter;

    private Guid ftmsServiceUuid = new Guid("00001826-0000-1000-8000-00805f9b34fb"); // FTMS Service
    private Guid cyclingPowerMeasurementUuid = new Guid("00002A63-0000-1000-8000-00805f9b34fb"); // Cycling Power Measurement

    internal AndroidBluetoothManager()
    {
        bluetoothLE = CrossBluetoothLE.Current;
        adapter = CrossBluetoothLE.Current.Adapter;

        adapter.ScanTimeout = 10000; // 10 seconds
    }

    public async void StartScan()
    {
        if (bluetoothLE == null)
            return;

        await adapter.StartScanningForDevicesAsync(new[] { ftmsServiceUuid });
    }

    public async void StopScan()
    {
        await adapter.StopScanningForDevicesAsync();
    }
}