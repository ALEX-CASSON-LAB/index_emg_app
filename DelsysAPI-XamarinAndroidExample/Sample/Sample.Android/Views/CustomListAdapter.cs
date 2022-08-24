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
        private List<int> stats;
        private LayoutInflater inflater;

        /*
         * CONSTRUCTOR
         */
        public CustomListAdapter(Context c, JavaList<Exercise> exercises, List<int> stats)
        {
            this.c = c;
            this.exercises = exercises;
            this.stats = stats;
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
           
            if (stats[position] == 5)
                holder.Img.SetImageResource(Resource.Drawable.stars_five);
            else if(stats[position] == 4)
                holder.Img.SetImageResource(Resource.Drawable.stars_four);
            else if (stats[position] == 3)
                holder.Img.SetImageResource(Resource.Drawable.stars_three);
            else if (stats[position] == 2)
                holder.Img.SetImageResource(Resource.Drawable.stars_two);
            else if (stats[position] == 1)
                holder.Img.SetImageResource(Resource.Drawable.stars_one);

            holder.RepsTxt.Text = stats[position] + "/5 stars";

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