namespace Velom.Sources.Objects;

public interface IDeviceManager
{
    string Id { get; }
    string Name { get; }
    bool AsPower { get; }
    bool AsCadence { get; }
    bool AsHeartRate { get; }
    bool CanSetPower { get; }

    event EventHandler<ushort> PowerUpdated;
    event EventHandler<ushort> CadenceUpdated;
    event EventHandler<ushort> HeartRateUpdated;

    Task SetPower(ushort power);
    Task StartControllingPower();
    Task StopControllingPower();
}
