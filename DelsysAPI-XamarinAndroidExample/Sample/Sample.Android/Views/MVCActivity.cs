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
using Android.Graphics.Drawables;

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
            // view set-up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_mvc);


            // button set-up
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
                StopButton.Visibility = ViewStates.Invisible; //TODO bit dramatic remove

                //Calculate MVC
                _myModel.mvc = calculate_MVC(del.Data)[0]; // todo make this per channel
                Console.WriteLine("mvc is {0}", _myModel.mvc);
                StartButton.Text = "Redo recording";
                Drawable img = GetDrawable(Resource.Drawable.icon_restart);
                StartButton.SetCompoundDrawablesWithIntrinsicBounds(img, null, null, null);
                NextButton.Text = "Next";
                allowStart();
            };

            NextButton = FindViewById<Button>(Resource.Id.btn_next);
            NextButton.Click += (s, e) =>
            {
                StartActivity(typeof(ExerciseSelectionActivity));
            };

            allowStart();

        }
        // Delays the start button appearing for 5 seconds. This is a clean up for the Delsys API.
        // Without waiting a couple of seconds, the connection may crash
        public async void allowStart()
        {
            await Task.Delay(5000); // WAIT BEFORE ALLOWING TO CLICK
            StartButton.Enabled = true;
        }

        public List<double> calculate_MVC(List<List<double>> data)
        {
            double mvc = 1; // default 
            double sum = 0;

            List<double> mvcs = new List<double>();

            for (int i = 0; i < data.Count; i++) // For each channel/sensor
            {
                foreach (var pt in data[i]) // for each data point
                {
                    sum = sum + (pt * pt); // Add the squares of all values
                }
                mvc = sum / data[i].Count;
                mvc = Math.Sqrt(mvc); // square root

                mvcs.Add(mvc);
            }

            return mvcs;
        }


    }
}