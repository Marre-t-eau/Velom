
namespace Velom.Source;

internal interface IDeviceManager
{
    string Name { get; }
    bool AsPower { get; }
    bool AsCadence { get; }
    bool AsHeartRate { get; }

    event EventHandler<ushort> PowerUpdated;
    event EventHandler<ushort> CadenceUpdated;
    event EventHandler<ushort> HeartRateUpdated;
}
