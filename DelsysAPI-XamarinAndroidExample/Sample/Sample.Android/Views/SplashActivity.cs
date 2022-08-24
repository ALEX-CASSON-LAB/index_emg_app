using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidSample.Views
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.Splash",MainLauncher = true, NoHistory = true, Icon = "@drawable/logo_circle")]
    
    public class SplashActivity : Android.Support.V7.App.AppCompatActivity
    {
        MainModel myModel;
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
            Log.Debug(TAG, "SplashActivity.OnCreate");
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { SimulateStartup(); });
            startupWork.Start();
        }

        // Simulates background work that happens behind the splash screen
        async void SimulateStartup()
        {
            // Model set up
            myModel = MainModel.Instance;
            myModel.readExerciseJSON();
            myModel.setupDatabase();
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
        //to prevent the back button from canceling the startup process, you can also override OnBackPressed and have it do nothing:
        public override void OnBackPressed() { }
    }
}