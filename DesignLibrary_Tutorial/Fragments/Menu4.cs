using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.V7.Preferences;
using Helper.Header;

namespace ScheduleApp.Fragments
{
    public class Menu4 : Android.Support.V7.Preferences.PreferenceFragmentCompat
    {
        ListPreference syncIntPreference;
        Preference schedulePreference;
        Preference classPreference;
        Preference vibrationPreference;
        Preference priorityPreference;
        Preference themePreference;

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

            //Change Schedule Pref.
            classPreference = FindPreference("ChangeClass");
            classPreference.Intent = new Intent(Activity, typeof(Activities.TimetableSetupActivity));
            classPreference.Summary = "Aktuell ausgewählt: " + config.GetClassName();

            //Change Schedule Pref.
            schedulePreference = FindPreference("ChangeSchedule");
            schedulePreference.Intent = new Intent(Activity, typeof(Activities.TimetableWeekActivity));
            //schedulePreference.Summary = "Ausgewählte Klasse: " + config.GetClassName();

            //Update Sequence Pref.
            syncIntPreference = (ListPreference)FindPreference("SyncIntervall_preference");
            syncIntPreference.SetDefaultValue(config.mSettings.updateSequence);
            syncIntPreference.PreferenceChange += SyncIntPreference_PreferenceChange;

            //Vibration Pref.
            vibrationPreference = FindPreference("vibration_preference");
            vibrationPreference.SetDefaultValue(config.mSettings.vibration);
            vibrationPreference.PreferenceChange += VibrationPreference_PreferenceChange;

            //Priority Pref.
            priorityPreference = FindPreference("priority_preference");
            priorityPreference.SetDefaultValue(config.mSettings.priority);
            priorityPreference.PreferenceChange += PriorityPreference_PreferenceChange;

            themePreference = FindPreference("theme_preference");
            themePreference.SetDefaultValue(DataHandler.GetDarkThemePref(Activity));
            themePreference.PreferenceChange += ThemePreference_PreferenceChange;
        }

        private void ThemePreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            Activity.GetSharedPreferences("Config", FileCreationMode.Private).Edit().PutBoolean("DarkTheme", (bool) e.NewValue).Apply();
            Activity.GetSharedPreferences("Config", FileCreationMode.Private).Edit().PutBoolean("ThemeChanged", true).Apply();
            var intent = Activity.Intent;
            Activity.Finish();
            StartActivity(intent);
        }

        private void PriorityPreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            var config = DataHandler.GetConfig();
            config.mSettings.priority = (bool)e.NewValue;
            DataHandler.SaveConfig(config);
        }

        private void VibrationPreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            var config = DataHandler.GetConfig();
            config.mSettings.vibration = (bool) e.NewValue;
            DataHandler.SaveConfig(config);
        }

        private void SyncIntPreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            var config = DataHandler.GetConfig();
            int.TryParse(e.NewValue.ToString().Replace("{", "").Replace("}", ""), out int newValue);
            //log
            if (newValue > 0 && newValue != config.mSettings.updateSequence)
            {
                Background.AlarmReceiver.mOldSequence = config.mSettings.updateSequence;
                config.mSettings.updateSequence = newValue;
                DataHandler.SaveConfig(config);
            }  
        }
    }
}