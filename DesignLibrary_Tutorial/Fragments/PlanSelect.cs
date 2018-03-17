using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using ScheduleApp.Handler;
using ScheduleApp.Helpers;
using Android.Support.V7.Widget;
using Android.Content;
using System.Threading.Tasks;
using Helper.Header;
using Android.App;
using Newtonsoft.Json;
using Android.Support.Design.Widget;
//using RecyclerViewAnimators.Animators;
//using Android.Views.Animations;

namespace ScheduleApp.Fragments
{
    public class PlanSelect : Android.Support.V4.App.Fragment
    {
        RecyclerView mRecyclerView;
        public RecyclerViewAdapter mRecyclerViewAdapter;
        SwipeRefreshLayout mSwipeRefresh;
        LinearLayout mLinearLayout, mProgLayout;
        TextView mTextMid;
        List<Card> mList;
        DateTime mLastUpdate;
        Spinner mSpinner;
        int mClassIndex = -1;
        bool updateIsRunning = true;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.menu3, container, false);
        }

        public override void OnStart()
        {
            base.OnStart();
            new InnerDataLoader(this).Execute();
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            this.Activity.Title = "Alle Pläne";
            mLinearLayout = Activity.FindViewById<LinearLayout>(Resource.Id.LinLayout);
            mProgLayout = Activity.FindViewById<LinearLayout>(Resource.Id.ProgLayout);
            mRecyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.RecyclerView);
            mRecyclerView.SetLayoutManager(new LinearLayoutManager(view.Context));
            mSwipeRefresh = Activity.FindViewById<SwipeRefreshLayout>(Resource.Id.SwipeRefresh);
            mSwipeRefresh.Refresh += MSwipeRefresh_Refresh;
            mTextMid = Activity.FindViewById<TextView>(Resource.Id.TextMid);
            mSpinner = Activity.FindViewById<Spinner>(Resource.Id.spinner);
            if (mSpinner != null)
            {
                mClassIndex = Application.Context.GetSharedPreferences("PlanSelect", FileCreationMode.Private).GetInt("ClassIndex", -1);
                mSpinner.SetSelection(mClassIndex + 1);
                mSpinner.ItemSelected += MSpinner_ItemSelected;
            }
            mSwipeRefresh.SetColorSchemeResources(Resource.Color.accent_color);
            if (DataHandler.GetDarkThemePref(Activity))
            {
                mSwipeRefresh.SetProgressBackgroundColorSchemeResource(Resource.Color.dark_spinner_bgd);
            }
        }

        private void MSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (!updateIsRunning && mClassIndex != e.Position - 1)
            {
                mClassIndex = e.Position - 1;
                mSpinner.Enabled = false;
                new InnerDataLoader(this, mClassIndex).Execute();
            }
        }

        private void MSwipeRefresh_Refresh(object sender, EventArgs e)
        {
            if (mClassIndex >= 0)
            {
                UpdateList();
            }
        }

        private async void UpdateList()
        {
            bool success = await UpdateAsync();
            if (success)
            {
                mRecyclerViewAdapter.mList = mList;
                mRecyclerViewAdapter.SortOutData();
                UpdateView();
            }
            else
                ShowConnectionError();
            mSwipeRefresh.Refreshing = false;
        }

        private RecyclerViewAdapter GetRecyclerAdapter()
        {
            bool potraitModeActive = Activity.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait;
            return new RecyclerViewAdapter(mList, Helpers.Type.ALL, DataHandler.GetDarkThemePref(Activity), potraitModeActive);
        }

        public Task<bool> UpdateAsync()
        {
            return Task.Factory.StartNew(() => Update());
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return base.OnOptionsItemSelected(item);
        }

        public bool Update()
        {
            Week[] week = new Week[2];
            var mDataHandler = DataHandler.GetDataHandler();
            week[0] = mDataHandler.GetDetailedWeek(mClassIndex, 0);
            week[1] = mDataHandler.GetDetailedWeek(mClassIndex, 1);
            if ((week[0] != null || week[1] != null)) //to avoid resetting data if there is no internet connection
            {
                mList = MessageHandler.GetCardList(week);
                mLastUpdate = System.DateTime.UtcNow;
                SaveList();
                return true;
            }
            return false;
        }

        private class InnerDataLoader : AsyncTask
        {
            PlanSelect menu;
            bool firstBuild;

            public InnerDataLoader(PlanSelect menu1, int classIndex = -2)
            {
                menu = menu1;
                firstBuild = classIndex == -2;
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                if (firstBuild)
                {
                    menu.LoadLastList();
                    menu.updateIsRunning = menu.mClassIndex > -1;
                    menu.mRecyclerViewAdapter = menu.GetRecyclerAdapter();
                    menu.Activity.RunOnUiThread(() => menu.mRecyclerView.SetAdapter(menu.mRecyclerViewAdapter));
                }
                else
                {
                    if (menu.mClassIndex > -1)
                    {
                        var updated = menu.Update();
                        if (!updated)
                        {
                            if (menu.mList != null) menu.mList.Clear();
                            menu.Activity.RunOnUiThread(() => menu.ShowConnectionError());
                        }
                        menu.mRecyclerViewAdapter.mList = menu.mList;
                    }
                    else if (menu.mRecyclerViewAdapter.mList != null && menu.mList != null)
                    {
                        menu.mRecyclerViewAdapter.mList.Clear();
                        menu.mList.Clear();
                        menu.SaveList();
                    }

                }
                menu.mRecyclerView.SetItemAnimator(new DefaultItemAnimator());
                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                menu.UpdateView();
                menu.updateIsRunning = false;
                if (firstBuild && menu.mClassIndex > -1)
                {
                    var time = System.DateTime.UtcNow - menu.mLastUpdate;
                    var msg = "Zuletzt aktualisiert vor ";
                    switch (time.Days)
                    {
                        case 0:
                            if (time.TotalMinutes < 5)
                                return;
                            if (time.TotalMinutes > 90)
                                msg += time.Hours.ToString() + " Stunden";
                            else
                                msg += time.Minutes.ToString() + " min";
                            break;
                        case 1:
                            msg = "Zuletzt aktualisiert vor einem Tag";
                            break;
                        default:
                            msg += time.Days.ToString() + " Tagen";
                            break;
                    }
                    var snackbar = Snackbar.Make(menu.View, msg, Snackbar.LengthShort);
                    //snackbar.View.SetBackgroundColor(Android.Graphics.Color.Argb(200, 103, 58, 183));
                    snackbar.Show();
                    //Toast.MakeText(menu.Activity, msg, ToastLength.Short).Show();
                }
            }

            protected override void OnPreExecute()
            {
                base.OnPreExecute();
                menu.mSwipeRefresh.Enabled = false;
                menu.mProgLayout.Visibility = ViewStates.Visible;
                menu.mLinearLayout.Visibility = ViewStates.Gone;
            }
        }

        private void UpdateView()
        {
            mProgLayout.Visibility = ViewStates.Gone;
            if (mRecyclerViewAdapter.mList == null || mRecyclerViewAdapter.mList.Count == 0)
            {
                if (mClassIndex == -1)
                {
                    mTextMid.Text = Activity.GetString(Resource.String.planselect_no_class_selected);
                }
                else
                {
                    mTextMid.Text = Activity.GetString(Resource.String.empty_dashboard);
                    mSwipeRefresh.Enabled = true;
                }
                mLinearLayout.Visibility = ViewStates.Visible;
                mRecyclerView.Visibility = ViewStates.Gone;
            }
            else
            {
                mLinearLayout.Visibility = ViewStates.Gone;
                mRecyclerView.Visibility = ViewStates.Visible;
                int itemCount = mRecyclerViewAdapter.ItemCount;
                //mRecyclerViewAdapter.mList = new List<Card>();
                //mRecyclerViewAdapter.NotifyItemRangeRemoved(0, itemCount + 2);
                //mRecyclerViewAdapter.mList = mList;
                //mRecyclerViewAdapter.NotifyItemRangeInserted(0, mList.Count - 1);
                mRecyclerViewAdapter.NotifyDataSetChanged();
                //mRecyclerViewAdapter.NotifyItemRangeRemoved(0, mRecyclerViewAdapter.mList.Count - 1);
                mSwipeRefresh.Enabled = true;
                //mRecyclerView.SetAdapter(mRecyclerViewAdapter);
                //mRecyclerViewAdapter.NotifyDataSetChanged();
            }
            if (mSpinner != null) mSpinner.Enabled = true;
        }

        private void LoadLastList()
        {
            try
            {
                var sharedPref = Application.Context.GetSharedPreferences("PlanSelect", FileCreationMode.Private);
                string source = sharedPref.GetString("CardList", string.Empty);
                var date = sharedPref.GetString("LastUpdate", string.Empty);
                //mClassIndex = sharedPref.GetInt("ClassIndex", -1);
                if (date.Length > 5) mLastUpdate = JsonConvert.DeserializeObject<DateTime>(date);
                mList = JsonConvert.DeserializeObject<List<Card>>(source);
            }
            catch (Exception) { }
        }

        private void ShowConnectionError()
        {
            var snackbar = Snackbar.Make(View, Activity.GetString(Resource.String.toast_no_internet_connection), Snackbar.LengthIndefinite); //.SetAction("OK", (v) => { })
            snackbar.SetAction("OK", (v) => { });
            //snackbar.View.SetBackgroundColor(Android.Graphics.Color.Argb(200, 103, 58, 183));
            snackbar.Show();
            //Toast.MakeText(Activity, Activity.GetString(Resource.String.toast_no_internet_connection), ToastLength.Short).Show();
        }

        private void SaveList()
        {
            var editor = Application.Context.GetSharedPreferences("PlanSelect", FileCreationMode.Private).Edit();
            if (mList != null && mList.Count > 0)
            {
                editor.PutString("CardList", JsonConvert.SerializeObject(mList));
                editor.PutString("LastUpdate", JsonConvert.SerializeObject(mLastUpdate));
                editor.PutInt("ClassIndex", mClassIndex);
            }
            else
            {
                editor.Remove("CardList");
                editor.Remove("LastUpdate");
                editor.Remove("ClassIndex");
            }
            editor.Apply();
        }
    }
}