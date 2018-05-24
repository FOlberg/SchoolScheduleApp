using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Preferences;
using Android.Views;
using Android.Widget;
using Helper.Header;

namespace ScheduleApp.Activities
{
    [Activity(Label = "PreferenceActivity")]
    public class AdvPreferenceActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (DataHandler.GetDarkThemePref(this))
                SetTheme(Resource.Style.Theme_DarkTheme);
            else
                SetTheme(Resource.Style.Theme_DesignDemo);
            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.slide_from_right, Resource.Animation.slide_to_left);
            SetContentView(Resource.Layout.Activity_Preference);
            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);

            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);

            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.preference_frame, new PreferenceFragment()).Commit();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.slide_from_left, Resource.Animation.slide_to_right);
        }

        private class PreferenceFragment : PreferenceFragmentCompat
        {
            public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
            {
                AddPreferencesFromResource(Resource.Layout.App_Preferences);
            }
        }
    }
}