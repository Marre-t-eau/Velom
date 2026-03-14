using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Velom.Sources.Objects;

namespace Velom.Platforms.Android.Sources;

internal class AndroidDeviceManager : IDeviceManager
{
    private IDevice Device { get; }
    public string Id => Device.Id.ToString();
    public string Name => Device.Name;

    public bool AsPower { get; private set; }

    public bool AsCadence { get; private set; }

    public bool AsHeartRate { get; private set; }

    public bool CanSetPower { get; private set; }

    internal AndroidDeviceManager(IDevice device)
    {
        Device = device;
    }

    public event EventHandler<ushort>? PowerUpdated;
    public event EventHandler<ushort>? CadenceUpdated;
    public event EventHandler<ushort>? HeartRateUpdated;

    private ICharacteristic? fitnessMachineControlPoint = null;

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
        IReadOnlyList<ICharacteristic> characteristics = await service.GetCharacteristicsAsync();

        ICharacteristic? characteristic = characteristics.FirstOrDefault(c => c.Id == IndoorBikeData.guid);

        if (characteristic != null)
        {
            characteristic.ValueUpdated += IndoorBikeDataCharacteristic_ValueUpdated;
            if (characteristic.CanUpdate)
            {
                await characteristic.StartUpdatesAsync();
            }
        }

        characteristic = characteristics.FirstOrDefault(c => c.Id == FitnessMachineControlPoint.guid);
        if (characteristic != null)
        {
            fitnessMachineControlPoint = characteristic;
            try
            {
                FitnessMachineControlPoint fmc = FitnessMachineControlPoint.CreateRequestControlCommand();
                await fitnessMachineControlPoint.WriteAsync(fmc.ToByteArray());
                fmc = FitnessMachineControlPoint.CreateStopAndResetCommand();
                await fitnessMachineControlPoint.WriteAsync(fmc.ToByteArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize control point: {ex.Message}");
                // Continue even if initialization fails
            }
        }

        characteristic = characteristics.FirstOrDefault(c => c.Id == FitnessMachineFeature.guid);
        if (characteristic != null)
        {
            (byte[], int) result = await characteristic.ReadAsync();
            if (result.Item2 == 0)
            {
                FitnessMachineFeature fmf = new(result.Item1);
                if (fmf.HasPowerTargetSettingSupported)
                {
                    CanSetPower = true;
                }
            }
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

    public async Task SetPower(ushort power)
    {
        if (fitnessMachineControlPoint == null)
            return;

        if (Device.State != DeviceState.Connected)
        {
            System.Diagnostics.Debug.WriteLine("Device not connected, cannot set power");
            return;
        }

        try
        {
            FitnessMachineControlPoint fmc = FitnessMachineControlPoint.CreateSetTargetPowerCommand(power);
            await fitnessMachineControlPoint.WriteAsync(fmc.ToByteArray());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set power: {ex.Message}");
            // Silently fail - the device might not be ready
        }
    }

    public async Task StartControllingPower()
    {
        if (fitnessMachineControlPoint == null)
            return;

        if (Device.State != DeviceState.Connected)
        {
            System.Diagnostics.Debug.WriteLine("Device not connected, cannot start controlling power");
            return;
        }

        try
        {
            FitnessMachineControlPoint fmc = FitnessMachineControlPoint.CreateStartOrResumeCommand();
            await fitnessMachineControlPoint.WriteAsync(fmc.ToByteArray());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to start controlling power: {ex.Message}");
            // Silently fail - the device might not be ready
        }
    }

    public async Task StopControllingPower()
    {
        if (fitnessMachineControlPoint == null)
            return;

        if (Device.State != DeviceState.Connected)
        {
            System.Diagnostics.Debug.WriteLine("Device not connected, cannot stop controlling power");
            return;
        }

        try
        {
            FitnessMachineControlPoint fmc = FitnessMachineControlPoint.CreateStopOrPauseCommand();
            await fitnessMachineControlPoint.WriteAsync(fmc.ToByteArray());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to stop controlling power: {ex.Message}");
            // Silently fail - the device might not be ready
        }
    }
}
