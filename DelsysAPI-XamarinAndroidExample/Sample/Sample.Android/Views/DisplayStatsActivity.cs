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

        private ListView lv;
        private CustomListAdapter adapter;
        private JavaList<Exercise> exercises;

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


            lv = FindViewById<ListView>(Resource.Id.view_stats_list);

            adapter = new CustomListAdapter(this, getExercises());

            lv.Adapter = adapter;


            NotesButton = FindViewById<Button>(Resource.Id.btn_notes);
            NotesButton.Click += (s, e) =>
            {
                StartActivity(typeof(NotesActivity));
            };
            HomeButton = FindViewById<Button>(Resource.Id.btn_home);
            HomeButton.Click += (s, e) =>
            {
                StartActivity(typeof(MainActivity));
            };

        }
        

        private JavaList<Exercise> getExercises()
        {
            List<int> exercisesDoneIds = _myModel.getExercisesDone();

            var exercises = new JavaList<Exercise>();

            foreach ( int id in exercisesDoneIds)
            {
                Exercise ex = _myModel.GetExercise(id);
                exercises.Add(ex);
            }
            
            return exercises;
        }

    }
}