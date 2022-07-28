﻿using Android.App;
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
    public class CustomGridViewAdapter : BaseAdapter
    {
        private Context context;
        private string[] gridViewString;
        private int[] gridViewImage;
        private List<int> exercisesDone;

        public CustomGridViewAdapter(Context context, string[] gridViewstr, int[] gridViewImage, List<int> exercisesDone)
        {
            this.context = context;
            gridViewString = gridViewstr;
            this.gridViewImage = gridViewImage;
            this.exercisesDone = exercisesDone;
        }
        public override int Count
        {
            get
            {
                return gridViewString.Length;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view;
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            if(convertView == null)
            {
                view = new View(context);
                view = inflater.Inflate(Resource.Layout.content_exerciseSelection, null);
                TextView txtView = view.FindViewById<TextView>(Resource.Id.textView);
                ImageView imgView = view.FindViewById<ImageView>(Resource.Id.imageView);
                ImageView imgCheckmark = view.FindViewById<ImageView>(Resource.Id.img_checkbox);
                txtView.Text = gridViewString[position];
                imgView.SetImageResource(gridViewImage[position]);

                if (exercisesDone.Contains(position)){
                    imgCheckmark.Visibility = ViewStates.Visible;
                }
            }
            else
            {
                view = (View)convertView;
            }
            return view;

        }
    }
}