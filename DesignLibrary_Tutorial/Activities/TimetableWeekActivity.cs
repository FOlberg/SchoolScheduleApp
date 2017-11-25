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
using Helper.Header;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ScheduleApp.Activities
{
    [Activity(Label = "Stundenplan")]
    public class TimetableWeekActivity : AppCompatActivity
    {
        TabLayout mTabs;
        TabAdapter mAdapter;
        FloatingActionButton mFab;
        ViewPager viewPager;

        static int countDay = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (DataHandler.GetDarkThemePref(this))
                SetTheme(Resource.Style.Theme_DarkTheme);
            else
                SetTheme(Resource.Style.Theme_DesignDemo);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Activity_Week);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBarW);
            mFab = FindViewById<FloatingActionButton>(Resource.Id.fabW);
            mTabs = FindViewById<TabLayout>(Resource.Id.tabLayoutW);
            SetSupportActionBar(toolBar);

            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            ab.SetHomeButtonEnabled(true);

            viewPager = FindViewById<ViewPager>(Resource.Id.viewPagerW);
            SetUpViewPager(viewPager);
            mTabs.SetupWithViewPager(viewPager);

            viewPager.PageSelected += ViewPager_PageSelected;
            mFab.Click += Fab_Click;
        }

        private void ViewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            if (e.Position == 4)
            {
                mFab.SetImageDrawable(GetDrawable(Resource.Drawable.ic_done));
            }
            else
            {
                mFab.SetImageDrawable(GetDrawable(Resource.Drawable.ic_arrow_forward));
            }
        }

        public static int GetDay()
        {
            return countDay++ % 5;
        }

        private void Fab_Click(object sender, System.EventArgs e)
        {
            if (mTabs.SelectedTabPosition < 4)
            {
                //Wrong doesnt change the view!
                mTabs.GetTabAt(mTabs.SelectedTabPosition + 1).Select();
            }
            else
            {
                FinishSetup();
                //FinishSetup();
            }

        }

        private void FinishSetup()
        {
            //Get Data from Children Fragments via SharedPreferences -> Delete Fragments -> Destructor passes Data -> Data will be gathered by:
            ISharedPreferences preferences = Application.Context.GetSharedPreferences("TableSetup", FileCreationMode.Private);
            ISharedPreferencesEditor editor = preferences.Edit();
            int[][] tempSel = new int[5][];
            string s;
            int classIndex = preferences.GetInt("classIndex", -1);

            //Destroy Fragments
            for (int i = mAdapter.Fragments.Count - 1; i >= 0; i--)
            {
                mAdapter.Fragments[i].OnDestroy();
            }

            for (int i = 0; i < 5; i++)
            {
                s = preferences.GetString("table" + i, string.Empty);
                if (s != string.Empty)
                {
                    tempSel[i] = JsonConvert.DeserializeObject<int[]>(s);
                }
            }
            //Delete Preference TableSetup
            editor.Clear();
            editor.Apply();

            preferences = Application.GetSharedPreferences("Config", FileCreationMode.Private);
            editor = preferences.Edit();
            editor.PutBoolean("Changed", true);
            editor.Apply();

            //Update mDataHandler
            var data = DataHandler.GetDataHandler();
            var config = DataHandler.GetConfig();
            config.AddTableConf(data.GetClassName(classIndex), tempSel);
            DataHandler.SaveConfig(config);

            SetResult(Result.Ok);
            Finish();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                countDay = 0;
                SetResult(Result.Canceled);
                Finish();
            }
            return base.OnOptionsItemSelected(item);
        }


        private void SetUpViewPager(ViewPager viewPager)
        {
            //mDataHandler = DataHandler.GetDataHandler();
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
                Fragments = new List<SupportFragment>();
                FragmentNames = new List<string>();
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