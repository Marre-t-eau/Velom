using System.Composition.Hosting;
using System.Reflection;
using Velom.Sources.Services;

namespace Velom;

public partial class App : Application
{
    public static CompositionHost Container { get; private set; }

    public App()
    {
        InitializeComponent();
        InitializeMefContainer();
        LocalizationService.Initialize();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    private static void InitializeMefContainer()
    {
        ContainerConfiguration configuration = new ContainerConfiguration().WithAssembly(Assembly.GetExecutingAssembly());
#if ANDROID
        //configuration.WithPart(typeof(Velom.Platforms.Android.Sources.AndroidBluetoothManager));
#endif
        Container = configuration.CreateContainer();
    }
}
