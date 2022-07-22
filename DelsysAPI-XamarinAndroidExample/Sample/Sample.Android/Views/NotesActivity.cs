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

namespace AndroidSample.Views
{
    [Activity(Label = "NotesActivity",Theme = "@style/AppTheme.NoActionBar")]
    public class NotesActivity : Android.Support.V7.App.AppCompatActivity
    {
        Button HomeButton;
        Button AddButton;
        EditText NotesText;

        private MainModel _myModel;
        public NotesActivity()
        {
            _myModel = MainModel.Instance;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_notes);

            NotesText = FindViewById<EditText>(Resource.Id.edt_notes);

            HomeButton = FindViewById<Button>(Resource.Id.btn_end);
            HomeButton.Click += (s, e) =>
            {
                StartActivity(typeof(MainActivity));
            };
            AddButton = FindViewById<Button>(Resource.Id.btn_add);
            AddButton.Click += (s, e) =>
            {
                _myModel.currentSession.notes = NotesText.Text;
            };

        }
    }
}