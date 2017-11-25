using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using DesignLibrary_Tutorial.Helpers;
using AppTestProzesse.Header;
using Newtonsoft.Json;
using DesignLibrary_Tutorial.Activities;
using System.Threading;

namespace DesignLibrary_Tutorial.Fragments
{
    public class TabFragment : Android.Support.V4.App.Fragment
    {
        ExpandableListViewAdapter mAdapter;
        ExpandableListView mExpandableListView;
        ISharedPreferences preferences;
        ISharedPreferencesEditor editor;
        DataHandler mDataHandler;
        int classIndex, day, previousGroup;
        int[] selected = new int[11];

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            day = TimetableWeekActivity.GetDay();
            Array.Fill<int>(selected, -1);
            LoadSharedPreferences();
            //Do not handle events here! like button click etc -> because OnCreate will be called before OnCreatedView
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.Fragment_Tab, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            mExpandableListView = view.FindViewById<ExpandableListView>(Resource.Id.expandableListView);
            mExpandableListView.NestedScrollingEnabled = true;

            SetData(out mAdapter);
            mExpandableListView.SetAdapter(mAdapter);
            mExpandableListView.ChildClick += ExpandableListView_ChildClick;
            mExpandableListView.GroupExpand += MExpandableListView_GroupExpand;
            mExpandableListView.ItemLongClick += MExpandableListView_ItemLongClick;
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
                //Toast.MakeText(Activity, "Group: " + (e.Position - children), ToastLength.Long).Show();
            }
        }

        private int GetGroupPosition(int position)
        {
            int children = 0;
            for (int i = 0; i < position; i++)
            {
                if (mExpandableListView.IsGroupExpanded(i)) //If a Group is expanded
                {
                    children += mAdapter.GetChildrenCount(i); // Add them to range
                    //break; -> only if one group at time is expandable
                }
            }
            return position - children;
        }

        private void MExpandableListView_GroupExpand(object sender, ExpandableListView.GroupExpandEventArgs e)
        {
            if (e.GroupPosition != previousGroup)
                mExpandableListView.CollapseGroup(previousGroup);
            previousGroup = e.GroupPosition;
        }

        public override void OnDestroy()
        {
            preferences = Application.Context.GetSharedPreferences("TableSetup", FileCreationMode.Private);
            editor = preferences.Edit();
            editor.PutString("table" + day, JsonConvert.SerializeObject(mAdapter.GetSelectedItems()));
            editor.Apply();

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

            //slow down collapse animation or wait couple of millisec.
            mExpandableListView.CollapseGroup(e.GroupPosition);
        }

        private void SetData(out ExpandableListViewAdapter mAdapter)
        {
            List<Parent> list = new List<Parent>();
            Timetable table = mDataHandler.GetTimetable(classIndex);
            for (int h = 0; h < 11; h++)
            {
                list.Add(new Parent(table.list[day, h], selected[h])); //Add Selected Item
            }
            mAdapter = new ExpandableListViewAdapter(Activity, list);
        }

        private void LoadSharedPreferences()
        {
            mDataHandler = DataHandler.GetDataHandler();
            preferences = Application.Context.GetSharedPreferences("TableSetup", FileCreationMode.Private);
            editor = preferences.Edit();

            classIndex = preferences.GetInt("classIndex", -1);
            //loadCfg = preferences.GetBoolean("LoadConfig", false);
            var config = DataHandler.GetConfig();
            if (classIndex > -1 && config.GetClassName() == mDataHandler.GetClassName(classIndex))
            {
                selected = config.GetTableConf()[day];
            }
            else if (classIndex == -1)
            {
                for (int i = 0; i < mDataHandler.GetClasses().Length; i++)
                {
                    if (mDataHandler.GetClasses()[i] == config.GetClassName())
                    {
                        selected = config.GetTableConf()[day];
                        classIndex = i;
                        editor.PutInt("classIndex", i);
                        editor.Apply();
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < config.GetConfigCount(); i++)
                {
                    if (config.GetClassName(i) == mDataHandler.GetClasses()[classIndex])
                    {
                        selected = config.GetTableConf(i)[day];
                        break;
                    }
                }
            }
            Activity.Title = mDataHandler.GetClassName(classIndex);
        }
    }
}