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

namespace AndroidSample.Views
{
    [Activity(Label = "DisplayStatsActivity", Theme = "@style/AppTheme.NoActionBar")]
    public class DisplayStatsActivity : Android.Support.V7.App.AppCompatActivity
    {
        private MainModel _myModel;

        //UI items
        TextView exerciseName;
        TextView exerciseStats;
        Button NotesButton;
        Button HomeButton;

        public DisplayStatsActivity()
        {
            _myModel = MainModel.Instance;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_displayStats);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Session statistics";

            exerciseName = FindViewById<TextView>(Resource.Id.txv_names);
            exerciseStats = FindViewById<TextView>(Resource.Id.txv_stats);
            NotesButton = FindViewById<Button>(Resource.Id.btn_notes);
            HomeButton = FindViewById<Button>(Resource.Id.btn_home);

            displayStats();

        }
        public void displayStats()
        {
            Session displaySession = _myModel.getSessionStats();

            string names = displaySession.exerciseIds;
            //string stats = "[";

            ////foreach (KeyValuePair<string, List<double>> exer in displaySession.exerciseStats)
            ////{
            ////    names = names + "],[" + _myModel.getExerciseNameById(exer.Key);
            ////    stats = stats + "] [" + exer.Value.ToString();
            ////}

            //names = names + "]";
            //stats += "]";

            

            string stats = displaySession.exerciseStats;

            exerciseName.Text = names;
            exerciseStats.Text = stats;
        }
    }
    
}