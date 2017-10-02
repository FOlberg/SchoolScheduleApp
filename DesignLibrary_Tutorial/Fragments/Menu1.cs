using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Support.V7.Widget;
using DesignLibrary_Tutorial.Helpers;
using System.Threading;
using SwipeRefresh = Android.Support.V4.Widget.SwipeRefreshLayout;
using System;
using DesignLibrary_Tutorial.Handler;
using DesignLibrary_Tutorial.Background;
using Android.Content;
using Android.Widget;

namespace DesignLibrary_Tutorial.Fragments
{
    public class Menu1 : Android.Support.V4.App.Fragment
    {
        RecyclerView mRecyclerView;
        RecyclerViewAdapter mRecyclerViewAdapter;
        SwipeRefresh mSwipeRefresh;
        MessageHandler mMsgHandler;
        AlarmReceiver mAlarmReceiver;
        LinearLayout mLinearLayout;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            mMsgHandler = MessageHandler.GetMsgHandler();
            mMsgHandler.OnDataChanged += MUpdater_OnDataChanged;
            mAlarmReceiver = new AlarmReceiver();
            mAlarmReceiver.SetAlarm(Activity);
            Activity.RegisterReceiver(mAlarmReceiver, new IntentFilter());

            // Create your fragment here
            //Do not handle events here! like button click etc -> because OnCreate will be called before OnCreatedView
        }

        private void MUpdater_OnDataChanged(object sender, EventArgs e)
        {
            //Update view
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.menu1, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            Activity.Title = mMsgHandler.GetCurrentClass();
            mSwipeRefresh = Activity.FindViewById<SwipeRefresh>(Resource.Id.SwipeRefresh);
            mSwipeRefresh.Refresh += MSwipeRefresh_Refresh;

            mRecyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.RecyclerView);
            mRecyclerView.SetLayoutManager(new LinearLayoutManager(view.Context));

            mLinearLayout = Activity.FindViewById<LinearLayout>(Resource.Id.LinLayout);

            mRecyclerViewAdapter = FillAdapter();
            mRecyclerView.SetAdapter(mRecyclerViewAdapter);
            mRecyclerView.SetItemAnimator(new DefaultItemAnimator());
            if (mRecyclerViewAdapter.mList.Count == 0)
            {
                mLinearLayout.Visibility = ViewStates.Visible;
                mRecyclerView.Visibility = ViewStates.Gone;
            }
            else
            {
                mLinearLayout.Visibility = ViewStates.Gone;
                mRecyclerView.Visibility = ViewStates.Visible;
            }
        }

        private void MSwipeRefresh_Refresh(object sender, System.EventArgs e)
        {
            UpdateList();         
            mMsgHandler.mDataHandler.DeleteOutdatedDataAsync();
            //MessageHandler.SaveMsgHandler(mMsgHandler);
        }

        private async void UpdateList()
        {
            await mMsgHandler.UpdateAsync();
            mRecyclerViewAdapter.mList = mMsgHandler.mList;
            mRecyclerView.SwapAdapter(mRecyclerViewAdapter, false);
            if (mRecyclerViewAdapter.mList.Count == 0)
            {
                mLinearLayout.Visibility = ViewStates.Visible;
                mRecyclerView.Visibility = ViewStates.Gone;
            }
            else
            {
                mLinearLayout.Visibility = ViewStates.Gone;
                mRecyclerView.Visibility = ViewStates.Visible;
            }
            mSwipeRefresh.Refreshing = false;
        }

        private RecyclerViewAdapter FillAdapter()
        {
            return new RecyclerViewAdapter(mMsgHandler.mList);
        }
    }
}