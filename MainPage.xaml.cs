using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Collections.ObjectModel;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Velom;

public partial class MainPage : ContentPage
{
    private IBluetoothLE ble;
    private IAdapter adapter;
    private ObservableCollection<IDevice> deviceList;
    private IDevice connectedDevice;
    private ICharacteristic powerCharacteristic;
    private Guid ftmsServiceUuid = new Guid("00001826-0000-1000-8000-00805f9b34fb"); // FTMS Service
    private Guid cyclingPowerMeasurementUuid = new Guid("00002A63-0000-1000-8000-00805f9b34fb"); // Cycling Power Measurement
    private Guid fitnessMachineStatusUuid = new Guid("00002AD3-0000-1000-8000-00805f9b34fb"); // Fitness Machine Status
    private Guid fitnessMachineFeatureUuid = new Guid("00002ACC-0000-1000-8000-00805f9b34fb"); // Fitness Machine Feature

    public MainPage()
    {
        InitializeComponent();
        ble = CrossBluetoothLE.Current;
        adapter = CrossBluetoothLE.Current.Adapter;
        deviceList = new ObservableCollection<IDevice>();
        DevicesListView.ItemsSource = deviceList;

        adapter.ScanTimeout = 20000; // 20 seconds
        adapter.DeviceDiscovered += (s, a) =>
        {
            if (!deviceList.Contains(a.Device))
            {
                MainThread.BeginInvokeOnMainThread(() => deviceList.Add(a.Device));
            }
        };
    }

    private async void ScanButton_Clicked(object sender, EventArgs e)
    {
        var status = await CheckAndRequestBluetoothPermissions();

        if (!status)
            return;

        if (ble.State != BluetoothState.On)
        {
            await DisplayAlert("Bluetooth est désactivé", "Veuillez activer le Bluetooth", "OK");
            return;
        }

        deviceList.Clear();
        await adapter.StartScanningForDevicesAsync(new Guid[] { ftmsServiceUuid });
    }

    private async Task<bool> CheckAndRequestBluetoothPermissions()
    {
        if (DeviceInfo.Platform != DevicePlatform.Android)
            return false;

        var status = PermissionStatus.Unknown;

        if (DeviceInfo.Version.Major >= 12)
        {
            status = await Permissions.CheckStatusAsync<MyBluetoothPermission>();

            if (status == PermissionStatus.Granted)
                return true;

            if (Permissions.ShouldShowRationale<MyBluetoothPermission>())
            {
                await Shell.Current.DisplayAlert("Permissions nécessaires", "L'application a besoin des permissions Bluetooth pour fonctionner.", "OK");
            }

            status = await Permissions.RequestAsync<MyBluetoothPermission>();
        }
        else
        {
            status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return true;

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                await Shell.Current.DisplayAlert("Permissions nécessaires", "L'application a besoin des permissions de localisation pour fonctionner.", "OK");
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
            await Shell.Current.DisplayAlert("Permission requise",
                "La permission de localisation est requise pour le scan Bluetooth. Nous ne stockons ni n'utilisons votre localisation.", "OK");

        return status == PermissionStatus.Granted;
    }

    private async void DevicesListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is IDevice device)
        {
            try
            {
                await adapter.StopScanningForDevicesAsync(); // Stop scanning

                await adapter.ConnectToDeviceAsync(device);
                connectedDevice = device;
                await DisplayAlert("Connected", $"Connected to {device.Name}", "OK");

                // Discover Services and Characteristics
                var service = await connectedDevice.GetServiceAsync(ftmsServiceUuid);
                if (service != null)
                {
                    powerCharacteristic = await service.GetCharacteristicAsync(fitnessMachineFeatureUuid);
                    foreach (var characteristic in await service.GetCharacteristicsAsync())
                    {
                        if (characteristic.CanRead)
                        {
                            (byte[] data, int resultCode) = await characteristic.ReadAsync();

                            if (data != null && data.Length >= 2)
                            {
                                int power = BitConverter.ToUInt16(data, 0); // Assuming little-endian
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    PowerLabel.Text = $"Power: {power} W";
                                });
                            }
                        }
                    }
                    if (powerCharacteristic != null && powerCharacteristic.CanRead)
                    {
                        byte[] data = powerCharacteristic.Value;

                        if (data != null && data.Length >= 2)
                        {
                            int power = BitConverter.ToUInt16(data, 0); // Assuming little-endian
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                PowerLabel.Text = $"Power: {power} W";
                            });
                        }
                    }
                    if (powerCharacteristic != null && powerCharacteristic.CanUpdate)
                    {
                        powerCharacteristic.ValueUpdated += PowerCharacteristic_ValueUpdated;
                        await powerCharacteristic.StartUpdatesAsync();
                    }
                    if (powerCharacteristic == null)
                    {
                        await DisplayAlert("Error", "Fitness Machine Status characteristic not found.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "FTMS service not found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Connection error: {ex.Message}", "OK");
            }
        }
    }

    private void PowerCharacteristic_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
    {
        byte[] data = e.Characteristic.Value;

        if (data != null && data.Length >= 2)
        {
            int power = BitConverter.ToUInt16(data, 0); // Assuming little-endian
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PowerLabel.Text = $"Power: {power} W";
            });
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (connectedDevice != null)
        {
            if (powerCharacteristic != null)
            {
                powerCharacteristic.StopUpdatesAsync();
            }
            adapter.DisconnectDeviceAsync(connectedDevice);
        }
    }

    internal class MyBluetoothPermission : BasePlatformPermission
    {
#if ANDROID
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new List<(string permission, bool isRuntime)>
            {
            ("android.permission.BLUETOOTH_SCAN", true),
            ("android.permission.BLUETOOTH_CONNECT", true)
            }.ToArray();
#endif
    }
}