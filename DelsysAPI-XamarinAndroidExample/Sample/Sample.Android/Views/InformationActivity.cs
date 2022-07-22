using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidSample.Core;
using System.Collections.Generic;
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
        public Button NextImageButton;
        public TextView SensorsText;
        FrameLayout imageFrame;



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

            // UI set up
            imageFrame = FindViewById<FrameLayout>(Resource.Id.frame_image);

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
                showInstructions();
            };

            MVCButton = FindViewById<Button>(Resource.Id.btn_mvc);
            MVCButton.Click += delegate {
                StartActivity(typeof(MVCActivity));
            };

            NextImageButton = FindViewById<Button>(Resource.Id.btn_next);
            NextImageButton.Click += (s, e) =>
            {
                updateInstruction();
            };

        }

        public void showInstructions()
        {
            
            imageFrame.Visibility = ViewStates.Visible;
            TextView TitleText = FindViewById<TextView>(Resource.Id.txv_title);
            TitleText.Text = "Follow these intructions";
            getImageLocations();
            updateInstruction();
        }

        public string[] imageNames =  {"apply_sensor","wipe"};
        public int[] imageIds;
        public int counter = 0;
        private void getImageLocations()
        {
            imageIds = new int[imageNames.Length];
            for (int i = 0; i < imageNames.Length; i++)
            {
                string imL = imageNames[i];
                var resourceId = (int)typeof(Resource.Drawable).GetField(imL).GetValue(null);
                imageIds[i] = resourceId;
            }
        }

        public void updateInstruction()
        {
            ImageView image = FindViewById<ImageView>(Resource.Id.imageView);
            if (counter < imageIds.Length)
            {
                image.SetImageResource(imageIds[counter]);
                counter++;
                //todo add imagedescription for each one?
            }
            else
            {
                imageFrame.Visibility = ViewStates.Gone;
                MVCButton.Visibility = ViewStates.Visible;
            }

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