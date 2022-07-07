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
using AndroidSample.Views;

namespace AndroidSample
{
    [Activity(Label = "MVC", Theme = "@style/AppTheme.NoActionBar")]
    public class MVCActivity : Android.Support.V7.App.AppCompatActivity
    {
        Button StartButton;
        Button StopButton;
        Button NextButton;

        private MainModel _myModel;

        Delsys del;
        public MVCActivity()
        {
            _myModel = MainModel.Instance;
            del = _myModel.del;
        }
       

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_mvc);

            

            StartButton = FindViewById<Button>(Resource.Id.btn_start);
            StartButton.Click += (s, e) =>
            {
                if (del != null)
                    del.SensorStream();
                else
                    Console.WriteLine("DELSYS object not initialised"); // TODO add 
                StopButton.Visibility = ViewStates.Visible;
            };

            StopButton = FindViewById<Button>(Resource.Id.btn_stop);
            StopButton.Click += async (s, e) =>
            {
                await del.SensorStop();
                Task.Delay(3000).Wait();
                StopButton.Visibility = ViewStates.Invisible;

                //Calculate MVC
                _myModel.mvc = calculate_MVC(del.Data);
                Console.WriteLine("mvc is {0}", _myModel.mvc);

                allowStart();
            };

            NextButton = FindViewById<Button>(Resource.Id.btn_next);
            NextButton.Click += (s, e) =>
            {
                StartActivity(typeof(ExerciseActivity));
            };

            allowStart();

        }
        // Must delay the start to avoid crashing. Delsys API requirements.
        public async void allowStart()
        {
            await Task.Delay(5000); // WAIT BEFORE ALLOWING TO CLICK
            StartButton.Enabled = true;
        }

        public double calculate_MVC(List<List<double>> data) // whats the datatpe of data
        {
            double mvc;
            double sum = 0;
            for (int i = 0; i < data.Count; i++)
            {
                Console.WriteLine("count of data {0}", data.Count);
                foreach (var pt in data[i])
                {
                    sum = sum + (pt * pt); // Add the squares of all values
                }
            }

            mvc = sum / data[0].Count;
            mvc = Math.Sqrt(mvc);
            return mvc;
        }


    }
}