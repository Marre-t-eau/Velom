#if ANDROID
using Velom.Platforms.Android.Sources;
#endif
using System.Composition;
using Velom.Sources.Objects;
using Velom.Sources.Pages;

namespace Velom;

public partial class MainPage : ContentPage
{
    [Import]
    private IBluetoothManager BluetoothManager { get; init; }

    public MainPage()
    {
        InitializeComponent();
        App.Container.SatisfyImports(this);
        if (BluetoothManager != null)
        {
            DevicesListView.ItemsSource = BluetoothManager.DiscoveredDevices;
        }
        FTP.Text = UserInfo.GetUserInfo().Result.FTP.ToString();
        DeviceDisplay.KeepScreenOn = true;
    }

    private async void ScanButton_Clicked(object sender, EventArgs e)
    {
        PermissionStatus status = await BluetoothManager.CheckAndRequestBluetoothPermissions();

        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permissions nécessaires", "L'application a besoin des permissions Bluetooth pour fonctionner.", "OK");
            return;
        }

        if (!BluetoothManager.IsBluetoothEnabled())
        {
            await DisplayAlert("Bluetooth désactivé", "Veuillez activer le Bluetooth pour continuer.", "OK");
            return;
        }

        BluetoothManager.PowerUpdated += (sender, power) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PowerLabel.Text = "Power: " + power;
            });
        };
        BluetoothManager.CadenceUpdated += (sender, cadence) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CadenceLabel.Text = "Cadence: " + cadence;
            });
        };
        BluetoothManager.HeartRateUpdated += (sender, heartRate) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                HeartRateLabel.Text = "Heart Rate: " + heartRate;
            });
        };

        BluetoothManager.StartScan();
    }

    private async void SetPowerCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (SetPowerCheckBox.IsChecked)
        {
            if (ushort.TryParse(PowerEntry.Text, out ushort power))
            {
                await BluetoothManager.SetPower(power);
            }
            await BluetoothManager.StartControllingPower();
        }
        else
        {
            await BluetoothManager.StopControllingPower();
        }
    }

    private async void PowerEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SetPowerCheckBox.IsChecked)
        {
            if (ushort.TryParse(PowerEntry.Text, out ushort power))
            {
                await BluetoothManager.SetPower(power);
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

    private async void OnViewWorkoutsClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WorkoutsListPage());
    }

    private async void OnViewHistoryClicked(object sender, EventArgs e)
    {
        var historyPage = new WorkoutHistoryPage();
        var navigationPage = new NavigationPage(historyPage);
        await Navigation.PushModalAsync(navigationPage);
    }

    private async void SetNewFTP_Clicked(object sender, EventArgs e)
    {
        if (ushort.TryParse(this.FTP.Text, out ushort FTP))
        {
            (await UserInfo.GetUserInfo()).FTP = FTP;
        }
    }
}