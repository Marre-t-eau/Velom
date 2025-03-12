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

        bluetoothManager.PowerUpdated += (sender, power) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PowerLabel.Text = "Power: " + power;
            });
        };
        bluetoothManager.CadenceUpdated += (sender, cadence) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CadenceLabel.Text = "Cadence: " + cadence;
            });
        };
        bluetoothManager.HeartRateUpdated += (sender, heartRate) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                HeartRateLabel.Text = "Heart Rate: " + heartRate;
            });
        };

        bluetoothManager.StartScan();
    }

    private async void SetPowerCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (SetPowerCheckBox.IsChecked)
        {
            if (ushort.TryParse(PowerEntry.Text, out ushort power))
            {
                await bluetoothManager.SetPower(power);
            }
            await bluetoothManager.StartControllingPower();
        }
        else
        {
            await bluetoothManager.StopControllingPower();
        }
    }

    private async void PowerEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SetPowerCheckBox.IsChecked)
        {
            if (ushort.TryParse(PowerEntry.Text, out ushort power))
            {
                await bluetoothManager.SetPower(power);
            }
        }
    }

    private void AddPower_Clicked(object sender, EventArgs e)
    {
        if (ushort.TryParse(PowerEntry.Text, out ushort power))
        {
            PowerEntry.Text = (power + 5).ToString();
        }
    }

    private void SubtractPower_Clicked(object sender, EventArgs e)
    {
        if (ushort.TryParse(PowerEntry.Text, out ushort power))
        {
            PowerEntry.Text = (power - 5).ToString();
        }
    }
}