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
        private Session displaySession;

        //UI items

        Button NotesButton;
        Button HomeButton;

        private ListView lv;
        private CustomListAdapter adapter;

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

            int session_id = Int32.Parse(Intent.GetStringExtra("session_id"));

            Session session = _myModel.GetSession(session_id);

            var exercises = getExercises(session);
            if (exercises != null)
            {
                var stats = GetStats(session);
                lv = FindViewById<ListView>(Resource.Id.view_stats_list);
                adapter = new CustomListAdapter(this,exercises,stats);
                lv.Adapter = adapter;
            }


            NotesButton = FindViewById<Button>(Resource.Id.btn_notes);
            NotesButton.Click += (s, e) =>
            {
                StartActivity(typeof(NotesActivity));
            };
            HomeButton = FindViewById<Button>(Resource.Id.btn_home);
            HomeButton.Click += (s, e) =>
            {
                _myModel.EndSession();
                StartActivity(typeof(MainActivity));
            };

        }
        

        private JavaList<Exercise> getExercises(Session ses)
        {
            string ids = ses.exerciseIds;
            var exercises = new JavaList<Exercise>();

            if (ids != null)
            {
                var lst = ids.Split(',').ToList();
                foreach (var val in lst)
                {
                    int id;
                    bool isint = int.TryParse(val, out id);
                    if (isint == true)
                    {
                        Exercise ex = _myModel.GetExercise(id);
                        exercises.Add(ex);
                    }
                }
                return exercises;
            }

            return null;
        }

        private List<int> GetStats(Session ses)
        {
            List<int> retStats = new List<int>();
            string stats = ses.exerciseStats;

            var lst = stats.Split(',').ToList();

            foreach (var val in lst)
            {
                int stat;
                bool isint = int.TryParse(val, out stat);
                if (isint == true)
                {
                    retStats.Add(stat);
                }
            }
            return retStats;
        }

    }
}