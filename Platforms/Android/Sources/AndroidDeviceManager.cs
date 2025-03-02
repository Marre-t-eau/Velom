using AndroidX.Lifecycle;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.Data.Common;
using System.Diagnostics.Metrics;
using Velom.Source;

namespace Velom.Platforms.Android.Sources;

internal class AndroidDeviceManager : IDeviceManager
{
    #region Services and Characteristics UUIDs
    // Services
    private readonly Guid fitnessMachineServiceUuid = new Guid("00001826-0000-1000-8000-00805f9b34fb"); // Fitness Machine Service

    // Characteristics
    private readonly Guid indoorBikeDataCharacteristicUuid = new Guid("00002AD2-0000-1000-8000-00805f9b34fb"); // Indoor Bike Data Characteristic
    #endregion

    private IDevice Device { get; }
    public string Name => Device.Name;

    public bool AsPower { get; private set; }
    private int Power { get; set; } = 0;

    public bool AsCadence { get; private set; }
    private int Cadence { get; set; } = 0;

    internal AndroidDeviceManager(IDevice device)
    {
        Device = device;
    }

    internal async Task Initialize()
    {
        if (Device.State != DeviceState.Connected)
            return;

        foreach (IService service in await Device.GetServicesAsync())
        {
            if (service.Id == fitnessMachineServiceUuid)
            {
                InitializeFitnessMachineService(service);
            }
        }
    }

    private async void InitializeFitnessMachineService(IService service)
    {
        ICharacteristic? characteristic = service.GetCharacteristicsAsync().Result
            .FirstOrDefault(c => c.Id == indoorBikeDataCharacteristicUuid);
        if (characteristic == null)
            return;
        characteristic.ValueUpdated += IndoorBikeDataCharacteristic_ValueUpdated;
        if (characteristic.CanUpdate)
        {
            await characteristic.StartUpdatesAsync();
        }
    }

    public async Task<int> GetPower()
    {
        if (!AsPower)
            return -1;
        return Power;
    }

    private void IndoorBikeDataCharacteristic_ValueUpdated(object? sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
    {
        byte[] data = e.Characteristic.Value;
        IndoorBikeData ibd = new(data);
        if (ibd.InstantaneousPower.HasValue)
        {
            Power = ibd.InstantaneousPower.Value;
            AsPower = true;
        }
        if (ibd.InstantaneousCadence.HasValue)
        {
            Cadence = (ushort)(ibd.InstantaneousCadence.Value * 0.5);
            AsCadence = true;
        }
    }

    public async Task<int> GetCadence()
    {
        if (!AsCadence)
            return -1;
        return Cadence;
    }
}
