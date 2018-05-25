using Android.App;
using Android.OS;
using Android.Views;
using SupportFragment = Android.Support.V4.App.Fragment;
using SupportFragmentManager = Android.Support.V4.App.FragmentManager;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.App;
using System.Collections.Generic;
using Android.Content;
using ScheduleApp.Fragments;
using ScheduleApp.Handler;
using Newtonsoft.Json;
using Android.Support.V4.Content;
using ScheduleApp.Objects;
using Android.Widget;

namespace ScheduleApp.Activities
{
    [Activity(Label = "@string/act_label_ttable_week")]
    public class TimetableWeekActivity : AppCompatActivity
    {
        static int mDayCounter = 0;

        TabLayout               mTabs;
        TabAdapter              mAdapter;
        ViewPager               mViewPager;
        FloatingActionButton    mFabBtn;
        ProgressBar             mProgressBar;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (DataHandler.GetDarkThemePref(this))
                SetTheme(Resource.Style.Theme_DarkTheme);
            else
                SetTheme(Resource.Style.Theme_Light);
            base.OnCreate(savedInstanceState);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                SetContentView(Resource.Layout.Activity_Week);
            else
                SetContentView(Resource.Layout.BL_Activity_Week);

            OverridePendingTransition(Resource.Animation.slide_from_right, Resource.Animation.slide_to_left);
            SupportToolbar toolBar  = FindViewById<SupportToolbar>(Resource.Id.toolBarW);
            mFabBtn                 = FindViewById<FloatingActionButton>(Resource.Id.fabW);
            mTabs                   = FindViewById<TabLayout>(Resource.Id.tabLayoutW);
            mViewPager              = FindViewById<ViewPager>(Resource.Id.viewPagerW);
            mProgressBar            = FindViewById<ProgressBar>(Resource.Id.progressbar);

            SetSupportActionBar(toolBar);

            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            ab.SetHomeButtonEnabled(true);
            
            SetUpViewPager(mViewPager);
            mTabs.SetupWithViewPager(mViewPager);

            Window.ClearFlags(WindowManagerFlags.NotTouchable);

            mViewPager.PageSelected += ViewPager_PageSelected;
            mFabBtn.Click           += Fab_Click;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.appbar_information, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        private void ViewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            if (e.Position == 4)
                mFabBtn.SetImageDrawable(ContextCompat.GetDrawable(this, Resource.Drawable.ic_done));
            else
                mFabBtn.SetImageDrawable(ContextCompat.GetDrawable(this, Resource.Drawable.ic_arrow_forward));
        }

        public static int GetDay()
        {
            return mDayCounter++ % 5;
        }

        private void Fab_Click(object sender, System.EventArgs e)
        {
            if (mTabs.SelectedTabPosition < 4)
                mTabs.GetTabAt(mTabs.SelectedTabPosition + 1).Select();
            else
                FinishSetup();
        }

        private void FinishSetup()
        {
            //Get Data from Children Fragments via SharedPreferences -> Delete Fragments -> Destructor passes Data -> Data will be gathered by:
            mProgressBar.Visibility = ViewStates.Visible;
            Window.SetFlags(WindowManagerFlags.NotTouchable, WindowManagerFlags.NotTouchable);

            ISharedPreferences preferences  = Application.Context.GetSharedPreferences("TableSetup", FileCreationMode.Private);
            ISharedPreferencesEditor editor = preferences.Edit();
            int[][] tempSel = new int[5][];
            int classIndex  = preferences.GetInt("classIndex", -1);

            //Destroy Fragments
            for (int i = mAdapter.Fragments.Count - 1; i >= 0; i--)
            {
                mAdapter.Fragments[i].OnDestroy();
            }

            string tableSource;
            for (int i = 0; i < 5; i++)
            {
                tableSource = preferences.GetString("table" + i, string.Empty);
                if (tableSource != string.Empty)
                    tempSel[i] = JsonConvert.DeserializeObject<int[]>(tableSource);
            }
            //Delete Preference TableSetup
            editor.Clear();
            editor.Apply();

            //Update mDataHandler
            var data        = DataHandler.GetDataHandler();
            var config      = DataHandler.GetConfig();
            var className   = data.GetClassName(classIndex);
            config.AddTableConf(className, tempSel);
            DataHandler.SaveConfig(config);

            preferences = Application.GetSharedPreferences("Config", FileCreationMode.Private);
            editor      = preferences.Edit();
            editor.PutString("className", className);
            editor.PutBoolean("Changed", true);
            editor.Apply();

            SetResult(Result.Ok);
            Finish();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    mDayCounter = 0;
                    SetResult(Result.Canceled);
                    Finish();
                    break;
                case Resource.Id.action_info:
                    ShowInformationDialog();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void ShowInformationDialog()
        {
            var builder = new Android.App.AlertDialog.Builder(this);
            var view = LayoutInflater.Inflate(Resource.Layout.alert_dialog_info, null);
            builder.SetView(view)
                .SetTitle("Hilfe")
                .SetPositiveButton("Ok", (o, ev) => { })
                .Create()
                .Show();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.slide_from_left, Resource.Animation.slide_to_right);
        }


        private void SetUpViewPager(ViewPager viewPager)
        {
            mAdapter = new TabAdapter(SupportFragmentManager);
            for (int i = 0; i < 5; i++)
            {
                mAdapter.AddFragment(new TabFragment(), ((Days)i).ToString());
            }
            viewPager.OffscreenPageLimit = 5;
            RunOnUiThread(() => viewPager.Adapter = mAdapter);
        }

        public class TabAdapter : FragmentPagerAdapter
        {
            public List<SupportFragment> Fragments { get; set; }
            public List<string> FragmentNames { get; set; }

            public TabAdapter(SupportFragmentManager sfm) : base(sfm)
            {
                Fragments       = new List<SupportFragment>();
                FragmentNames   = new List<string>();
            }

            public void AddFragment(SupportFragment fragment, string name)
            {
                Fragments.Add(fragment);
                FragmentNames.Add(name);
            }

            public override int Count
            {
                get
                {
                    return Fragments.Count;
                }
            }

            public override SupportFragment GetItem(int position)
            {
                return Fragments[position];
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(FragmentNames[position]);
            }
        }
    }
}