using Android.OS;
using Android.Views;
using Android.Support.V7.Widget;
using DesignLibrary_Tutorial.Helpers;
using System.Threading;
using SwipeRefresh = Android.Support.V4.Widget.SwipeRefreshLayout;
using System;
using DesignLibrary_Tutorial.Handler;
using DesignLibrary_Tutorial.Background;
using Void = Java.Lang.Void;
using Android.Widget;
using Java.Lang;
using Android.Content;
using Android.App;
//using RecyclerViewAnimators.Animators;
//using Android.Views.Animations;

namespace DesignLibrary_Tutorial.Fragments
{
    public class Menu1 : Android.Support.V4.App.Fragment
    {
        protected RecyclerView mRecyclerView;
        public RecyclerViewAdapter mRecyclerViewAdapter;
        SwipeRefresh mSwipeRefresh;
        MessageHandler mMsgHandler;
        AlarmReceiver mAlarmReceiver;
        LinearLayout mLinearLayout, mProgLayout;
        //ProgressBar progressBar;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //progressBar = Activity.FindViewById<ProgressBar>(Resource.Id.loading_spinner);
            // Create your fragment here
            //Do not handle events here! like button click etc -> because OnCreate will be called before OnCreatedView
        }
        public override void OnStart()
        {
            base.OnStart();
            Activity.Title = "Klasse ";
            new BckTask(this).Execute();
            //mMsgHandler = MessageHandler.GetMsgHandler();
            //mMsgHandler.OnDataChanged += MUpdater_OnDataChanged;
            //mAlarmReceiver = new AlarmReceiver();
            //mAlarmReceiver.SetAlarm(Activity);
            //Activity.RegisterReceiver(mAlarmReceiver, new IntentFilter());
            //Activity.Title = "Klasse " + mMsgHandler.GetCurrentClass();
            //mRecyclerViewAdapter = FillAdapter();
            //mRecyclerView.SetAdapter(mRecyclerViewAdapter);
            ////var animator = new SlideInUpAnimator(new OvershootInterpolator(1f));
            //mRecyclerView.SetItemAnimator(new DefaultItemAnimator());
            //if (mRecyclerViewAdapter.mList.Count == 0)
            //{
            //    mLinearLayout.Visibility = ViewStates.Visible;
            //    mRecyclerView.Visibility = ViewStates.Gone;
            //}
            //else
            //{
            //    mLinearLayout.Visibility = ViewStates.Gone;
            //    mRecyclerView.Visibility = ViewStates.Visible;
            //}
        }

        private class BckTask : AsyncTask
        {
            Menu1 menu;

            public BckTask(Menu1 menu1)
            {
                menu = menu1;
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                menu.mMsgHandler = MessageHandler.GetMsgHandler();
                menu.mMsgHandler.OnDataChanged += menu.MUpdater_OnDataChanged;
                menu.mAlarmReceiver = new AlarmReceiver();
                menu.mAlarmReceiver.SetAlarm(menu.Activity);
                menu.Activity.RegisterReceiver(menu.mAlarmReceiver, new IntentFilter());
                if (menu.Activity.GetSharedPreferences("Config", FileCreationMode.Private).GetBoolean("Changed", false))
                {
                    menu.mMsgHandler.Update();
                    menu.Activity.GetSharedPreferences("Config", FileCreationMode.Private).Edit().PutBoolean("Changed", false).Apply();
                }
                menu.mRecyclerViewAdapter = menu.FillAdapter();
                //var animator = new SlideInUpAnimator(new OvershootInterpolator(1f));
                menu.mRecyclerView.SetItemAnimator(new DefaultItemAnimator());
                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);
                menu.Activity.Title = "Klasse " + menu.mMsgHandler.GetCurrentClass();
                menu.mProgLayout.Visibility = ViewStates.Gone;
                if (menu.mRecyclerViewAdapter.mList.Count == 0)
                {
                    menu.mLinearLayout.Visibility = ViewStates.Visible;
                    menu.mRecyclerView.Visibility = ViewStates.Gone;
                }
                else
                {
                    menu.mLinearLayout.Visibility = ViewStates.Gone;
                    menu.mRecyclerView.Visibility = ViewStates.Visible;
                    menu.mRecyclerView.SetAdapter(menu.mRecyclerViewAdapter);
                }
            }

            protected override void OnPreExecute()
            {
                base.OnPreExecute();
                menu.mProgLayout.Visibility = ViewStates.Visible;
            }
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
            //progressBar = Activity.FindViewById<ProgressBar>(Resource.Id.loading_spinner);
            mLinearLayout = Activity.FindViewById<LinearLayout>(Resource.Id.LinLayout);
            mProgLayout = Activity.FindViewById<LinearLayout>(Resource.Id.ProgLayout);
            mRecyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.RecyclerView);
            mRecyclerView.SetLayoutManager(new LinearLayoutManager(view.Context));
            mSwipeRefresh = Activity.FindViewById<SwipeRefresh>(Resource.Id.SwipeRefresh);
            mSwipeRefresh.Refresh += MSwipeRefresh_Refresh;
        }

        private void MSwipeRefresh_Refresh(object sender, System.EventArgs e)
        {
            UpdateList();         
            //mMsgHandler.DeleteOutdatedDataAsync();
            //MessageHandler.SaveMsgHandler(mMsgHandler);
        }

        private async void UpdateList()
        {
            bool success = await mMsgHandler.UpdateAsync();
            if (success)
            {
                mRecyclerViewAdapter.mList = mMsgHandler.mList;
                mRecyclerViewAdapter.SortOutData();
                mRecyclerView.SwapAdapter(mRecyclerViewAdapter, false);
                mProgLayout.Visibility = ViewStates.Gone;
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
            else
            {
                Toast.MakeText(Activity, "Keine Internetverbindung", ToastLength.Short).Show();
            }

            mSwipeRefresh.Refreshing = false;
        }

        private RecyclerViewAdapter FillAdapter()
        {
            return new RecyclerViewAdapter(mMsgHandler.mList);
        }
    }
}