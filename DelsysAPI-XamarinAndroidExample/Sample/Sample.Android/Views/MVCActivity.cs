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
                {
                    del.mvcCollection = true;
                    del.SensorStream();
                }                    
                else
                    Console.WriteLine("DELSYS object not initialised"); // TODO add 

                StartButton.SetBackgroundColor(Android.Graphics.Color.Green);
                StartButton.Text = "Recording";
                StartButton.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);

                StopButton.Visibility = ViewStates.Visible;
            };

            StopButton = FindViewById<Button>(Resource.Id.btn_stop);
            StopButton.Click += async (s, e) =>
            {
                await del.SensorStop();
                Task.Delay(3000).Wait();
                StopButton.Visibility = ViewStates.Invisible; //TODO bit dramatic remove

                //Calculate MVC
                _myModel.UpdateMvcs(del.calculate_MVC());
                

                StartButton.Text = "Redo recording";
                Drawable img = GetDrawable(Resource.Drawable.icon_restart);
                StartButton.SetCompoundDrawablesWithIntrinsicBounds(img, null, null, null);
                StartButton.SetBackgroundResource(Resource.Color.colorButton);
                NextButton.Text = "Next";
                allowStart();
            };

            NextButton = FindViewById<Button>(Resource.Id.btn_next);
            NextButton.Click += (s, e) =>
            {
                del.mvcCollection = false;
                StartActivity(typeof(ExerciseSelectionActivity));
            };

            allowStart();

        }

        /// <summary>
        /// Delays the start button appearing for 5 seconds. This is a clean up for the Delsys API.
        /// Without waiting a couple of seconds, the connection may crash
        public async void allowStart()
        {
            await Task.Delay(5000);
            StartButton.Enabled = true;
        }

    }
}