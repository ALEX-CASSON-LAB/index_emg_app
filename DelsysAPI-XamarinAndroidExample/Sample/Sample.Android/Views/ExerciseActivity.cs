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
    [Activity(Label = "@string/exercise_title", Theme = "@style/AppTheme")]
    public class ExerciseActivity : Android.Support.V7.App.AppCompatActivity
    {
        Button StartButton;
        Button StopButton;
        Button NextButton;

        MainModel _model;

        Delsys del;

        public ExerciseActivity()
        {
            _model = MainModel.Instance;
            del = _model.del;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_exercise);

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

            };

            NextButton = FindViewById<Button>(Resource.Id.btn_next);
            NextButton.Click += (s, e) =>
            {
                StartActivity(typeof(ExerciseActivity));
                Console.WriteLine("TODO next add exercises");
            };
        }
    }
}