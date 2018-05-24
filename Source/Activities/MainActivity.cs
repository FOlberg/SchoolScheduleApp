using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.Widget;
using SupportFragmentManager = Android.Support.V4.App.FragmentManager;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Newtonsoft.Json;
using ScheduleApp.Fragments;
using ScheduleApp.Handler;
using Android.Content;
using System.Linq;

namespace ScheduleApp.Activities
{
    [Activity(Label = "@string/app_name", Icon = "@drawable/icon", Theme = "@style/Theme.Light")]
    public class MainActivity : AppCompatActivity
    {
        private DrawerLayout    mDrawerLayout;
        NavigationView          mNavigationView;
        SupportActionBar        mActionBar;
        IMenuItem               mPreviousItem;
        bool                    mDarkTheme = false;
        int                     mSelectFragment;

        protected override void OnCreate(Bundle bundle)
        {
            mDarkTheme = DataHandler.GetDarkThemePref(this);
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                if (mDarkTheme)
                    SetTheme(Resource.Style.Theme_DarkTheme_KitKat);
                else
                    SetTheme(Resource.Style.Theme_Light_KitKat);
            }
            else
            {
                if (mDarkTheme)
                    SetTheme(Resource.Style.Theme_DarkTheme);
                else
                    SetTheme(Resource.Style.Theme_Light);
            }
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);

            mActionBar = SupportActionBar;
            mActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            mActionBar.SetDisplayHomeAsUpEnabled(true);

            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            InitHandler();
            
            mNavigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            if (mNavigationView != null)
                SetUpDrawerContent(mNavigationView);

            mSelectFragment = GetSharedPreferences("Config", FileCreationMode.Private).GetBoolean("ThemeChanged", false) ? 3 : 1;
            if (bundle == null)
            {
                if (mSelectFragment == 3)
                {
                    mNavigationView.SetCheckedItem(Resource.Id.nav_menu_third);
                    ListItemClicked(2);
                    GetSharedPreferences("Config", FileCreationMode.Private).Edit().PutBoolean("ThemeChanged", false).Apply();
                }
                else
                {
                    ListItemClicked(0);
                    mNavigationView.SetCheckedItem(Resource.Id.nav_menu_first);
                }
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    mDrawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);                    
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (mSelectFragment == 2)//previousItem != null && previousItem.GroupId == 1)
            {
                MenuInflater.Inflate(Resource.Menu.appbar_spinner, menu);
                ArrayAdapter<string> adapter;
                var item    = menu.FindItem(Resource.Id.spinner);
                var spinner = item.ActionView as Spinner;
                var arr     = DataHandler.GetDataHandler().GetClasses().ToList<string>();
                arr.Insert(0, "Klasse");
                
                if (mDarkTheme)
                    adapter = new ArrayAdapter<string>(this, Resource.Layout.support_simple_spinner_dropdown_item, arr);
                else
                {
                    adapter = new ArrayAdapter<string>(mActionBar.ThemedContext, Resource.Layout.support_simple_spinner_dropdown_item, arr);
                    adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
                }
                spinner.Adapter = adapter;
                spinner.SetMinimumWidth(300);
                spinner.TextAlignment = TextAlignment.TextEnd;
            }
            return base.OnCreateOptionsMenu(menu);
        }

        private void SetUpDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (object sender, NavigationView.NavigationItemSelectedEventArgs e) =>
            {

                if (mPreviousItem != null)
                    mPreviousItem.SetChecked(false);

                navigationView.SetCheckedItem(e.MenuItem.ItemId);  

                mPreviousItem = e.MenuItem;
                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_menu_first:
                        mSelectFragment = 1;
                        break;
                    case Resource.Id.nav_menu_sec:
                        mSelectFragment = 2;
                        break;
                    case Resource.Id.nav_menu_third:
                        mSelectFragment = 3;
                        break;
                        //case Resource.Id.nav_debug:
                        //    mSelectFragment = 4;
                        //    break;
                }
                InvalidateOptionsMenu();
                ListItemClicked(mSelectFragment - 1);
                e.MenuItem.SetChecked(true);
                mDrawerLayout.CloseDrawers();
            };
        }

        private void ListItemClicked(int position)
        {
            Android.Support.V4.App.Fragment fragment = null;
            switch (position)
            {
                case 0:
                    fragment = new Dashboard();
                    break;
                case 1:
                    fragment = new PlanSelect();
                    break;
                case 2:
                    fragment = new Properties();
                    break;
                    //case 3:
                    //    fragment = new Debug_Fragment();
                    //    break;
            }

            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.content_frame, fragment)
                .Commit();
        }

        private void InitHandler()
        {
            MessageHandler msgHandler;
            ISharedPreferences preferences  = Application.GetSharedPreferences("Dashboard", FileCreationMode.Private);
            string dataSource               = preferences.GetString("MsgHandler", string.Empty);
            if (dataSource == string.Empty)
            {
                msgHandler  = new MessageHandler();
                string s    = JsonConvert.SerializeObject(msgHandler);
                preferences.Edit().PutString("MsgHandler", s).Apply();
            }
            else
                msgHandler  = JsonConvert.DeserializeObject<MessageHandler>(dataSource);
        }
    }
}

