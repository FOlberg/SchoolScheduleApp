using Android.App;
using Android.OS;
using Android.Views;
using Android.Content;
using Android.Preferences;
using Android.Support.V4.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using SupportFragmentManager = Android.Support.V4.App.FragmentManager;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Newtonsoft.Json;
using Android.Widget;
using System.Collections.Generic;
using AppTestProzesse.Header;
using Android.Runtime;
using System.Runtime;
using System;
using System.Threading.Tasks;

namespace DesignLibrary_Tutorial.Activities
{
    [Activity(Label = "Klasse auswählen", Theme = "@style/Theme.DesignDemo")]
    public class TimetableSetupActivity : AppCompatActivity
    {
        private List<string> mItems;
        private ListView mListView;
        public DataHandler mDataHandler;
        ISharedPreferences preferences;
        ISharedPreferencesEditor editor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
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

        private void MListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //Toast.MakeText(this, e.Position + "c", ToastLength.Short).Show();

            preferences = Application.GetSharedPreferences("TableSetup", FileCreationMode.Private);
            editor = preferences.Edit();
            editor.PutInt("classIndex", e.Position);
            editor.Apply();
            Intent iActivity = new Intent(this, typeof(Activities.TimetableWeekActivity)); //Activities.TimetableWeekActivity
            StartActivityForResult(iActivity, 1);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            //base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1 && resultCode == Result.Ok)
            {
                Finish();
                if (Intent.GetBooleanExtra("StartMain", false))
                {
                    StartActivity(new Intent(this, typeof(Activities.MainActivity)));
                }
                
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