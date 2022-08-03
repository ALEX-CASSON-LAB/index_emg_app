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
using System.ComponentModel;

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
        TextView DataText;
        TextView ExerciseDescriptionText;


        private MainModel _myModel;

        Delsys del;

        private List<Exercise> _exerciseList; //holds all the exercises available in the exercise database
        private Exercise _currentExercise;
        List<List<double>> exerciseData = new List<List<double>>();

        public ExerciseActivity()
        {
            _myModel = MainModel.Instance;
            del = _myModel.del;
            _exerciseList = _myModel.availableExercises; //TODO do something with this, also cant be used whilst not adding exercises properly
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // view set up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_exercise);

            int image_id = Int32.Parse(Intent.GetStringExtra("image_id"));
            int exercise_id = Int32.Parse(Intent.GetStringExtra("exercise_id")); //theres a tryparse for it if it fails
            // make exercise by id TODO using database info
            _currentExercise = _myModel.GetExercise(exercise_id);

            TitleText = FindViewById<TextView>(Resource.Id.txv_title);
            TitleText.Text = _currentExercise.name;

            // Set description of exercise
            ExerciseDescriptionText = FindViewById<TextView>(Resource.Id.txv_description);
            ExerciseDescriptionText.Text = _currentExercise.description; //todo process to look prettier

            DataText = FindViewById<TextView>(Resource.Id.txv_data);

            //set up image 
            ExerciseImage = FindViewById<ImageView>(Resource.Id.im_exercise1);
            ExerciseImage.SetImageResource(image_id);

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
                //if (del != null)
                //    del.SensorStream();
                //else
                //    Console.WriteLine("DELSYS object not initialised"); // TODO add check
                //StopButton.Visibility = ViewStates.Visible;
                //StartButton.Enabled = false;
                //NextButton.Text = "Next Exercise";
                startCollection();
            };

            StopButton = FindViewById<Button>(Resource.Id.btn_stop);
            StopButton.Click += async (s, e) =>
            {
                await del.SensorStop();
                //Task.Delay(3000).Wait();
                //StopButton.Visibility = ViewStates.Invisible;
                ////todo add one to the rep
                //exerciseData = del.Normalise(_myModel.mvc);
                //StartButton.Enabled = true;
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
                del.ClearData();// clear the previous data to get only this exercises data
            //TODO do this when you finish a set?

            if (_myModel.realTimeCollection == true)
            {
                del.MuscleActive += (object sender, Delsys.MuscleActiveEventArgs e)
                        => {
                            double[] d = e.MuscleData[0];
                            d = MainModel.fullWaveRectification(d);
                            double avg = d.Average(); //avg of the 10 datapoints provided
                            var normAvg = Math.Truncate((avg / _myModel.mvc) * 100);
                            Console.WriteLine(avg.ToString() + " /  " + _myModel.mvc.ToString() + "*100 = " + normAvg.ToString());
                            DataText.Text = normAvg.ToString();
                        };
            }


            del.CollectionStopped += DelStopCollection;
        }
        public async void allowStart()
        {
            await Task.Delay(5000); // WAIT BEFORE ALLOWING TO CLICK
            StartButton.Enabled = true;
        }
        public async void startCollection()
        {
            StopButton.Visibility = ViewStates.Visible;
            //StartButton.Enabled = false;
            NextButton.Text = "Next Exercise";
            if (del != null)
                await del.SensorStream();
            else
                Console.WriteLine("DELSYS object not initialised"); // TODO add check

        }
        public void stopCollection()
        {
            Task.Delay(3000).Wait();
            StopButton.Visibility = ViewStates.Invisible;
            exerciseData = del.Normalise(_myModel.mvc);
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
        public void DelStopCollection(object sender, Delsys.CollectionStoppedEventArgs e)
        {

            Task.Delay(3000).Wait();
            Console.WriteLine("CC: Count of data - " + e.DataCount.ToString());

            //todo check how many count and automatically restart
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

            //DISPLAY ERROR ON PAGE AS PLS WAIT


        }
    }
}