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

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class MainActivity : Android.Support.V7.App.AppCompatActivity
    {
        Button InfoActivityButton;
        MainModel myModel;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // View set up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar1);
            toolbar.SetLogo(Resource.Drawable.index_logo);
            SetSupportActionBar(toolbar);
            CheckAppPermissions();

            // Model set up
            myModel = MainModel.Instance;

            // Button set up
            InfoActivityButton = FindViewById<Button>(Resource.Id.btn_begin);
            InfoActivityButton.Click += delegate
            {
                StartActivity(typeof(InformationActivity));
            };

            /********SHORTCUT********/
            //Button shortcutButton = FindViewById<Button>(Resource.Id.btn_shortcut);
            //shortcutButton.Click += delegate
            //{
            //    StartActivity(typeof(Views.ExerciseSelectionActivity));
            //};
            /***********************/

            myModel.readExerciseJSON();
            myModel.setupDatabase();

        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }


        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                StartActivity(typeof(SettingsActivity));
                return true;
             
            }
            else if (id == Resource.Id.action_profile)
            {
                return true;
                //TODO add a profile page
            }

            return base.OnOptionsItemSelected(item);
        }

        #region Check App permission

        // Check and request permissions
        private void CheckAppPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                return;
            }
            else
            {
                if (PackageManager.CheckPermission(Manifest.Permission.ReadExternalStorage, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, PackageName) != Permission.Granted)
                {
                    var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage, Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation, Manifest.Permission.Bluetooth, Manifest.Permission.BluetoothAdmin };
                    RequestPermissions(permissions, 1);
                }
            }
        }


        #endregion
    }
}