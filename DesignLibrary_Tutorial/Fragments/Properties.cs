using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.V7.Preferences;
using Helper.Header;
using Android.App;
using System.Threading;
using Android.Widget;
using Java.Lang;

namespace ScheduleApp.Fragments
{
    public class Properties : Android.Support.V7.Preferences.PreferenceFragmentCompat
    {
        ListPreference syncIntPreference;
        Preference schedulePreference;
        Preference classPreference;
        Preference vibrationPreference;
        Preference priorityPreference;
        Preference themePreference;
        ProgressDialog mProgressDialog;
        //RelativeLayout mProgressBar;

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

            //mProgressBar = Activity.FindViewById<RelativeLayout>(Resource.Id.stripeProBar);
            mProgressDialog = new ProgressDialog(Activity);
            //Change Schedule Pref.
            schedulePreference = FindPreference("ChangeSchedule");
            schedulePreference.PreferenceClick += SchedulePreference_PreferenceClick;
            //schedulePreference.Intent = new Intent(Activity, typeof(Activities.TimetableWeekActivity)); //typeof(Activities.TimetableWeekActivity)

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

        //private void SchedulePreference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        //{
        //    //var mProgressDialog = ProgressDialog.Show(Activity, "", "Stundenplan wird geladen...", true);
        //    //new Thread(new ThreadStart(delegate
        //    //{
        //    //    StartActivity(new Intent(Activity, typeof(Activities.TimetableWeekActivity)));
        //    //    Activity.RunOnUiThread(() => mProgressDialog.Cancel());
        //    //})).Start();
        //}

        private void SchedulePreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            if (!mProgressDialog.IsShowing)
                new InnerScheduleLoader(Activity, mProgressDialog).Execute();
            //mProgressBar.Visibility = ViewStates.Visible;
            //if (!mProgressDialog.IsShowing)
            //{
            //    mProgressDialog.Show();
            //    new Thread(new ThreadStart(delegate
            //    {
            //        StartActivity(new Intent(Activity, typeof(Activities.TimetableWeekActivity)));
            //        Activity.RunOnUiThread(() => mProgressDialog.Cancel());
            //    })).Start();
            //}

        }

        private class InnerScheduleLoader : AsyncTask
        {
            private Activity activity;
            private ProgressDialog progressDialog;
            public InnerScheduleLoader(Activity activity, ProgressDialog progress)
            {
                this.activity = activity;
                progressDialog = progress;
                progressDialog.SetMessage("Stundenplan wird geladen...");
                progressDialog.Indeterminate = true;
            }
            protected override Object DoInBackground(params Object[] @params)
            {
                //    new Thread(new ThreadStart(delegate
                //    {
                //        StartActivity(new Intent(Activity, typeof(Activities.TimetableWeekActivity)));
                //        Activity.RunOnUiThread(() => mProgressDialog.Cancel());
                //    })).Start();
                activity.StartActivity(new Intent(activity, typeof(Activities.TimetableWeekActivity)));
                return true;
            }
            protected override void OnPreExecute()
            {
                base.OnPreExecute();
                activity.RunOnUiThread(() => progressDialog.Show());
            }
            protected override void OnPostExecute(Object result)
            {
                base.OnPostExecute(result);
                activity.RunOnUiThread(() => progressDialog.Cancel());
            }
        }

        //private class InnerScheduleLoader : Intent
        //{
        //    //private Activity mBaseActivity;

        //    public InnerScheduleLoader(Activity activity)
        //    {
        //        var mProgressDialog = ProgressDialog.Show(activity, "", "Stundenplan wird geladen...", true);
        //        new Thread(new ThreadStart(delegate
        //        {
        //            activity.StartActivity(new Intent(activity, typeof(Activities.TimetableWeekActivity)));
        //            activity.RunOnUiThread(() => mProgressDialog.Cancel());
        //        })).Start();
        //    }
        //    //override  

        //    //protected override void OnHandleIntent(Intent intent)
        //    //{
        //    //    var mProgressDialog = ProgressDialog.Show(mBaseActivity, "", "Stundenplan wird geladen...", true);
        //    //    new Thread(new ThreadStart(delegate
        //    //    {
        //    //        StartActivity(new Intent(mBaseActivity, typeof(Activities.TimetableWeekActivity)));
        //    //        mBaseActivity.RunOnUiThread(() => mProgressDialog.Cancel());
        //    //    })).Start();
        //    //}
        //}


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