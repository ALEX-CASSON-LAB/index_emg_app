using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidSample.Core;
using System.Threading.Tasks;

namespace AndroidSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class InformationActivity : Android.Support.V7.App.AppCompatActivity
    {
        // Defining buttons for UI
        public Button StartButton;
        public Button ScanButton;
        public Button ArmButton;
        public Button MVCButton;

        // Delsys trigno emg pipeline class
        private Delsys del;
        private MainModel _model;

        public InformationActivity()
        {
            _model = MainModel.Instance;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // View set up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_info);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            //Button set up
            StartButton = FindViewById<Button>(Resource.Id.btn_start);
            StartButton.Click += (s, e) =>
            {
                if (del == null)
                {
                    _model.del = new Delsys();
                    del = _model.del;
                }
                ScanButton.Visibility =ViewStates.Visible;

                _model.startSession();
            };

            ScanButton = FindViewById<Button>(Resource.Id.btn_scan);
            ScanButton.Click += async (s, e) =>
            {
                await del.SensorScan(); //todo display sensors and select them
                ScanButton.Visibility = ViewStates.Gone;
                ArmButton.Visibility = ViewStates.Visible; //this needs to be called by an event within the delsys completescan thingy
            };

            ArmButton = FindViewById<Button>(Resource.Id.btn_arm);
            ArmButton.Click += (s, e) =>
            {
                del.SensorArm();
                MVCButton.Visibility = ViewStates.Visible;
                //TODO join this with the scan / show sensors and arm
            };

            MVCButton = FindViewById<Button>(Resource.Id.btn_mvc);
            MVCButton.Click += delegate {
                StartActivity(typeof(MVCActivity));
            };
        }

        #region Activity functions
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
                //TODO add settings page here
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
        #endregion

    }
}