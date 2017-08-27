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
using DesignLibrary_Tutorial.Fragments;

namespace DesignLibrary_Tutorial
{
    [Activity(Label = "DesignLibrary_Tutorial", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/Theme.DesignDemo")]
    public class MainActivity : AppCompatActivity
    {
        private DrawerLayout mDrawerLayout;
        IMenuItem previousItem;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);

            SupportActionBar ab = SupportActionBar;
            ab.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            ab.SetDisplayHomeAsUpEnabled(true);

            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            if (navigationView != null)
            {
                SetUpDrawerContent(navigationView);
            }
            if (bundle == null)
            {
                ListItemClicked(0);
                navigationView.SetCheckedItem(Resource.Id.nav_menu1);
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
                        ListItemClicked(0);
                        break;
                    case Resource.Id.nav_menu2:
                        ListItemClicked(1);
                        break;
                    case Resource.Id.nav_menu3:
                        ListItemClicked(2);
                        break;
                    case Resource.Id.nav_menu4:
                        ListItemClicked(3);
                        break;
                }
                //e.MenuItem.SetChecked(true);
                mDrawerLayout.CloseDrawers();
            };
        }

        private void ListItemClicked(int position)
        {
            Android.Support.V4.App.Fragment fragment = null;
            switch (position)
            {
                case 0:
                    fragment = new Menu1();
                    break;
                case 1:
                    fragment = new Menu2();
                    break;
                case 2:
                    fragment = new Menu3();
                    break;
                case 3:
                    fragment = new Menu4();
                    break;
            }

            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.content_frame, fragment)
                .Commit();
        }

        
    }
}

