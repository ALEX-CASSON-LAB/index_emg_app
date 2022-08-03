﻿using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidSample.Core;
using AndroidSample.Views;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using Java.Lang;
using System.Reflection;
using Android.Media;

namespace AndroidSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class InformationActivity : Android.Support.V7.App.AppCompatActivity
    {
        // Defining buttons/labels for UI
        private TextView TitleText;
        private Button ScanButton;
        private Button ArmButton;
        private Button RescanButton;

        private Button ExerciseButton;
        private Button MVCButton;

        private Button NextImageButton;
        private TextView SensorsText;
        private FrameLayout imageFrame;
        private TextView instrucText;
        private CheckBox realtimeCheckBox;

        private ProgressBar searchProgBar;

        private BackgroundWorker startWorker;
        private BackgroundWorker scanWorker;

        private VideoView SetupVideo;
        private VideoView ApplyVideo;

        // Delsys trigno emg pipeline class
        private Delsys del;
        private MainModel _model;

        string setup_path;
        string apply_path;
        public InformationActivity()
        {
            _model = MainModel.Instance;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            startWorker = new BackgroundWorker();
            scanWorker = new BackgroundWorker();

            // View set up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_info);
            //Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);

            // UI set up
            imageFrame = FindViewById<FrameLayout>(Resource.Id.frame_image);
            TitleText = FindViewById<TextView>(Resource.Id.txv_title);
            MVCButton = FindViewById<Button>(Resource.Id.btn_mvc);
            ExerciseButton = FindViewById<Button>(Resource.Id.btn_exercise);
            RescanButton = FindViewById<Button>(Resource.Id.btn_rescan);
            ArmButton = FindViewById<Button>(Resource.Id.btn_arm);
            searchProgBar = FindViewById<ProgressBar>(Resource.Id.progBar_search);
            ScanButton = FindViewById<Button>(Resource.Id.btn_scan);

            // Video setup
            SetupVideo = FindViewById<VideoView>(Resource.Id.video_sensor);
            ApplyVideo = FindViewById<VideoView>(Resource.Id.video_apply);

            MediaController mController = new Android.Widget.MediaController(this);

            mController.SetAnchorView(SetupVideo);

            setup_path = string.Format("android.resource://{0}/{1}", PackageName, Resource.Raw.avideo);
            apply_path = string.Format("android.resource://{0}/{1}", PackageName, Resource.Raw.avideo);
            
            SetupVideo.SetVideoPath(setup_path); // Path of your saved video file.
            SetupVideo.SetMediaController(mController);
            SetupVideo.Start();
            SetupVideo.SetOnPreparedListener(new VideoLoop());


            ScanButton.Click += (s, e) =>
            {
                FindViewById<CardView>(Resource.Id.card_sensor).Visibility = ViewStates.Gone;
                FindViewById<TextView>(Resource.Id.txv_subtitle).Visibility = ViewStates.Gone;
                TitleText.Text = Resources.GetString(Resource.String.scan_txt);
                ScanButton.Visibility = ViewStates.Gone;
                realtimeCheckBox.Visibility = ViewStates.Gone;
                searchProgBar.Visibility = ViewStates.Visible; //TODO make the bar actually progress

                startWorker.DoWork += (o, e) =>
                {
                    _model.del = new Delsys();
                    del = _model.del;
                   
                    
                    _model.startSession();

                    // Display sensors found
                    del.ScanFinished += (object sender, Delsys.ScanResultsEventArgs e)
                        =>
                    {
                        if (del.sensors.Count == 0)
                        {
                            displayRescanUI();
                        }
                        else
                        {
                            RunOnUiThread(() =>
                            {
                                SensorsText = FindViewById<TextView>(Resource.Id.txv_sensors);
                                TitleText.Text = "Sensors found: ";
                                SensorsText.Visibility = ViewStates.Visible;
                                searchProgBar.Visibility = ViewStates.Gone;

                                foreach (var sensor in del.sensors)
                                {
                                    SensorsText.Text = SensorsText.Text + "\n" + sensor;
                                }

                                ArmButton.Visibility = ViewStates.Visible;
                            });
                        }
                    };

                    del.SensorScan().Wait();
                };
                
                startWorker.RunWorkerAsync(); 
            };

            RescanButton.Click += (s, e) =>
            {
                TitleText.Text = Resources.GetString(Resource.String.scan_txt);
                RescanButton.Visibility = ViewStates.Gone;
                searchProgBar.Visibility = ViewStates.Visible;
               
                scanWorker.DoWork += (s, o) =>
                {
                    del.SensorScan().Wait();
                };
                scanWorker.RunWorkerAsync();
            };
                        

            ArmButton.Click += (s, e) =>
            {
                del.SensorArm();

                ArmButton.Visibility = ViewStates.Gone;
                showInstructions();
            };

            NextImageButton = FindViewById<Button>(Resource.Id.btn_next);
            NextImageButton.Click += (s, e) =>
            {
               updateInstruction();
                
            };

            realtimeCheckBox = FindViewById<CheckBox>(Resource.Id.chkb_realtime);
            realtimeCheckBox.Click += (s, e) =>
            {
                if (realtimeCheckBox.Checked)
                    _model.realTimeCollection = true;
                else
                    _model.realTimeCollection = false;
            };

            
            MVCButton.Click += (s, e) =>
            {
                StartActivity(typeof(MVCActivity));
            };

            
            ExerciseButton.Click += (s, e) =>
            {
                StartActivity(typeof(ExerciseSelectionActivity));
            };

            //TODO get rid later
            var all = _model.getAllSessions();
            foreach(Session s in all)
            {
                System.Console.WriteLine("CC: " + s.date);
            }
            System.Console.WriteLine("CC: " + all.Count);
        }

        public class VideoLoop : Java.Lang.Object, MediaPlayer.IOnPreparedListener
        {
            public void OnPrepared(MediaPlayer mp)
            {
                mp.Looping = true;
            }
        }
        public void displayRescanUI()
        {
            RunOnUiThread(() =>
            {
                TitleText.Text = Resources.GetString(Resource.String.rescan_txt);
                RescanButton.Visibility = ViewStates.Visible;
                RescanButton.Text = "Search again ... ";
            });
        }

        public void showInstructions()
        {
            
            imageFrame.Visibility = ViewStates.Visible;
            TitleText.Text = "Follow these instructions";
            instrucText = FindViewById<TextView>(Resource.Id.txv_instruction);
            instrucText.Visibility = ViewStates.Visible;
            getImageLocations();
            updateInstruction();

            MediaController mController = new Android.Widget.MediaController(this);

            mController.SetAnchorView(ApplyVideo);

            ApplyVideo.SetVideoPath(setup_path); // Path of your saved video file.
            ApplyVideo.SetMediaController(mController);
            ApplyVideo.Start();
            ApplyVideo.SetOnPreparedListener(new VideoLoop());
        }

        public string[] imageNames =  {"info_wipe","info_stick","info_peel","info_apply"};
        public string[] imageDescriptions = { "1. Wipe the area with an alcohol wipe and let dry", "2. Add a sticker to each sensor", "3. Peel the backing off the stickers", "4. Apply firmly to the skin" };
        public int[] imageIds;
        public int counter = 0;
        private void getImageLocations() //todo move this to model
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
                NextImageButton.Visibility = ViewStates.Gone;
                ExerciseButton.Visibility = ViewStates.Visible;
                MVCButton.Visibility = ViewStates.Visible;

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