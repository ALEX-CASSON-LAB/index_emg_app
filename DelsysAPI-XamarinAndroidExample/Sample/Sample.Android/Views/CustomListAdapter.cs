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
    class CustomListAdapter : BaseAdapter
    {
        private readonly Context c;
        private readonly JavaList<Exercise> exercises;
        private LayoutInflater inflater;

        /*
         * CONSTRUCTOR
         */
        public CustomListAdapter(Context c, JavaList<Exercise> exercises)
        {
            this.c = c;
            this.exercises = exercises;
        }

        public override Object GetItem(int position)
        {
            return exercises.Get(position);
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
                convertView = inflater.Inflate(Resource.Layout.stats_item, parent, false);
            }

            //BIND DATA
            CustomAdapterViewHolder holder = new CustomAdapterViewHolder(convertView)
            {
                NameTxt = { Text = exercises[position].name }
            };
           
            holder.Img.SetImageResource((int)typeof(Resource.Drawable).GetField(exercises[position].img_name).GetValue(null));
            holder.RepsTxt.Text = exercises[position].reps + " / " + exercises[position].reps + " reps";

            return convertView;
        }
        public override int Count
        {
            get { return exercises.Size(); }
        }
    }

    class CustomAdapterViewHolder : Java.Lang.Object
    {
        //adapter views to re-use
        public TextView NameTxt;
        public TextView RepsTxt;
        public ImageView Img;

        public CustomAdapterViewHolder(View itemView)
        {
            NameTxt = itemView.FindViewById<TextView>(Resource.Id.txv_name);
            RepsTxt = itemView.FindViewById<TextView>(Resource.Id.txv_reps);
            Img = itemView.FindViewById<ImageView>(Resource.Id.img_icon);

        }
    }
}