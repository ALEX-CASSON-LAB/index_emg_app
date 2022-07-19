using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Widget;
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
        int[] imageId;
        List<int> exercisesDone;

        public ExerciseSelectionActivity()
        {
            _myModel = MainModel.Instance;
            exerciseInfo = _myModel.getExerciseInfo();
            gridViewString = exerciseInfo[0].ToArray();
            gridViewImg = exerciseInfo[1].ToArray();
            convertImageLocation();
        }

        private void convertImageLocation()
        {          
            imageId = new int[exerciseInfo[1].Count];
            for (int i = 0; i < gridViewImg.Length;i++)
            {
                string imL = gridViewImg[i];
                var resourceId = (int)typeof(Resource.Drawable).GetField(imL).GetValue(null);
                imageId[i] = resourceId;
            }
        }
        
         int[] imageid=
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

            //TODO display exercises already done differently
            exercisesDone = _myModel.getExercisesDone();

            CustomGridViewAdapter adapter = new CustomGridViewAdapter(this, gridViewString, imageId, exercisesDone);
            gridView = FindViewById<GridView>(Resource.Id.grid_view_image_text);
            gridView.Adapter = adapter;
            gridView.ItemClick += (s, e) =>
            {
                Intent intent = new Intent(this, typeof(ExerciseActivity));
                intent.PutExtra("exercise_name", gridViewString[e.Position]);
                intent.PutExtra("exercise_id", imageId[e.Position].ToString());
                StartActivity(intent);
            };


        }
    }
}