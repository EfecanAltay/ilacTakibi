using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;

namespace ilacTakibi.Droid
{
    [Activity(Label = "ilacTakibi", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        MedicineListUpdateServiceConnection serviceConnection;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new App());            
        }

        protected override void OnStart()
        {
            Intent serviceToStart = new Intent(Application.Context, typeof(MedicineListUpdateService));

            if (serviceConnection == null)
            {
                this.serviceConnection = new MedicineListUpdateServiceConnection(this);
            }
            //StartService(serviceToStart);
            BindService(serviceToStart, this.serviceConnection, Bind.AutoCreate);
            base.OnStart();
        }

        internal void UpdateUiForBoundService()
        {
            //..
        }

        internal void UpdateUiForUnboundService()
        {
            //..
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}