using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using SupportDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using SupportFragmentManager = Android.Support.V4.App.FragmentManager;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Newtonsoft.Json;
using ScheduleApp.Fragments;
using ScheduleApp.Handler;
using Android.Content;
using Helper.Header;
using System.Linq;
using Android.Graphics;

namespace ScheduleApp.Activities
{
    [Activity(Label = "@string/app_name", Icon = "@drawable/icon", Theme = "@style/Theme.DesignDemo")]
    public class MainActivity : AppCompatActivity
    {
        private DrawerLayout mDrawerLayout;
        IMenuItem previousItem;
        NavigationView navigationView;
        bool darkTheme = false;
        SupportActionBar ab;
        int selFragment;

        protected override void OnCreate(Bundle bundle)
        {
            darkTheme = DataHandler.GetDarkThemePref(this);
            if (darkTheme)
                SetTheme(Resource.Style.Theme_DarkTheme);
            else
                SetTheme(Resource.Style.Theme_DesignDemo);
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);

            ab = SupportActionBar;
            ab.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            ab.SetDisplayHomeAsUpEnabled(true);

            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            InitHandler();
            
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            if (navigationView != null)
            {
                SetUpDrawerContent(navigationView);
            }
            selFragment = GetSharedPreferences("Config", FileCreationMode.Private).GetBoolean("ThemeChanged", false) ? 3 : 1;
            if (bundle == null)
            {
                if (selFragment == 3)
                {
                    navigationView.SetCheckedItem(Resource.Id.nav_menu4);
                    ListItemClicked(2);
                    GetSharedPreferences("Config", FileCreationMode.Private).Edit().PutBoolean("ThemeChanged", false).Apply();
                }
                else
                {
                    ListItemClicked(0);
                    navigationView.SetCheckedItem(Resource.Id.nav_menu1);
                }
            }
            //var spinner = FindViewById<Spinner>(Resource.Id.appbar_spinner);
            //var arr = DataHandler.GetDataHandler().GetClasses().ToList<string>();
            //arr.Insert(0, "Keine");
            //ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Resource.Layout.support_simple_spinner_dropdown_item, arr);
            //spinner.Adapter = adapter;
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
            if (selFragment == 2)//previousItem != null && previousItem.GroupId == 1)
            {
                MenuInflater.Inflate(Resource.Menu.appbar_spinner, menu);
                var item = menu.FindItem(Resource.Id.spinner);
                var spinner = item.ActionView as Spinner;
                var arr = DataHandler.GetDataHandler().GetClasses().ToList<string>();
                arr.Insert(0, "Klasse");
                //Resource.Layout.abc_action_menu_item_layout
                ArrayAdapter<string> adapter;
   
                if (darkTheme)
                {
                    adapter = new ArrayAdapter<string>(this, Resource.Layout.support_simple_spinner_dropdown_item, arr);
                }
                else
                {
                    //Context con = ab.ThemedContext;
                    adapter = new ArrayAdapter<string>(ab.ThemedContext, Resource.Layout.support_simple_spinner_dropdown_item, arr);
                    adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
                    //adapter = new ArrayAdapter<string>(this, Resource.Layout.spinnerLayout, arr);
                    //adapter.SetDropDownViewResource(Resource.Layout.spinnerDropDownItem);
                }
                spinner.Adapter = adapter;
                spinner.SetMinimumWidth(300);
                spinner.TextAlignment = TextAlignment.TextEnd;
                //spinner.ForceHasOverlappingRendering(false);
            }


            return base.OnCreateOptionsMenu(menu);
        }

        private void SetUpDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (object sender, NavigationView.NavigationItemSelectedEventArgs e) =>
            {

                if (previousItem != null)
                    previousItem.SetChecked(false);

                navigationView.SetCheckedItem(e.MenuItem.ItemId);  

                previousItem = e.MenuItem;
                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_menu1:
                        selFragment = 1;
                        break;
                    //case Resource.Id.nav_menu2:
                    //    ListItemClicked(1);
                    //    break;
                    case Resource.Id.nav_menu3:
                        selFragment = 2;
                        break;
                    case Resource.Id.nav_menu4:
                        selFragment = 3;
                        break;
                }
                InvalidateOptionsMenu();
                ListItemClicked(selFragment - 1);
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
                //case 1:
                //    fragment = new Menu2();
                //    break;
                case 1:
                    fragment = new PlanSelect();//new Menu2();
                    break;
                case 2:
                    fragment = new Properties();
                    break;
            }

            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.content_frame, fragment)
                .Commit();
        }

        private void InitHandler()
        {
            Android.Content.ISharedPreferences preferences = Application.GetSharedPreferences("Dashboard", FileCreationMode.Private);
            MessageHandler msgHandler;
            string dataSource = preferences.GetString("MsgHandler", string.Empty);
            if (dataSource == string.Empty)
            {
                msgHandler = new MessageHandler();
                string s = JsonConvert.SerializeObject(msgHandler);
                preferences.Edit().PutString("MsgHandler", s).Apply();
            }
            else
            {
                msgHandler = JsonConvert.DeserializeObject<MessageHandler>(dataSource);
            }
        }
    }
}

