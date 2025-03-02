#if ANDROID
using Velom.Platforms.Android.Sources;
#endif
using Velom.Source;

namespace Velom;

public partial class MainPage : ContentPage
{
    private IBluetoothManager bluetoothManager;

    public MainPage()
    {
        InitializeComponent();
#if ANDROID
        bluetoothManager = new AndroidBluetoothManager();
#endif
        DevicesListView.ItemsSource = bluetoothManager?.DiscoveredDevices;
    }

    private async void ScanButton_Clicked(object sender, EventArgs e)
    {
        PermissionStatus status = await bluetoothManager.CheckAndRequestBluetoothPermissions();

        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permissions nécessaires", "L'application a besoin des permissions Bluetooth pour fonctionner.", "OK");
            return;
        }

        if (!bluetoothManager.IsBluetoothEnabled())
        {
            await DisplayAlert("Bluetooth désactivé", "Veuillez activer le Bluetooth pour continuer.", "OK");
            return;
        }

        bluetoothManager.StartScan();

        await Task.Delay(5000);
        while (true)
        {
            if (bluetoothManager.AsPower)
            {
                PowerLabel.Text = "Power: " + await bluetoothManager.GetPower();
            }
            if (bluetoothManager.AsCadence)
            {
                CadenceLabel.Text = "Cadence: " + await bluetoothManager.GetCadence();
            }
            await Task.Delay(1000);
        }
    }
}