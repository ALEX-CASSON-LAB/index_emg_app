using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidSample.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndroidSample.Views
{
    [Activity(Label = "SessionHistoryActivity", Theme = "@style/AppTheme.NoActionBar")]
    public class SessionHistoryActivity : Android.Support.V7.App.AppCompatActivity
    {
        private MainModel _myModel;

        private ListView lv;
        private SessionListAdapter adapter;
        private JavaList<Exercise> exercises;

        Button HomeButton;

        public SessionHistoryActivity()
        {
            _myModel = MainModel.Instance;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_sessionHistory);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Session history";

            lv = FindViewById<ListView>(Resource.Id.view_stats_list);

            adapter = new SessionListAdapter(this, getAllSessions());

            lv.Adapter = adapter;

            HomeButton = FindViewById<Button>(Resource.Id.btn_home);
            HomeButton.Click += (s, e) =>
            {
                StartActivity(typeof(MainActivity));
            };
        }
        private JavaList<Session> getAllSessions()
        {
            List<Session> allSessions = _myModel.getAllSessions();
            allSessions.Reverse();// In order to have the most recent session first

            var sessions = new JavaList<Session>();

            foreach (Session ses in allSessions)
            {
                sessions.Add(ses);
            }

            return sessions;
        }
    }
}


