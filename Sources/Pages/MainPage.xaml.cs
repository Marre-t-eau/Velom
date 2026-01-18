using System.Composition;
using Velom.Sources.Objects;
using Velom.Sources.Pages;

namespace Velom;

public partial class MainPage : ContentPage
{
    [Import]
    private IBluetoothManager BluetoothManager { get; init; }

    private bool _isScanning = false;

    public MainPage()
    {
        InitializeComponent();
        App.Container.SatisfyImports(this);
        
        if (BluetoothManager != null)
        {
            DevicesListView.ItemsSource = BluetoothManager.DiscoveredDevices;
            SetupBluetoothEventHandlers();
        }
        
        LoadUserProfile();
        DeviceDisplay.KeepScreenOn = true;
    }

    private async void LoadUserProfile()
    {
        try
        {
            var userInfo = await UserInfo.GetUserInfo();
            FTP.Text = userInfo.FTP.ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load user profile: {ex.Message}", "OK");
        }
    }

    private void SetupBluetoothEventHandlers()
    {
        BluetoothManager.PowerUpdated += (sender, power) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PowerSensorIndicator.Fill = new SolidColorBrush(Colors.Green);
                CheckAllSensorsConnected();
            });
        };
        
        BluetoothManager.CadenceUpdated += (sender, cadence) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CadenceSensorIndicator.Fill = new SolidColorBrush(Colors.Green);
                CheckAllSensorsConnected();
            });
        };
        
        BluetoothManager.HeartRateUpdated += (sender, heartRate) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                HeartRateSensorIndicator.Fill = new SolidColorBrush(Colors.Green);
                CheckAllSensorsConnected();
            });
        };
    }

    private void CheckAllSensorsConnected()
    {
        if (BluetoothManager.AsPower && BluetoothManager.AsCadence && BluetoothManager.AsHeartRate)
        {
            StopScanning();
        }
    }

    private void StopScanning()
    {
        _isScanning = false;
        ScanButton.IsEnabled = true;
        ScanButton.Text = "🔍 Scan devices";
    }

    private async void ScanButton_Clicked(object sender, EventArgs e)
    {
        if (_isScanning)
        {
            return;
        }

        PermissionStatus status = await BluetoothManager.CheckAndRequestBluetoothPermissions();

        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permissions Required", "Bluetooth permissions are required to scan for devices.", "OK");
            return;
        }

        if (!BluetoothManager.IsBluetoothEnabled())
        {
            await DisplayAlert("Bluetooth Disabled", "Please enable Bluetooth to continue.", "OK");
            return;
        }

        _isScanning = true;
        ScanButton.IsEnabled = false;
        ScanButton.Text = "🔍 Scanning...";

        BluetoothManager.StartScan();

        // Safety timeout after 10 seconds
        await Task.Delay(10000);
        
        if (_isScanning)
        {
            StopScanning();
        }
    }

    private async void OnManualControlClicked(object sender, EventArgs e)
    {
        var controlPage = new ControlPage();
        var navigationPage = new NavigationPage(controlPage);
        await Navigation.PushModalAsync(navigationPage);
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
        if (!string.IsNullOrWhiteSpace(FTP.Text) && ushort.TryParse(FTP.Text, out ushort ftpValue))
        {
            try
            {
                var userInfo = await UserInfo.GetUserInfo();
                userInfo.FTP = ftpValue;
                await DisplayAlert("Success", $"FTP updated to {ftpValue}W", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save FTP: {ex.Message}", "OK");
            }
        }
        else
        {
            await DisplayAlert("Invalid Input", "Please enter a valid numeric FTP value.", "OK");
        }
    }
}