using Android.App;
using Android.OS;
using Android.Views;
using Android.Support.V4.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using SupportFragmentManager = Android.Support.V4.App.FragmentManager;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.App;
using Android.Widget;
using System.Collections.Generic;
using Java.Lang;
using Android.Content;
using DesignLibrary_Tutorial.Fragments;
using AppTestProzesse.Header;
using Newtonsoft.Json;
using Android.Graphics.Drawables;

namespace DesignLibrary_Tutorial.Activities
{
    [Activity(Label = "Week", Theme = "@style/Theme.DesignDemo")]
    public class TimetableWeekActivity : AppCompatActivity
    {
        DataHandler mDataHandler;
        TabLayout mTabs;
        TabAdapter mAdapter;
        FloatingActionButton mFab;

        static int countDay = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetContentView(Resource.Layout.Activity_Week);

                SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBarW);
                SetSupportActionBar(toolBar);

                SupportActionBar ab = SupportActionBar;
                ab.SetDisplayHomeAsUpEnabled(true);
                ab.SetHomeButtonEnabled(true);

                mTabs = FindViewById<TabLayout>(Resource.Id.tabLayoutW);

                ViewPager viewPager = FindViewById<ViewPager>(Resource.Id.viewPagerW);

                SetUpViewPager(viewPager);
                viewPager.PageSelected += ViewPager_PageSelected;

                mTabs.SetupWithViewPager(viewPager);

                mFab = FindViewById<FloatingActionButton>(Resource.Id.fabW);

                mFab.Click += Fab_Click;
            }
            catch (Exception e)
            {
                var m = e.Message;
            }

        }

        private void ViewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            if (e.Position == 4)
            {
                //mFab.SetImageIcon(Icon.CreateWithResource(this, Resource.Drawable.ic_done));
                mFab.SetImageDrawable(GetDrawable(Resource.Drawable.ic_done));
            }
            else
            {
                mFab.SetImageDrawable(GetDrawable(Resource.Drawable.ic_arrow_forward));
            }
        }

        private void GetDataHandler()
        {
            string HandlerSource = Application.Context.GetSharedPreferences("DataHandler", FileCreationMode.Private).GetString("mData", null);
            if (HandlerSource != null)
            {
                mDataHandler = JsonConvert.DeserializeObject<DataHandler>(HandlerSource);
            }
            else throw new System.Exception("DataHandler NullException");
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
            //mAdapter.Fragments.Clear();
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

            //Update mDataHandler
            mDataHandler.mConfig.AddTableConf(mDataHandler.GetClassName(classIndex), tempSel);
            preferences = Application.Context.GetSharedPreferences("DataHandler", FileCreationMode.Private);
            editor = preferences.Edit();

            editor.PutString("mData", JsonConvert.SerializeObject(mDataHandler));
            editor.Apply();

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
            GetDataHandler();
            mAdapter = new TabAdapter(SupportFragmentManager);
            for (int i = 0; i < 5; i++)
            {
                mAdapter.AddFragment(new TabFragment(), ((Days)i).ToString());
            }
            viewPager.OffscreenPageLimit = 5;
            viewPager.Adapter = mAdapter;
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

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(FragmentNames[position]);
            }
        }
    }
}