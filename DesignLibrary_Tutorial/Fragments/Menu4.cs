using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.V7.Preferences;
using AppTestProzesse.Header;

namespace DesignLibrary_Tutorial.Fragments
{
    public class Menu4 : Android.Support.V7.Preferences.PreferenceFragmentCompat
    {
        ListPreference syncIntPreference;
        Preference schedulePreference;
        Preference vibrationPreference;

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            // Load the Preferences from the XML file
            AddPreferencesFromResource(Resource.Layout.App_Preferences);

            var config = DataHandler.GetConfig();

            //Change Schedule Pref.
            schedulePreference = FindPreference("ChangeSchedule");
            schedulePreference.Intent = new Intent(Activity, typeof(DesignLibrary_Tutorial.Activities.TimetableWeekActivity));
            schedulePreference.Summary = "Ausgewählte Klasse: " + config.GetClassName();

            //Update Sequence Pref.
            syncIntPreference = (ListPreference) FindPreference("SyncIntervall_preference");
            syncIntPreference.SetDefaultValue(config.updateSequence);
            syncIntPreference.PreferenceChange += SyncIntPreference_PreferenceChange;

            //Vibration Pref.
            vibrationPreference = FindPreference("vibration_preference");
            vibrationPreference.SetDefaultValue(config.vibration);
            vibrationPreference.PreferenceChange += VibrationPreference_PreferenceChange;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            Activity.Title = "Einstellungen";
        }

        private void VibrationPreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            var config = DataHandler.GetConfig();
            config.vibration = (bool) e.NewValue;
            DataHandler.SaveConfig(config);
        }

        private void SyncIntPreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            var config = DataHandler.GetConfig();
            int.TryParse(e.NewValue.ToString().Replace("{", "").Replace("}", ""), out int newValue);
            //log
            if (newValue > 0 && newValue != config.updateSequence)
            {
                Background.AlarmReceiver.mOldSequence = config.updateSequence;
                config.updateSequence = newValue;
                DataHandler.SaveConfig(config);
            }  
        }
    }
}