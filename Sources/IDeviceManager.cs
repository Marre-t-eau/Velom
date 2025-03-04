
namespace Velom.Source;

internal interface IDeviceManager
{
    string Name { get; }
    bool AsPower { get; }
    bool AsCadence { get; }

    event EventHandler<ushort> PowerUpdated;
    event EventHandler<ushort> CadenceUpdated;
}
