using Android.App;
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


        private MainModel _myModel;

        Delsys del;

        private List<Exercise> _exerciseList; //holds all the exercises available in the exercise database
        private Exercise _currentExercise;
        List<List<double>> exerciseData = new List<List<double>>();

        public ExerciseActivity()
        {
            _myModel = MainModel.Instance;
            del = _myModel.del;
            _exerciseList = _myModel.getExercises(); //TODO do something with this, also cant be used whilst not adding exercises properly
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // view set up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_exercise);
            string exercise_name = Intent.GetStringExtra("exercise_name");
            int exercise_id = Int32.Parse(Intent.GetStringExtra("exercise_id")); //theres a tryparse for it if it fails
            TitleText = FindViewById<TextView>(Resource.Id.txv_title);
            TitleText.Text = exercise_name;

            // make exercise by id TODO using database info
            _currentExercise = new Exercise();
            _currentExercise.name = exercise_name;
            _currentExercise.reps = 1;

            //set up image 
            ExerciseImage = FindViewById<ImageView>(Resource.Id.im_exercise1);
            ExerciseImage.SetImageResource(exercise_id);

            //todo understand and maybe fix idk
            //RelativeLayout lay = FindViewById<RelativeLayout>(Resource.Id.exercise_layout);
            //var w = lay.LayoutParameters.Width;
            ////var h = lay.LayoutParameters.Height;
            //var h = ExerciseImage.LayoutParameters.Width;
            
            //ExerciseImage.LayoutParameters.Width = w/4;
            //ExerciseImage.LayoutParameters.Height = h;

            //set up buttons
            StartButton = FindViewById<Button>(Resource.Id.btn_start);
            StartButton.Click += (s, e) =>
            {
                if (del != null)
                    del.SensorStream();
                else
                    Console.WriteLine("DELSYS object not initialised"); // TODO add check
                StopButton.Visibility = ViewStates.Visible;
            };

            StopButton = FindViewById<Button>(Resource.Id.btn_stop);
            StopButton.Click += async (s, e) =>
            {
                await del.SensorStop();
                Task.Delay(3000).Wait();
                StopButton.Visibility = ViewStates.Invisible;

                exerciseData = del.Normalise(_myModel.mvc);
            };

            NextButton = FindViewById<Button>(Resource.Id.btn_next);
            NextButton.Click += (s, e) =>
            {
                storeResult();
                StartActivity(typeof(ExerciseSelectionActivity));
            };

            EndSessionButton = FindViewById<Button>(Resource.Id.btn_end);
            EndSessionButton.Click += (s, e) =>
            {
                storeResult();
                _myModel.recordCurrentSession();
                StartActivity(typeof(DisplayStatsActivity));
            };

            allowStart();

            if (del != null)
                del.clearData();// clear the previous data to get only this exercises data
            //TODO do this when you finish a set?
        }
        public async void allowStart()
        {
            await Task.Delay(5000); // WAIT BEFORE ALLOWING TO CLICK
            StartButton.Enabled = true;
        }

        public void storeResult()
        {
            double maxValue = 0; //store the highest value in the exercise data
            foreach (List<double> channel in exerciseData)
            {
                maxValue = channel.Max(z=>z);
                //TODO make this work with more than one channel
                // list of max percents?
            }

            _myModel.currentSession.addExerciseStat(_currentExercise, maxValue);
        }
    }
}