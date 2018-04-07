using Android.OS;
using Android.Views;
using Android.Support.V7.Widget;
using ScheduleApp.Helpers;
using SwipeRefresh = Android.Support.V4.Widget.SwipeRefreshLayout;
using System;
using ScheduleApp.Handler;
using ScheduleApp.Background;
using Android.Widget;
using Android.Content;
using Android.Support.Design.Widget;
//using RecyclerViewAnimators.Animators;
//using Android.Views.Animations;

namespace ScheduleApp.Fragments
{
    public class Dashboard : Android.Support.V4.App.Fragment
    {
        public RecyclerViewAdapter mRecyclerViewAdapter;

        RecyclerView    mRecyclerView;
        SwipeRefresh    mSwipeRefresh;
        MessageHandler  mMsgHandler;
        AlarmReceiver   mAlarmReceiver;
        LinearLayout    mLinearLayout, mProgLayout;

        public override void OnStart()
        {
            base.OnStart();
            Activity.Title = "Klasse " + Activity.GetSharedPreferences("Config", FileCreationMode.Private).GetString("className", "");
            new BckTask(this).Execute();
        }

        private class BckTask : AsyncTask
        {
            Dashboard menu;

            public BckTask(Dashboard menu1)
            {
                menu = menu1;
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                menu.mMsgHandler    = MessageHandler.GetMsgHandler();
                menu.mAlarmReceiver = new AlarmReceiver();

                menu.mMsgHandler.OnDataChanged += menu.MUpdater_OnDataChanged;
                menu.mAlarmReceiver.SetUpAlarmService(menu.Activity);
                menu.Activity.RegisterReceiver(menu.mAlarmReceiver, new IntentFilter());

                if (menu.Activity.GetSharedPreferences("Config", FileCreationMode.Private).GetBoolean("Changed", false))
                {
                    menu.mMsgHandler.Update();
                    menu.Activity.GetSharedPreferences("Config", FileCreationMode.Private).Edit().PutBoolean("Changed", false).Apply();
                }

                //var animator = new SlideInUpAnimator(new OvershootInterpolator(1f));
                menu.mRecyclerViewAdapter = menu.FillAdapter();
                menu.mRecyclerView.SetItemAnimator(new DefaultItemAnimator());
                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);
                menu.Activity.Title = "Klasse " + menu.mMsgHandler.GetCurrentClass();
                menu.mProgLayout.Visibility = ViewStates.Gone;

                if (menu.mRecyclerViewAdapter.mList == null || menu.mRecyclerViewAdapter.mList.Count == 0)
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
            mLinearLayout   = Activity.FindViewById<LinearLayout>(Resource.Id.LinLayout);
            mProgLayout     = Activity.FindViewById<LinearLayout>(Resource.Id.ProgLayout);
            mRecyclerView   = Activity.FindViewById<RecyclerView>(Resource.Id.RecyclerView);
            mSwipeRefresh   = Activity.FindViewById<SwipeRefresh>(Resource.Id.SwipeRefresh);
            mRecyclerView.SetLayoutManager(new LinearLayoutManager(view.Context));
            mSwipeRefresh.SetColorSchemeResources(Resource.Color.accent_color);
            if (DataHandler.GetDarkThemePref(Activity))
                mSwipeRefresh.SetProgressBackgroundColorSchemeResource(Resource.Color.dark_spinner_bgd);
            mSwipeRefresh.Refresh += MSwipeRefresh_Refresh;
        }

        private void MSwipeRefresh_Refresh(object sender, System.EventArgs e)
        {
            UpdateList();
        }

        private async void UpdateList()
        {
            bool success = await mMsgHandler.UpdateAsync();
            if (success)
            {
                mRecyclerViewAdapter.mList  = mMsgHandler.mList;
                mProgLayout.Visibility      = ViewStates.Gone;
                mRecyclerViewAdapter.SortOutData();
                mRecyclerView.SwapAdapter(mRecyclerViewAdapter, false);

                if (mRecyclerViewAdapter.mList == null || mRecyclerViewAdapter.mList.Count == 0)
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
                var snackbar = Snackbar.Make(View, Activity.GetString(Resource.String.toast_no_internet_connection), Snackbar.LengthIndefinite);
                snackbar.SetAction("OK", (v) => { });
                snackbar.Show();
            }

            mSwipeRefresh.Refreshing = false;
        }

        private RecyclerViewAdapter FillAdapter()
        {
            bool potraitModeActive = Activity.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait;
            return new RecyclerViewAdapter(mMsgHandler.mList, Helpers.Type.USER, DataHandler.GetDarkThemePref(Activity), potraitModeActive);
        }
    }
}