using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.V7.Preferences;
using ScheduleApp.Handler;
using Android.App;
using Java.Lang;

namespace ScheduleApp.Fragments
{
    public class Properties : Android.Support.V7.Preferences.PreferenceFragmentCompat
    {
        ListPreference mSyncIntPreference;
        Preference mSchedulePreference;
        Preference mClassPreference;
        Preference mVibrationPreference;
        Preference mPriorityPreference;
        Preference mThemePreference;
        Preference mAdvSettingsPreference;
        ProgressDialog mProgressDialog;

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            // Load the Preferences from the XML file
            AddPreferencesFromResource(Resource.Layout.App_Preferences);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            Activity.Title = "Einstellungen";
        }

        public override void OnStart()
        {
            base.OnStart();
            var config = DataHandler.GetConfig();

            // Change Schedule Pref.
            mClassPreference            = FindPreference("ChangeClass");
            mClassPreference.Intent     = new Intent(Activity, typeof(Activities.TimetableSetupActivity));
            mClassPreference.Summary    = "Aktuell ausgewählt: " + config.GetClassName();

            // mProgressBar = Activity.FindViewById<RelativeLayout>(Resource.Id.stripeProBar);
            mProgressDialog = new ProgressDialog(Activity);

            // Change Schedule Pref.
            mSchedulePreference = FindPreference("ChangeSchedule");
            mSchedulePreference.PreferenceClick += SchedulePreference_PreferenceClick;

            // Update Sequence Pref.
            mSyncIntPreference = (ListPreference)FindPreference("SyncIntervall_preference");
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
        }

        private void SchedulePreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            if (!mProgressDialog.IsShowing)
                new InnerScheduleLoader(Activity, mProgressDialog).Execute();
        }

        private class InnerScheduleLoader : AsyncTask
        {
            private Activity mActivity;
            private ProgressDialog mProgressDialog;

            public InnerScheduleLoader(Activity activity, ProgressDialog progress)
            {
                this.mActivity  = activity;
                mProgressDialog = progress;
                mProgressDialog.SetMessage("Stundenplan wird geladen...");
                mProgressDialog.Indeterminate = true;
            }
            protected override Object DoInBackground(params Object[] @params)
            {
                var editor = mActivity.GetSharedPreferences("TableSetup", FileCreationMode.Private).Edit();
                editor.PutInt("classIndex", -1).Apply();
                mActivity.StartActivity(new Intent(mActivity, typeof(Activities.TimetableWeekActivity)));
                return true;
            }
            protected override void OnPreExecute()
            {
                base.OnPreExecute();
                mActivity.RunOnUiThread(() => mProgressDialog.Show());
            }
            protected override void OnPostExecute(Object result)
            {
                base.OnPostExecute(result);
                mActivity.RunOnUiThread(() => mProgressDialog.Cancel());
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