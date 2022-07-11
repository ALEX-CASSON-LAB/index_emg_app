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
            _exerciseList = _myModel.getExercises();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // view set up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_exercise);
            string exercise_name = Intent.GetStringExtra("exercise_name");
            TitleText = FindViewById<TextView>(Resource.Id.txv_title);
            TitleText.Text = exercise_name;

            // make exercise by id TODO using database info
            _currentExercise = new Exercise();
            _currentExercise.name = exercise_name;
            _currentExercise.reps = 1;


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