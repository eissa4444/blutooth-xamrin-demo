using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Android.Bluetooth;
using System.Linq;
using Newtonsoft.Json;
using AndroidX.Core.App;
using Android.Content.PM;
using Android;
using System.Collections.Generic;
using Java.Interop;
using Android.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Java.Util;
using System.Text;

namespace test
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public List<BluetoothDevice> Devices { get; set; }
        public BluetoothDevice SelectedDevice { get; set; }
        public Button OpenDoorBtn { get; set; }
        public Button CloseDoor { get; set; }
        public BluetoothSocket Socket { get; set; }
        public ListView ListView { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);



            //base.OnCreate(savedInstanceState);
            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);


            //SetContentView(Resource.Layout.activity_main);

            OpenDoorBtn = FindViewById<Button>(Resource.Id.button1);
            OpenDoorBtn.Click += OpenDoorButtonHandler;

            CloseDoor = FindViewById<Button>(Resource.Id.button2);
            CloseDoor.Click += CloseDoorButtonHandler;




            #region handle permissions

            //if (!(CheckPermissionGranted(Manifest.Permission.BluetoothConnect) ))
            //{
            RequestBlutoothPermission();
            //}

            #endregion



            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null)
                throw new Exception("No Bluetooth adapter found.");

            if (!adapter.IsEnabled)
                throw new Exception("Bluetooth adapter is not enabled.");

            Devices = adapter.BondedDevices.ToList();

            //string DevObjs = JsonConvert.SerializeObject(devices);
            //var editText1 = FindViewById<Android.Widget.EditText>(Resource.Id.editText1);

            var adabter = new ArrayAdapter<string>(this, Resource.Layout.list_item, Devices.Select(d => $"{d.Name},{d.Address}").ToArray());

            ListView = FindViewById<Android.Widget.ListView>(Resource.Id.listView1);
            ListView.Adapter = adabter;

            ListView.TextFilterEnabled = true;

            ListView.ItemClick += ListView_ItemClick;


            //ListView.ItemClick += ListView_ItemClick;
        }

        private void CloseDoorButtonHandler(object sender, EventArgs e)
        {
            byte[] openDoorCommand = Encoding.ASCII.GetBytes("2");

            Socket.OutputStream.WriteAsync(openDoorCommand, 0, openDoorCommand.Length).Wait();
            OpenDoorBtn.Visibility = ViewStates.Visible;
            CloseDoor.Enabled = false;
            OpenDoorBtn.Enabled = true;
        }

        private void OpenDoorButtonHandler(object sender, EventArgs e)
        {
            byte[] openDoorCommand = Encoding.ASCII.GetBytes("1");

            Socket.OutputStream.WriteAsync(openDoorCommand, 0, openDoorCommand.Length).Wait();
            CloseDoor.Enabled = true;
            OpenDoorBtn.Enabled = false;

        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var complexName = ((TextView)e.View).Text.Split(',');
            var name = complexName[0];
            var address = complexName[1];

            SelectedDevice = Devices.FirstOrDefault(d => d.Address == address);
            //Toast.MakeText(Application, ((TextView)e.View).Text, ToastLength.Long).Show();

            Socket = SelectedDevice.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
            Socket.ConnectAsync().Wait();
            OpenDoorBtn.Visibility = ViewStates.Visible;

            ListView.Adapter = null;

            CloseDoor.Visibility = ViewStates.Visible;
            OpenDoorBtn.Visibility = ViewStates.Visible;

        }

        [Export]
        public bool CheckPermissionGranted(string Permissions)
        {
            // Check if the permission is already available.
            if (ActivityCompat.CheckSelfPermission(this, Permissions) != Permission.Granted)
            {
                return false;
            }
            else
            {
                return true;
            }


        }

        private void RequestBlutoothPermission()
        {
            ActivityCompat.RequestPermissions(this, new List<string> { Manifest.Permission.BluetoothConnect }.ToArray(), 1);

            //if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.BluetoothConnect))
            //{
            //    // Provide an additional rationale to the user if the permission was not granted
            //    // and the user would benefit from additional context for the use of the permission.
            //    // For example if the user has previously denied the permission.
            //    ActivityCompat.RequestPermissions(this,new List<string> { Manifest.Permission.BluetoothConnect }.ToArray(), 1);
            //    ActivityCompat.RequestPermissions(this,new List<string> { Manifest.Permission.BluetoothAdmin }.ToArray(), 2);
            //    ActivityCompat.RequestPermissions(this,new List<string> { Manifest.Permission.BluetoothPrivileged }.ToArray(), 3);
            //    ActivityCompat.RequestPermissions(this,new List<string> { Manifest.Permission.BluetoothScan }.ToArray(), 4);

            //}
            //else
            //{
            //    // Camera permission has not been granted yet. Request it directly.
            //    ActivityCompat.RequestPermissions(this, new List<string> { Manifest.Permission.BluetoothConnect }.ToArray(), 1);
            //    ActivityCompat.RequestPermissions(this, new List<string> { Manifest.Permission.BluetoothAdmin }.ToArray(), 2);
            //    ActivityCompat.RequestPermissions(this, new List<string> { Manifest.Permission.BluetoothPrivileged }.ToArray(), 3);
            //    ActivityCompat.RequestPermissions(this, new List<string> { Manifest.Permission.BluetoothScan }.ToArray(), 4);
            //}
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
