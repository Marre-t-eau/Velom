
namespace Velom.Source;

internal interface IDeviceManager
{
    internal string Name { get; }
    internal bool AsPower { get; }
    internal bool AsCadence { get; }

    internal Task<int> GetPower();

    internal Task<int> GetCadence();
}
