using Android.App;
using Android.OS;
using Android.Views;
using Android.Content;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.App;
using Android.Widget;
using System.Collections.Generic;
using ScheduleApp.Handler;
using Android.Runtime;
using System.Threading;

namespace ScheduleApp.Activities
{
    [Activity(Label = "@string/act_label_ttable_setup")]
    public class TimetableSetupActivity : AppCompatActivity
    {
        private List<string>        mItems;
        private ListView            mListView;
        public DataHandler          mDataHandler;
        ISharedPreferences          mPreference;
        ISharedPreferencesEditor    mEditor;
        ProgressBar                 mProgressBar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (DataHandler.GetDarkThemePref(this))
                SetTheme(Resource.Style.Theme_DarkTheme);
            else
                SetTheme(Resource.Style.Theme_Light);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Activity_Setup);
            OverridePendingTransition(Resource.Animation.slide_from_right, Resource.Animation.slide_to_left);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBarT);
            SetSupportActionBar(toolBar);

            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            ab.SetHomeButtonEnabled(true);

            mListView       = FindViewById<ListView>(Resource.Id.listView);
            mProgressBar    = FindViewById<ProgressBar>(Resource.Id.progressbar);

            //Loading mData
            mDataHandler = DataHandler.GetDataHandler();

            //Adding Items (classes) to the displayed list
            mItems = new List<string>();
            foreach (var s in mDataHandler.GetClasses())
            {
                mItems.Add(s);
            }

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mItems);
            mListView.Adapter = adapter;
            mListView.ItemClick += MListView_ItemClick;
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (mProgressBar != null)
            {
                //hiding ProgressBar and enable user interaction
                mProgressBar.Visibility = ViewStates.Invisible;
                Window.ClearFlags(WindowManagerFlags.NotTouchable);
            }       
        }

        private void MListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //Showing ProgressBar and disable any user interaction to avoid starting multiple instances
            mProgressBar.Visibility = ViewStates.Visible;
            Window.SetFlags(WindowManagerFlags.NotTouchable, WindowManagerFlags.NotTouchable);

            new Thread(new ThreadStart(delegate
            {
                //LOAD METHOD     
                mPreference = Application.GetSharedPreferences("TableSetup", FileCreationMode.Private);
                mEditor = mPreference.Edit();
                mEditor.PutInt("classIndex", e.Position);
                mEditor.Apply();
                mDataHandler.GetTimetable(e.Position);
                Intent iActivity = new Intent(this, typeof(Activities.TimetableWeekActivity));
                StartActivityForResult(iActivity, 1);              
            })).Start();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == 1 && resultCode == Result.Ok)
            {    
                if (Intent.GetBooleanExtra("StartMain", false))
                {
                    StartActivity(new Intent(this, typeof(Activities.MainActivity)));
                    Finish();
                }
                else Finish();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.slide_from_left, Resource.Animation.slide_to_right);
        }
    }
}