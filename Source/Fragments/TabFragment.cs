using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using ScheduleApp.Helpers;
using ScheduleApp.Handler;
using Newtonsoft.Json;
using ScheduleApp.Activities;
using ScheduleApp.Objects;

namespace ScheduleApp.Fragments
{
    public class TabFragment : Android.Support.V4.App.Fragment
    {
        ExpandableListViewAdapter   mAdapter;
        ExpandableListView          mExpandableListView;
        ISharedPreferences          mPreferences;
        ISharedPreferencesEditor    mEditor;
        DataHandler                 mDataHandler;
        int mClassIndex, mDay, mPreviousGroup;
        int[] selected = new int[11];

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            mDay = TimetableWeekActivity.GetDay();
            Array.Fill<int>(selected, -1);
            LoadSharedPreferences();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                return inflater.Inflate(Resource.Layout.Fragment_Tab, container, false);
            else
                return inflater.Inflate(Resource.Layout.BL_Fragment_Tab, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            mExpandableListView = view.FindViewById<ExpandableListView>(Resource.Id.expandableListView);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                mExpandableListView.NestedScrollingEnabled = true;

            SetData(out mAdapter);
            mExpandableListView.SetAdapter(mAdapter);
            mExpandableListView.ChildClick      += ExpandableListView_ChildClick;
            mExpandableListView.GroupExpand     += MExpandableListView_GroupExpand;
            mExpandableListView.ItemLongClick   += MExpandableListView_ItemLongClick;
        }

        private void MExpandableListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            if (mExpandableListView.GetItemAtPosition(e.Position).GetType() == typeof(Java.Lang.Integer)) //If clicked Item equals type of group
            {
                int groupPosition = GetGroupPosition(e.Position);
                if (mAdapter.mParentList[groupPosition].mSelected != -1)
                {
                    mAdapter.mParentList[groupPosition].mEditMode = true;
                    mAdapter.NotifyDataSetChanged();
                }
            }
        }

        private int GetGroupPosition(int position)
        {
            int children = 0;
            for (int i = 0; i < position; i++)
            {
                if (mExpandableListView.IsGroupExpanded(i)) //If a Group is expanded
                    children += mAdapter.GetChildrenCount(i); // Add them to range
            }
            return position - children;
        }

        private void MExpandableListView_GroupExpand(object sender, ExpandableListView.GroupExpandEventArgs e)
        {
            if (e.GroupPosition != mPreviousGroup)
                mExpandableListView.CollapseGroup(mPreviousGroup);
            mPreviousGroup = e.GroupPosition;
        }

        public override void OnDestroy()
        {
            mPreferences    = Application.Context.GetSharedPreferences("TableSetup", FileCreationMode.Private);
            mEditor         = mPreferences.Edit();
            mEditor.PutString("table" + mDay, JsonConvert.SerializeObject(mAdapter.GetSelectedItems()));
            mEditor.Apply();
            base.OnDestroy();
        }

        private void ExpandableListView_ChildClick(object sender, ExpandableListView.ChildClickEventArgs e)
        {
            mAdapter.mParentList[e.GroupPosition].mSelected = e.ChildPosition;
            selected[e.GroupPosition] = e.ChildPosition;

            if (e.GroupPosition < 10 && mAdapter.mParentList[e.GroupPosition + 1].mChildren.Length > e.ChildPosition
                && mAdapter.mParentList[e.GroupPosition].mChildren[e.ChildPosition].Item1 == mAdapter.mParentList[e.GroupPosition + 1].mChildren[e.ChildPosition].Item1
                && mAdapter.mParentList[e.GroupPosition].mChildren[e.ChildPosition].Item2 == mAdapter.mParentList[e.GroupPosition + 1].mChildren[e.ChildPosition].Item2)
            {
                selected[e.GroupPosition + 1] = e.ChildPosition;
                mAdapter.mParentList[e.GroupPosition + 1].mSelected = e.ChildPosition;
            }
            else if (e.GroupPosition > 0 && mAdapter.mParentList[e.GroupPosition - 1].mChildren.Length > e.ChildPosition
                && mAdapter.mParentList[e.GroupPosition].mChildren[e.ChildPosition].Item1 == mAdapter.mParentList[e.GroupPosition - 1].mChildren[e.ChildPosition].Item1
                && mAdapter.mParentList[e.GroupPosition].mChildren[e.ChildPosition].Item2 == mAdapter.mParentList[e.GroupPosition - 1].mChildren[e.ChildPosition].Item2)
            {
                selected[e.GroupPosition - 1] = e.ChildPosition;
                mAdapter.mParentList[e.GroupPosition - 1].mSelected = e.ChildPosition;
            }

            mExpandableListView.CollapseGroup(e.GroupPosition);
        }

        private void SetData(out ExpandableListViewAdapter mAdapter)
        {
            List<Parent> list = new List<Parent>();
            Timetable table = mDataHandler.GetTimetable(mClassIndex);
            for (int h = 0; h < 11; h++)
            {
                list.Add(new Parent(table.mSubNames[mDay, h], selected[h])); //Add Selected Item
            }
            mAdapter = new ExpandableListViewAdapter(Activity, list);
        }

        private void LoadSharedPreferences()
        {
            mDataHandler    = DataHandler.GetDataHandler();
            mPreferences    = Application.Context.GetSharedPreferences("TableSetup", FileCreationMode.Private);
            mEditor         = mPreferences.Edit();
            mClassIndex     = mPreferences.GetInt("classIndex", -1);
            var config      = DataHandler.GetConfig();

            if (mClassIndex > -1 && config.GetClassName() == mDataHandler.GetClassName(mClassIndex))
                selected = config.GetTableConf()[mDay];

            else if (mClassIndex == -1)
            {
                FindClassIndex(ref config);
                if (mClassIndex == -1 && (config.GetClassName() == null || config.mConfigSel > 0))
                {
                    config.RemoveCurrentConfig();
                    FindClassIndex(ref config);
                }
            }
            else
            {
                for (int i = 0; i < config.GetConfigCount(); i++)
                {
                    if (config.GetClassName(i) == mDataHandler.GetClasses()[mClassIndex])
                    {
                        selected = config.GetTableConf(i)[mDay];
                        break;
                    }
                }
            }
            Activity.Title = mDataHandler.GetClassName(mClassIndex);
        }

        private void FindClassIndex(ref Config config)
        {
            for (int i = 0; i < mDataHandler.GetClasses().Length; i++)
            {
                if (mDataHandler.GetClasses()[i] == config.GetClassName())
                {
                    selected = config.GetTableConf()[mDay];
                    mClassIndex = i;
                    mEditor.PutInt("classIndex", i);
                    mEditor.Apply();
                    break;
                }
            }
        }
    }
}