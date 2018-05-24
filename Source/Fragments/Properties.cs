using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.V7.Preferences;
using ScheduleApp.Handler;
using Android.Widget;
using System.Threading;
using Thread = System.Threading.Thread;

namespace ScheduleApp.Fragments
{
    public class Properties : Android.Support.V7.Preferences.PreferenceFragmentCompat
    {
        ListPreference  mSyncIntPreference;
        Preference      mSchedulePreference;
        Preference      mClassPreference;
        Preference      mVibrationPreference;
        Preference      mPriorityPreference;
        Preference      mThemePreference;
        Preference      mAdvSettingsPreference;
        ProgressBar     mProgressBar;

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            // Load the Preferences from the XML file
            AddPreferencesFromResource(Resource.Layout.App_Preferences);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            Activity.Title = Activity.GetString(Resource.String.frag_label_properties);
        }

        public override void OnStart()
        {
            base.OnStart();
            var config = DataHandler.GetConfig();

            mProgressBar = Activity.FindViewById<ProgressBar>(Resource.Id.progress_bar);
            mProgressBar.Visibility = ViewStates.Invisible;

            // Change Schedule Pref.
            mClassPreference = FindPreference("change_class");
            mClassPreference.Intent = new Intent(Activity, typeof(Activities.TimetableSetupActivity));

            // Change Schedule Pref.
            mSchedulePreference = FindPreference("change_schedule");
            mSchedulePreference.PreferenceClick += SchedulePreference_PreferenceClick;

            // Update Sequence Pref.
            mSyncIntPreference = (ListPreference)FindPreference("sync_intervall_preference");
            mSyncIntPreference.SetDefaultValue(config.mSettings.mUpdateSequence);
            mSyncIntPreference.PreferenceChange += SyncIntPreference_PreferenceChange;

            // Vibration Pref.
            mVibrationPreference = FindPreference("vibration_preference");
            mVibrationPreference.SetDefaultValue(config.mSettings.mVibration);
            mVibrationPreference.PreferenceChange += VibrationPreference_PreferenceChange;

            // Priority Pref.
            mPriorityPreference = FindPreference("priority_preference");
            mPriorityPreference.SetDefaultValue(config.mSettings.mPriority);
            mPriorityPreference.PreferenceChange += PriorityPreference_PreferenceChange;

            // Light/Dark Theme Pref.
            mThemePreference = FindPreference("theme_preference");
            mThemePreference.SetDefaultValue(DataHandler.GetDarkThemePref(Activity));
            mThemePreference.PreferenceChange += ThemePreference_PreferenceChange;

            mAdvSettingsPreference          = FindPreference("advsettings_preference");
            mAdvSettingsPreference.Intent   = new Intent(Activity, typeof(Activities.AdvancedPreferenceActivity));

            var licensePreference           = FindPreference("license_preference");
            licensePreference.Intent        = new Intent(Activity, typeof(Activities.LicenseActivity));

            if (config.mConfigSel < 0) {
                mSchedulePreference.Enabled = false;
                mClassPreference.Summary = string.Empty;
            }
            else
                mClassPreference.Summary = "Aktuell ausgewählt: " + config.GetClassName();
        }

        private void SchedulePreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            if (mProgressBar.Visibility == ViewStates.Invisible || mProgressBar.Visibility == ViewStates.Gone)
            {
                mProgressBar.Visibility = ViewStates.Visible;

                new Thread(new ThreadStart(delegate
                {
                    var editor = Activity.GetSharedPreferences("TableSetup", FileCreationMode.Private).Edit();
                    editor.PutInt("classIndex", -1).Apply();
                    Activity.StartActivity(new Intent(Activity, typeof(Activities.TimetableWeekActivity)));
                })).Start();
            }
        }

        private void ThemePreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            Activity.GetSharedPreferences("Config", FileCreationMode.Private).Edit().PutBoolean("DarkTheme", (bool)e.NewValue).Apply();
            Activity.GetSharedPreferences("Config", FileCreationMode.Private).Edit().PutBoolean("ThemeChanged", true).Apply();
            var intent = Activity.Intent;
            Activity.Finish();
            StartActivity(intent);
        }

        private void PriorityPreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            var config = DataHandler.GetConfig();
            config.mSettings.mPriority = (bool) e.NewValue;
            DataHandler.SaveConfig(config);
        }

        private void VibrationPreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            var config = DataHandler.GetConfig();
            config.mSettings.mVibration = (bool) e.NewValue;
            DataHandler.SaveConfig(config);
        }

        private void SyncIntPreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            var config = DataHandler.GetConfig();
            int.TryParse(e.NewValue.ToString().Replace("{", "").Replace("}", ""), out int newValue);
            //log
            if (newValue > 0 && newValue != config.mSettings.mUpdateSequence)
            {
                Background.AlarmReceiver.mOldSequence   = config.mSettings.mUpdateSequence;
                config.mSettings.mUpdateSequence         = newValue;
                DataHandler.SaveConfig(config);
            }
        }
    }
}