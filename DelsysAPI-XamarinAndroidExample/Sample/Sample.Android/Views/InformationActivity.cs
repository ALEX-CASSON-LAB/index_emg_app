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
        // Defining buttons/labels for UI
        public Button ScanButton;
        public Button ArmButton;
        public Button MVCButton;
        public TextView SensorsText;



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

            // Button set up
            ScanButton = FindViewById<Button>(Resource.Id.btn_scan);
            ScanButton.Click += async (s, e) =>
            {

                ScanButton.Text = "Scanning...";
                if (del == null)
                {
                    _model.del = new Delsys();
                    del = _model.del;

                    //Display buttons found event
                    del.ScanFinished += (object sender, Delsys.ScanResultsEventArgs e)
                        => {
                            SensorsText = FindViewById<TextView>(Resource.Id.txv_sensors);
                            SensorsText.Visibility = ViewStates.Visible;
                            foreach (var sensor in del.sensors)
                            {
                                SensorsText.Text = SensorsText.Text + "\n" + sensor;
                                //TODO add some sort of selection of the sensors, make sure there are two displayed etc.
                            }
                        };
                }
                _model.startSession();
                await del.SensorScan();
                ScanButton.Visibility = ViewStates.Gone;

                ArmButton.Visibility = ViewStates.Visible;
            };

            

            ArmButton = FindViewById<Button>(Resource.Id.btn_arm);
            ArmButton.Click += (s, e) =>
            {
                del.SensorArm();

                ArmButton.Visibility = ViewStates.Invisible;
                MVCButton.Visibility = ViewStates.Visible;
                showInstructions();
            };

            MVCButton = FindViewById<Button>(Resource.Id.btn_mvc);
            MVCButton.Click += delegate {
                StartActivity(typeof(MVCActivity));
            };
        }

        public void showInstructions()
        {
            TextView TitleText = FindViewById<TextView>(Resource.Id.txv_title);
            TitleText.Text = "Follow theese intructions";
            TextView WipeText = FindViewById<TextView>(Resource.Id.txv_wipe);
            WipeText.Visibility = ViewStates.Visible;
            TextView StickerText = FindViewById<TextView>(Resource.Id.txv_stickers);
            StickerText.Visibility = ViewStates.Visible;
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