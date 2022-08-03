using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;

namespace AndroidSample.Views
{
    [Activity(Label = "ExerciseSelectionActivity",Theme = "@style/AppTheme.NoActionBar")]
    public class ExerciseSelectionActivity : Android.Support.V7.App.AppCompatActivity
    {
        private MainModel _myModel;
        GridView gridView;
        List<string>[] exerciseInfo;
        string[] gridViewString;
        string[] gridViewImg;
        int[] exerciseIds;
        int[] imageResId;
        List<int> exercisesDone;

        public ExerciseSelectionActivity()
        {
            _myModel = MainModel.Instance;
            exerciseInfo = _myModel.getExerciseInfo();
            gridViewString = exerciseInfo[0].ToArray(); // names
            gridViewImg = exerciseInfo[1].ToArray(); // image ids
            exerciseIds = Array.ConvertAll(exerciseInfo[2].ToArray(), s => int.Parse(s)); ;
            convertImageLocation();
        }

        private void convertImageLocation()
        {          
            imageResId = new int[exerciseInfo[1].Count];
            for (int i = 0; i < gridViewImg.Length;i++)
            {
                string imL = gridViewImg[i];
                var resourceId = (int)typeof(Resource.Drawable).GetField(imL).GetValue(null);
                imageResId[i] = resourceId;
            }
        }
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_exerciseSelection);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Exercises available";

            //TODO display exercises already done differently
            exercisesDone = _myModel.getExercisesDone();

            CustomGridViewAdapter adapter = new CustomGridViewAdapter(this, gridViewString, imageResId, exercisesDone);
            gridView = FindViewById<GridView>(Resource.Id.grid_view_image_text);
            gridView.Adapter = adapter;
            gridView.ItemClick += (s, e) =>
            {
                Intent intent = new Intent(this, typeof(ExerciseActivity));
                intent.PutExtra("image_id", imageResId[e.Position].ToString());
                intent.PutExtra("exercise_id", exerciseIds[e.Position].ToString());
                StartActivity(intent);
            };


        }
    }
}