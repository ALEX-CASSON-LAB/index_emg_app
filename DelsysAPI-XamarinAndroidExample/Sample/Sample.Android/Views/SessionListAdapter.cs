//Taken and adapted from https://camposha.info/xamarin-android-listview-imagestext-and-itemclick/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidSample.Core;
using Object = Java.Lang.Object;

namespace AndroidSample.Views
{
    class SessionListAdapter : BaseAdapter
    {
        private MainModel _myModel;
        private readonly Context c;
        private readonly JavaList<Session> sessions;
        private LayoutInflater inflater;
        public SessionListAdapter(Context c, JavaList<Session> sessions)
        {
            this.c = c;
            this.sessions = sessions;

            _myModel = MainModel.Instance;
        }

        public override Object GetItem(int position)
        {
            return sessions.Get(position);
        }


        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
   
            if (inflater == null)
            {
                inflater = (LayoutInflater)c.GetSystemService(Context.LayoutInflaterService);
            }

            if (convertView == null)
            {
                convertView = inflater.Inflate(Resource.Layout.session_item, parent, false);
            }

            //BIND DATA
            SessionAdapterViewHolder holder = new SessionAdapterViewHolder(convertView)
            {
                DateTxt = { Text = sessions[position].date.ToString() }
            };
          
            holder.NotesTxt.Text = sessions[position].notes;

            JavaList<Exercise> exercises = getExercises(sessions[position]);
            //if (exercises != null)
            //    holder.ExerciseList.Adapter = new CustomListAdapter(this.c,exercises);
            if(exercises != null)
                holder.ExerTxt.Text = exercises.Count + " Completed";
           
                
            //TODO lead button to display of all data
            holder.MoreButton.Click += (s, o) => {
                Intent intent = new Intent(this.c, typeof(DisplayStatsActivity));
                intent.PutExtra("session_id", sessions[position].Id.ToString());
                this.c.StartActivity(intent);
            };

            return convertView;
        }
        public override int Count
        {
            get { return sessions.Size(); }
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
    }

    class SessionAdapterViewHolder : Java.Lang.Object
    {
        //adapter views to re-use
        public TextView DateTxt;
        public TextView NotesTxt;
        public TextView ExerTxt;
        public Button MoreButton;
        //public ListView ExerciseList;

        public SessionAdapterViewHolder(View itemView)
        {
            DateTxt = itemView.FindViewById<TextView>(Resource.Id.txv_date);
            NotesTxt = itemView.FindViewById<TextView>(Resource.Id.txv_note);
            ExerTxt = itemView.FindViewById<TextView>(Resource.Id.txv_exer);
            MoreButton = itemView.FindViewById<Button>(Resource.Id.btn_more);
            //ExerciseList = itemView.FindViewById<ListView>(Resource.Id.view_stats_list);

        }
    }

    
}