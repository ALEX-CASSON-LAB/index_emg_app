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

            //displayStats();

        }
        //public void displayStats()
        //{
        //    Session displaySession = _myModel.getSessionStats();

        //    string names = displaySession.exerciseIds;

        //    string stats = displaySession.exerciseStats;

        //    exerciseName.Text = names;
        //    exerciseStats.Text = stats;
        //}

        private JavaList<Exercise> getExercises()
        {
            var exercises = new JavaList<Exercise>();

            Exercise s;

            s = new Exercise(); // TODO get from the current session
            s.name = "Pelvic tilt";
            s.reps = 5;
            s.img_name = "icon_good_review";
            // TODO calculate what the performance was
            exercises.Add(s);

            s = new Exercise(); // TODO get from the current session
            s.name = "Stomach tone";
            s.reps = 3;
            s.img_name = "icon_ranking";
            // TODO calculate what the performance was
            exercises.Add(s);

            s = new Exercise(); // TODO get from the current session
            s.name = "Buttock tone";
            s.reps = 5;
            s.img_name = "icon_star_color";
            // TODO calculate what the performance was
            exercises.Add(s);

            return exercises;
        }

    }
}