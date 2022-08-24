﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidSample.Core;
using System.Threading.Tasks;
using System.ComponentModel;
using Android.Support.V4.Content;
using Android.Media;

namespace AndroidSample.Views
{
    [Activity(Label = "@string/exercise_title", Theme = "@style/AppTheme.NoActionBar")]
    public class ExerciseActivity : Android.Support.V7.App.AppCompatActivity
    {
        Button StartButton;
        Button StopButton;
        Button NextButton;
        Button EndSessionButton;
        ImageView ExerciseImage;

        TextView TitleText;
        TextView ExerciseDescriptionText;

        ProgressBar DataProgBar;
        ProgressBar DataProgBar2;

        TextView DataText;
        TextView DataText2;

        BackgroundWorker startWorker;
        BackgroundWorker stopWorker;
        BackgroundWorker dataWorker;

        private MainModel _myModel;

        private VideoView ExerciseVideo;
        private string exercise_path;

        Button realtimeButton;
        Button postButton;
        Button selectButton;
        bool realtime;

        Delsys del;

        private Exercise _currentExercise;

        public ExerciseActivity()
        {
            _myModel = MainModel.Instance;
            del = _myModel.del;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _myModel.SetUpExerciseValues(); //TODO move this to a more appropriate place

            startWorker = new BackgroundWorker();
            stopWorker = new BackgroundWorker();
            dataWorker = new BackgroundWorker();

            // view set up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_exercise);

            //Realtime vs post exercise selection
            realtimeButton = FindViewById<Button>(Resource.Id.btn_realtime);
            postButton = FindViewById<Button>(Resource.Id.btn_post);
            selectButton = FindViewById<Button>(Resource.Id.btn_select);


            realtimeButton.Click += (s, e) =>
            {
                realtime = !realtime;

                changeButtonColor();
            };
            postButton.Click += (s, e) =>
            {
                if (realtime == true)
                    realtime = !realtime;

                changeButtonColor();
            };

            selectButton.Click += (s, e) =>
            {
                if (realtime == true)
                    _myModel.realTimeCollection = true;
                else
                    _myModel.realTimeCollection = false;
                FindViewById<RelativeLayout>(Resource.Id.layout_realtime).Visibility=ViewStates.Gone;
                FindViewById<RelativeLayout>(Resource.Id.layout_exercise).Visibility=ViewStates.Visible;
                DataProgBar = FindViewById<ProgressBar>(Resource.Id.progBar_data);
                DataProgBar2 = FindViewById<ProgressBar>(Resource.Id.progBar_data2);

                if (_myModel.realTimeCollection == false)
                    FindViewById<LinearLayout>(Resource.Id.layout_progress_bars).Visibility = ViewStates.Invisible;
                else
                {
                    FindViewById<LinearLayout>(Resource.Id.layout_progress_bars).Visibility = ViewStates.Visible;
                    //TODO depending on how many chanels
                    
                    if (del.sensors.Count == 2)
                    {
                        FindViewById<LinearLayout>(Resource.Id.layout_progress2).Visibility = ViewStates.Visible;
                    }  
                }  
            };


            int image_id = Int32.Parse(Intent.GetStringExtra("image_id"));
            int exercise_id = Int32.Parse(Intent.GetStringExtra("exercise_id"));
            
            _currentExercise = _myModel.GetExercise(exercise_id);

            TitleText = FindViewById<TextView>(Resource.Id.txv_title);
            TitleText.Text = _currentExercise.name;

            // Set description of exercise
            ExerciseDescriptionText = FindViewById<TextView>(Resource.Id.txv_description);
            ExerciseDescriptionText.Text = _currentExercise.description;

            DataText = FindViewById<TextView>(Resource.Id.txv_data);
            DataText2 = FindViewById<TextView>(Resource.Id.txv_data2);

            //set up image 
            ExerciseImage = FindViewById<ImageView>(Resource.Id.im_exercise1);
            ExerciseImage.SetImageResource(image_id);
            ExerciseVideo = FindViewById<VideoView>(Resource.Id.video_exercise);

            MediaController mController = new Android.Widget.MediaController(this);
            mController.SetAnchorView(ExerciseVideo);

            var videoId = (int)Resources.GetIdentifier(_currentExercise.vid_name,"raw", PackageName);
            exercise_path = string.Format("android.resource://{0}/{1}", PackageName, videoId);

            ExerciseVideo.SetVideoPath(exercise_path); // Path of your saved video file.
            ExerciseVideo.SetMediaController(mController);
            ExerciseVideo.Start();
            ExerciseVideo.SetOnPreparedListener(new VideoLoop());

           
            //set up buttons
            StartButton = FindViewById<Button>(Resource.Id.btn_start);
            StartButton.Click += (s, e) =>
            {
                StartButton.SetBackgroundColor(Android.Graphics.Color.Green);
                StartButton.Text = "Recording";
                StartButton.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
                startCollection();
            };
            startWorker.DoWork += async (o, e) =>
            {
                if (del != null)
                    await del.SensorStream();
                else
                    Console.WriteLine("DELSYS object not initialised"); // TODO add check
            };
            StopButton = FindViewById<Button>(Resource.Id.btn_stop);
            StopButton.Click += async (s, e) =>
            {
                StopButton.Visibility = ViewStates.Invisible;
                await del.SensorStop();
            };
            stopWorker.DoWork += (o, e) =>
            {
                Task.Delay(3000).Wait();
                //exerciseData = del.Normalise(_myModel.mvc); //TODO add to list 
            };

            NextButton = FindViewById<Button>(Resource.Id.btn_next);
            NextButton.Click += (s, e) =>
            {
                StoreResult();
                _myModel.ClearExerciseStats();
                StartActivity(typeof(ExerciseSelectionActivity));
            };

            EndSessionButton = FindViewById<Button>(Resource.Id.btn_end);
            EndSessionButton.Click += (s, e) =>
            {
                StoreResult();
                _myModel.RecordCurrentSession();

                Intent intent = new Intent(this, typeof(DisplayStatsActivity));
                intent.PutExtra("session_id", _myModel.currentSession.Id.ToString());
                StartActivity(intent);
            };

            allowStart();

            if (del != null)
                del.ClearData();// clear the previous data to get only this exercises data

            del.CollectionStopped += DelStopCollection;


            del.MuscleActive += (object sender, Delsys.MuscleActiveEventArgs e)
                    =>
            {
                double[] sensor1_data = e.MuscleData[0]; //data from first channel
                var data1_mean = sensor1_data.Average();
                double data2_mean = double.NaN;
                if (del.sensors.Count == 2)
                {
                    double[] sensor2_data = e.MuscleData[1]; // data from second channel
                    data2_mean = sensor2_data.Average();
                    _myModel.AddExerciseValue(1, data2_mean);
                }

                    _myModel.AddExerciseValue(0,data1_mean);
                if (_myModel.realTimeCollection == true)
                {
                    RunOnUiThread(() =>
                    {
                        DataText.Text = Math.Round(data1_mean,2).ToString() + " %";

                        DataProgBar.SetProgress(Math.Min((int)data1_mean, 100), false);

                        if (del.sensors.Count == 2)
                        {
                            DataText2.Text = Math.Round(data2_mean,2).ToString() + " %";
                            DataProgBar2.SetProgress(Math.Min((int)data2_mean, 100), false);
                        }
                    });
                    
                }
            };
        }

