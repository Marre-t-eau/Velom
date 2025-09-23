using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

using Microsoft.Xna.Framework;
using System.Linq;
using System.Threading.Tasks;
using VelomMonoGame.Android.Sources;
using VelomMonoGame.Core;
using VelomMonoGame.Core.Sources.Bluetooth.Interfaces;

namespace VelomMonoGame.Android
{
    /// <summary>
    /// The main activity for the Android application. It initializes the game instance,
    /// sets up the rendering view, and starts the game loop.
    /// </summary>
    /// <remarks>
    /// This class is responsible for managing the Android activity lifecycle and integrating
    /// with the MonoGame framework.
    /// </remarks>
    [Activity(
        Label = "VelomMonoGame",
        MainLauncher = true,
        Icon = "@drawable/icon",
        Theme = "@style/Theme.Splash",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden
    )]
    public class MainActivity : AndroidGameActivity
    {
        private VelomMonoGameGame _game;
        private View _view;
        public static TaskCompletionSource<IBluetoothManager.PermissionStatus> BluetoothPermissionTcs;

        public static Activity CurrentActivity { get; private set; }

        /// <summary>
        /// Called when the activity is first created. Initializes the game instance,
        /// retrieves its rendering view, and sets it as the content view of the activity.
        /// Finally, starts the game loop.
        /// </summary>
        /// <param name="bundle">A Bundle containing the activity's previously saved state, if any.</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            CurrentActivity = this;

            // Request Bluetooth permissions if needed.
            IBluetoothManager bluetoothManager = new AndroidBluetoothManager();
            IBluetoothManager.PermissionStatus status = bluetoothManager.CheckAndRequestBluetoothPermissions().Result;
            if (status != IBluetoothManager.PermissionStatus.Granted)
            {
                // TODO : Handle permission request and user feedback
            }

            // Création du provider et enregistrement comme service
            IFileProvider fileProvider = new AndroidFileProvider(CurrentActivity.Assets);

            _game = new VelomMonoGameGame();

            // Enregistre le service pour accès global
            _game.Services.AddService(typeof(IFileProvider), fileProvider);
            _game.Services.AddService(typeof(IBluetoothManager), bluetoothManager);

            _view = _game.Services.GetService(typeof(View)) as View;

            SetContentView(_view);
            _game.Run();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 1001 && BluetoothPermissionTcs != null)
            {
                if (grantResults.All(r => r == Permission.Granted))
                    BluetoothPermissionTcs.TrySetResult(IBluetoothManager.PermissionStatus.Granted);
                else
                    BluetoothPermissionTcs.TrySetResult(IBluetoothManager.PermissionStatus.Denied);

                BluetoothPermissionTcs = null;
            }
        }
    }
}