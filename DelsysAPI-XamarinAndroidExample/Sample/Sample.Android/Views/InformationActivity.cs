using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidSample.Core;
using AndroidSample.Views;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;

namespace AndroidSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class InformationActivity : Android.Support.V7.App.AppCompatActivity
    {
        // Defining buttons/labels for UI
        private TextView TitleText;
        private Button ScanButton;
        private Button ArmButton;
        private Button MVCButton;
        private Button NextImageButton;
        private TextView SensorsText;
        private FrameLayout imageFrame;
        private TextView instrucText;
        private CheckBox realtimeCheckBox;

        private BackgroundWorker mWorker;



        // Delsys trigno emg pipeline class
        private Delsys del;
        private MainModel _model;

        public InformationActivity()
        {
            _model = MainModel.Instance;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            mWorker = new BackgroundWorker();

            // View set up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_info);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            // UI set up
            imageFrame = FindViewById<FrameLayout>(Resource.Id.frame_image);

            TitleText = FindViewById<TextView>(Resource.Id.txv_title);

            ScanButton = FindViewById<Button>(Resource.Id.btn_scan);
            ScanButton.Click += (s, e) =>
            {
                FindViewById<CardView>(Resource.Id.view_sensor).Visibility = ViewStates.Gone;
                FindViewById<TextView>(Resource.Id.txv_subtitle).Visibility = ViewStates.Gone;
                TitleText.Text = "Searching for your sensors ...";
                ScanButton.Visibility = ViewStates.Gone;
                realtimeCheckBox.Visibility = ViewStates.Gone;

                mWorker.DoWork += (o, e) =>
                {
                    _model.del = new Delsys();
                    del = _model.del;
                    if (del == null)
                    {
                        _model.del = new Delsys();
                        del = _model.del;

                    }
                    _model.startSession();
                    del.SensorScan().Wait();

                    //Display buttons found event
                    del.ScanFinished += (object sender, Delsys.ScanResultsEventArgs e)
                        =>
                    {
                        RunOnUiThread(() =>
                        {
                            SensorsText = FindViewById<TextView>(Resource.Id.txv_sensors);
                            TitleText.Text = "Sensors found: ";
                            SensorsText.Visibility = ViewStates.Visible;
                            foreach (var sensor in del.sensors)
                            {
                                SensorsText.Text = SensorsText.Text + "\n" + sensor;
                                    //TODO add some sort of selection of the sensors, make sure there are two displayed etc.
                                }

                            ArmButton.Visibility = ViewStates.Visible;
                        });

                    };
                };
                mWorker.RunWorkerAsync();

            };

            

            ArmButton = FindViewById<Button>(Resource.Id.btn_arm);
            ArmButton.Click += (s, e) =>
            {
                del.SensorArm();

                ArmButton.Visibility = ViewStates.Gone;
                showInstructions();
            };

            NextImageButton = FindViewById<Button>(Resource.Id.btn_next);
            NextImageButton.Click += (s, e) =>
            {
                if (counter < imageIds.Length)
                    updateInstruction();
                else
                    StartActivity(typeof(ExerciseSelectionActivity));
            };

            realtimeCheckBox = FindViewById<CheckBox>(Resource.Id.chkb_realtime);
            realtimeCheckBox.Click += (s, e) =>
            {
                if (realtimeCheckBox.Checked)
                    _model.realTimeCollection = true;
                else
                    _model.realTimeCollection = false;
            };
        }

        public void showInstructions()
        {
            
            imageFrame.Visibility = ViewStates.Visible;
            TitleText.Text = "Follow these instructions";
            instrucText = FindViewById<TextView>(Resource.Id.txv_instruction);
            instrucText.Visibility = ViewStates.Visible;
            getImageLocations();
            updateInstruction();
        }

        public string[] imageNames =  {"info_wipe","info_stick","info_peel","info_apply"};
        public string[] imageDescriptions = { "1. Wipe the area with an alcohol wipe and let dry", "2. Add a sticker to each sensor", "3. Peel the backing off the stickers", "4. Apply firmly to the skin" };
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
            TextView description = FindViewById<TextView>(Resource.Id.txv_instruction);
            if (counter == imageIds.Length - 1)
            {
                NextImageButton.Text = "Start exercising";
            }
            if (counter < imageIds.Length)
            {
                image.SetImageResource(imageIds[counter]);
                description.Text = imageDescriptions[counter];
                counter++;
                //todo add imagedescription for each one?
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