using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace Velom
{
    [Activity(Theme = "@style/MainTheme", MainLauncher = true, Label = "", LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private const int REQUEST_PERMISSION_CODE = 1001;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Force hide action bar
            if (SupportActionBar != null)
            {
                SupportActionBar.Hide();
            }
            
            initPermission();
        }
        private void initPermission()
        {
            List<String> mPermissionList = new List<string>();
            // When the Android version is 12 or greater, apply for new Bluetooth permissions
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                mPermissionList.Add(Manifest.Permission.BluetoothScan);
                mPermissionList.Add(Manifest.Permission.BluetoothAdvertise);
                mPermissionList.Add(Manifest.Permission.BluetoothConnect);
                //Request for location permissions based on your actual needs
                //mPermissionList.add(Manifest.permission.ACCESS_COARSE_LOCATION);
                //mPermissionList.add(Manifest.permission.ACCESS_FINE_LOCATION);
            }
            else
            {
                mPermissionList.Add(Manifest.Permission.AccessCoarseLocation);
                mPermissionList.Add(Manifest.Permission.AccessFineLocation);
            }

            ActivityCompat.RequestPermissions(this, mPermissionList.ToArray(), REQUEST_PERMISSION_CODE);
        }
        // Override OnRequestPermissionsResult in your Activity (MainActivity.cs)
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 1001) // Nearby Devices
            {
                if (grantResults.Length > 0 && grantResults[0] == PermissionChecker.PermissionGranted)
                {
                    // Permission granted, proceed with Bluetooth operations
                    // ...
                }
                else
                {
                    // Permission denied
                    // ...
                }
            }
            else if (requestCode == 1002) // BluetoothScan and BluetoothConnect
            {
                if (grantResults.Length > 0 && grantResults[0] == PermissionChecker.PermissionGranted && grantResults[1] == PermissionChecker.PermissionGranted)
                {
                    // Permissions granted
                }
                else
                {
                    // Permission denied
                }
            }
            else if (requestCode == 1003) // AccessFineLocation
            {
                if (grantResults.Length > 0 && grantResults[0] == PermissionChecker.PermissionGranted)
                {
                    // Permissions granted
                }
                else
                {
                    // Permission denied
                }
            }
        }
    }
}
