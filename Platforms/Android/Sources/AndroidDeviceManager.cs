using Plugin.BLE.Abstractions.Contracts;
using Velom.Source;

namespace Velom.Platforms.Android.Sources;

internal class AndroidDeviceManager : IDeviceManager
{
    private IDevice Device { get; }
    public string Name => Device.Name;

    internal AndroidDeviceManager(IDevice device)
    {
        Device = device;
    }
}
