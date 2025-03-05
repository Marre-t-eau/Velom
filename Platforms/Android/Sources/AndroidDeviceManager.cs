using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Velom.Source;
using Plugin.BLE.Abstractions.EventArgs;

namespace Velom.Platforms.Android.Sources;

internal class AndroidDeviceManager : IDeviceManager
{
    private IDevice Device { get; }
    public string Name => Device.Name;

    public bool AsPower { get; private set; }

    public bool AsCadence { get; private set; }

    public bool AsHeartRate { get; private set; }

    internal AndroidDeviceManager(IDevice device)
    {
        Device = device;
    }

    public event EventHandler<ushort>? PowerUpdated;
    public event EventHandler<ushort>? CadenceUpdated;
    public event EventHandler<ushort>? HeartRateUpdated;

    internal async Task Initialize()
    {
        if (Device.State != DeviceState.Connected)
            return;

        foreach (IService service in await Device.GetServicesAsync())
        {
            if (service.Id == BluetoothServices.FitnessMachineServiceUuid)
            {
                InitializeFitnessMachineService(service);
            }
            else if (service.Id == BluetoothServices.HeartRateServiceUuid)
            {
                InitializeHeartRateService(service);
            }
        }
    }

    private async void InitializeFitnessMachineService(IService service)
    {
        ICharacteristic? characteristic = service.GetCharacteristicsAsync().Result
            .FirstOrDefault(c => c.Id == IndoorBikeData.guid);
        if (characteristic == null)
            return;
        characteristic.ValueUpdated += IndoorBikeDataCharacteristic_ValueUpdated;
        if (characteristic.CanUpdate)
        {
            await characteristic.StartUpdatesAsync();
        }
    }

    private async void InitializeHeartRateService(IService service)
    {
        ICharacteristic? characteristic = service.GetCharacteristicsAsync().Result
            .FirstOrDefault(c => c.Id == HeartRateMeasurement.guid);
        if (characteristic == null)
            return;
        characteristic.ValueUpdated += HeartRateMeasurementCharacteristic_ValueUpdated;
        if (characteristic.CanUpdate)
        {
            await characteristic.StartUpdatesAsync();
        }
    }

    private void IndoorBikeDataCharacteristic_ValueUpdated(object? sender, CharacteristicUpdatedEventArgs e)
    {
        byte[] data = e.Characteristic.Value;
        IndoorBikeData ibd = new(data);
        if (ibd.InstantaneousPower.HasValue)
        {
            AsPower = true;
            PowerUpdated?.Invoke(this, ibd.InstantaneousPower.Value);
        }
        if (ibd.InstantaneousCadence.HasValue)
        {
            AsCadence = true;
            CadenceUpdated?.Invoke(this, (ushort)(ibd.InstantaneousCadence.Value * 0.5));
        }
    }

    private void HeartRateMeasurementCharacteristic_ValueUpdated(object? sender, CharacteristicUpdatedEventArgs e)
    {
        byte[] data = e.Characteristic.Value;
        HeartRateMeasurement hrm = new(data);
        if (hrm.HeartRate > 0)
        {
            AsHeartRate = true;
            HeartRateUpdated?.Invoke(this, hrm.HeartRate);
        }
    }
}
