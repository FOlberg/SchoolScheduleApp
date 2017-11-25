using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Helper.Header;
using ScheduleApp.Helpers;

namespace ScheduleApp.Helpers
{
    public class ExpandableListViewAdapter : BaseExpandableListAdapter
    {
        private Context mContext;
        public List<Parent> mParentList { get; set; }
        public View.IOnClickListener mListener;
        public int mImageViewId { get; }

        public ExpandableListViewAdapter(Context context, List<Parent> ParentList)
        {
            mContext = context;
            mParentList = ParentList;
        }

        public override int GroupCount
        {
            get
            {
                return mParentList.Count;
            }
        }

        public override bool HasStableIds
        {
            get
            {
                return false;
            }
        }

        public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
        {
            return mParentList[groupPosition].mChildren[childPosition].Item1;
        }

        public override long GetChildId(int groupPosition, int childPosition)
        {
            return childPosition;
        }

        public override int GetChildrenCount(int groupPosition)
        {
            return mParentList[groupPosition].mChildren.Length;
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                LayoutInflater inflater = (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService);
                convertView = inflater.Inflate(Resource.Layout.item_layout, null);
            }
            TextView txtRoom = convertView.FindViewById<TextView>(Resource.Id.itemRoom);
            TextView txtSub = convertView.FindViewById<TextView>(Resource.Id.itemSub);
            txtRoom.Text = mParentList[groupPosition].mChildren[childPosition].Item1;
            txtSub.Text = mParentList[groupPosition].mChildren[childPosition].Item2;
            return convertView;
        }

        public override Java.Lang.Object GetGroup(int groupPosition)
        {
            return mParentList[groupPosition].mHour;
        }

        public override long GetGroupId(int groupPosition)
        {
            return groupPosition;
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                LayoutInflater inflater = (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService);
                convertView = inflater.Inflate(Resource.Layout.group_item, null);
            }
            Parent tParent = mParentList[groupPosition];
            TextView txtIndex = convertView.FindViewById<TextView>(Resource.Id.Index);
            TextView txtRoom = convertView.FindViewById<TextView>(Resource.Id.groupRoom);
            TextView txtClass = convertView.FindViewById<TextView>(Resource.Id.groupClass);
            ImageView indicatorIcon = convertView.FindViewById<ImageView>(Resource.Id.imageView1);
            RelativeLayout relativeLayout = convertView.FindViewById<RelativeLayout>(Resource.Id.groupItemLayout);
            if (isExpanded && mParentList[groupPosition].mEnabled)
                indicatorIcon.SetImageResource(Resource.Drawable.chevron_up);
            else
                indicatorIcon.SetImageResource(Resource.Drawable.chevron_down);
            txtIndex.Text = TimeHandler.HourName[groupPosition];
            if (tParent.mSelected != -1)
            {
                txtRoom.Text = tParent.mChildren[tParent.mSelected].Item1;
                txtClass.Text = tParent.mChildren[tParent.mSelected].Item2;
                txtClass.Visibility = ViewStates.Visible;
                txtRoom.Visibility = ViewStates.Visible;
            }
            else
            {
                txtClass.Visibility = ViewStates.Invisible;
                txtRoom.Visibility = ViewStates.Invisible;
            }
            //TestSpace

            if(mParentList[groupPosition].mEditMode)
            {
                indicatorIcon.SetImageResource(Resource.Drawable.ic_close);
                indicatorIcon.Clickable = true;
                indicatorIcon.Focusable = true;
                indicatorIcon.SetTag(Resource.Id.TAG_IMGVIEW_ID, groupPosition);
                indicatorIcon.Click += IndicatorIcon_Click;
            }
            else
            {
                indicatorIcon.Clickable = false;
                indicatorIcon.Focusable = false;
            }     

            return convertView;
        }

        private void IndicatorIcon_Click(object sender, EventArgs e)
        {
            int groupPosition = 0;
            try
            {
                groupPosition = (int)((ImageView)sender).GetTag(Resource.Id.TAG_IMGVIEW_ID);

            }
            catch(System.Exception)
            {
                //Add Exception to logger
            }
            mParentList[groupPosition].mSelected = -1;
            mParentList[groupPosition].mEditMode = false;
            //-> Animation
            NotifyDataSetChanged();
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return true;
        }

        public int[] GetSelectedItems()
        {
            int[] selected = new int[mParentList.Count];
            for (int i = 0; i < mParentList.Count; i++)
            {
                selected[i] = mParentList[i].mSelected;
            }
            return selected;
        }
    }
}