using System;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using Android;
using System.IO;

namespace AndroidSample

{

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : Android.Support.V7.App.AppCompatActivity
    {
        Button InfoActivityButton;
        MainModel myModel;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // View set up
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
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

            myModel.readExerciseJSON();

            //Data set up
            getDatabasePath();

            myModel.setupDatabase();
            myModel.accessDatabase();
            
        }

        private void getDatabasePath()
        {
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),"database.db3");
            // dbPath contains a valid file path for the database file to be stored
            MainModel.Instance.dbPath = dbPath;
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
                return true;
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
                    var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage, Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation };
                    RequestPermissions(permissions, 1);
                }
            }
        }


        #endregion
    }
}