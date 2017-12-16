using Android.App;
using Android.OS;
using Android.Views;
using Android.Content;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.App;
using Android.Widget;
using System.Collections.Generic;
using Helper.Header;
using Android.Runtime;
using System.Threading;

namespace ScheduleApp.Activities
{
    [Activity(Label = "Klassen")]
    public class TimetableSetupActivity : AppCompatActivity
    {
        private List<string> mItems;
        private ListView mListView;
        public DataHandler mDataHandler;
        ISharedPreferences preferences;
        ISharedPreferencesEditor editor;
        ProgressDialog mProgressDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (DataHandler.GetDarkThemePref(this))
                SetTheme(Resource.Style.Theme_DarkTheme);
            else
                SetTheme(Resource.Style.Theme_DesignDemo);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Activity_Setup);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBarT);
            SetSupportActionBar(toolBar);

            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            ab.SetHomeButtonEnabled(true);

            // Create your application here
            mListView = FindViewById<ListView>(Resource.Id.listView);

            //Loading mData
            mDataHandler = DataHandler.GetDataHandler();

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
            if (mProgressDialog != null)
                mProgressDialog.Hide();
        }

        private void MListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            mProgressDialog = ProgressDialog.Show(this, "", "Stundenplan wird geladen...", true);

            new Thread(new ThreadStart(delegate
            {
                //LOAD METHOD     
                preferences = Application.GetSharedPreferences("TableSetup", FileCreationMode.Private);
                editor = preferences.Edit();
                editor.PutInt("classIndex", e.Position);
                editor.Apply();
                mDataHandler.GetTimetable(e.Position);
                Intent iActivity = new Intent(this, typeof(Activities.TimetableWeekActivity)); //Activities.TimetableWeekActivity
                StartActivityForResult(iActivity, 1);              
                //RunOnUiThread(() => progressDialog.Hide());
            })).Start();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            //base.OnActivityResult(requestCode, resultCode, data);
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
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}