        private void changeButtonColor()
        {
            if (realtime == true)
            {
                realtimeButton.SetBackgroundColor(Android.Graphics.Color.Gray);
                postButton.SetBackgroundColor(Android.Graphics.Color.White);
            }
            else
            {
                realtimeButton.SetBackgroundColor(Android.Graphics.Color.White);
                postButton.SetBackgroundColor(Android.Graphics.Color.Gray);
            }
            selectButton.Enabled = true;
        }

        public async void allowStart()
        {
            await Task.Delay(5000); // Wait 5 seconds before enabling button - this is necessary to ensure that the sensors are ready
            StartButton.Text = "Start recording";
            var draw = ContextCompat.GetDrawable(this, Resource.Drawable.icon_play_arrow);
            StartButton.SetCompoundDrawablesWithIntrinsicBounds(draw, null, null, null);
            StartButton.Enabled = true;
        }
        public void startCollection()
        {
            RunOnUiThread(() =>
            {
                StartButton.Enabled = false;
                NextButton.Text = "Next Exercise";
            });
            startWorker.RunWorkerAsync();
            RunOnUiThread(() =>
            {
                Task.Delay(3000).Wait();
                StopButton.Visibility = ViewStates.Visible;
            });
        }
        public void stopCollection()
        {
            stopWorker.RunWorkerAsync();
            RunOnUiThread(() =>
            {
                StartButton.Enabled = true;
                StartButton.SetBackgroundResource(Resource.Color.colorButton);
                StartButton.Text = "Start Recording";
                var draw = ContextCompat.GetDrawable(this, Resource.Drawable.icon_play_arrow);
                StartButton.SetCompoundDrawablesWithIntrinsicBounds(draw, null, null, null);
            });
        }
        public void StoreResult()
        {
            var perform = _myModel.CalculatePerformace();

            _myModel.currentSession.addExerciseStat(_currentExercise, perform);
        }
        public void DelStopCollection(object sender, Delsys.CollectionStoppedEventArgs e)
        {

            Task.Delay(3000).Wait();
            Console.WriteLine("CC: Count of data - " + e.DataCount.ToString());

            if (e.DataCount < 10)
            {
                Console.WriteLine("ERROR: Start didnt work. Try again");
                Task.Delay(15000).Wait();
                startCollection();
            }
            else
            {
                stopCollection();
            }
            //TODO DISPLAY ERROR ON PAGE AS PLS WAIT

        }
        public class VideoLoop : Java.Lang.Object, MediaPlayer.IOnPreparedListener
        {
            public void OnPrepared(MediaPlayer mp)
            {
                mp.Looping = true;
                mp.SetVolume(0f, 0f);
            }
        }
    }
}