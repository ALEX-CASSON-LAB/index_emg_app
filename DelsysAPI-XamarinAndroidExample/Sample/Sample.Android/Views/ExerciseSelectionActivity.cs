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
    [Activity(Label = "ExerciseSelectionActivity",Theme = "@style/AppTheme.NoActionBar")]
    public class ExerciseSelectionActivity : Android.Support.V7.App.AppCompatActivity
    {
        GridView gridView;
        string[] gridViewString =
        {
            "Hamstring","Legraise"
        };
        int[] imageId =
        {
            Resource.Drawable.va_hamstring,Resource.Drawable.va_legraise
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_exerciseSelection);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Exercises available";

            CustomGridViewAdapter adapter = new CustomGridViewAdapter(this, gridViewString, imageId);
            gridView = FindViewById<GridView>(Resource.Id.grid_view_image_text);
            gridView.Adapter = adapter;
            gridView.ItemClick += (s, e) =>
            {
                Toast.MakeText(this, "GridView Item:" + gridViewString[e.Position], ToastLength.Short).Show();
            };
        }
    }
}