using System;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using Android;
using System.IO;
using AndroidSample.Views;

namespace AndroidSample

{

    [Activity(Label = "@string/settings_title", Theme = "@style/AppTheme.NoActionBar")]
    public class SettingsActivity : Android.Support.V7.App.AppCompatActivity
    {
        MainModel myModel;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // View set up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            toolbar.SetLogo(Resource.Drawable.index_logo);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            // Model set up
            myModel = MainModel.Instance;

        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            //Back button pressed -> toggle event
            if (item.ItemId == Android.Resource.Id.Home)
                this.OnBackPressed();

            return base.OnOptionsItemSelected(item);
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

    }
}