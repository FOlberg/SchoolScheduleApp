﻿using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using AlertDialog = Android.App.AlertDialog;
using Android.App;
using Android.Content;
using Android.OS;
using System;
using Android.Support.V7.App;
using Android.Support.V7.Preferences;
using Android.Views;
using Android.Widget;
using ScheduleApp.Handler;
using System.Threading;

namespace ScheduleApp.Activities
{
    [Activity(Label = "@string/act_label_adv_pref")]
    public class AdvancedPreferenceActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                if (DataHandler.GetDarkThemePref(this))
                    SetTheme(Resource.Style.Theme_DarkTheme_KitKat);
                else
                    SetTheme(Resource.Style.Theme_Light_KitKat);
            }
            else
            {
                if (DataHandler.GetDarkThemePref(this))
                    SetTheme(Resource.Style.Theme_DarkTheme);
                else
                    SetTheme(Resource.Style.Theme_Light);
            }
            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.slide_from_right, Resource.Animation.slide_to_left);
            SetContentView(Resource.Layout.Activity_Adv_Preference);
            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);

            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);

            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.preference_frame, new PreferenceFragment())
                .Commit();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.slide_from_left, Resource.Animation.slide_to_right);
        }

        private class PreferenceFragment : PreferenceFragmentCompat
        {
            Preference  mInfoText, mSourceClass, mSourcePlan, mSourceDelete, mDebug;
            EditText    mTextPartOne, mTextPartTwo, mTextPartThree;
            AlertDialog mSourceDialog;
            TextView    mAssembledText;
            ImageView   mLinkIcon;
            bool        mSourceAll;

            public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
            {
                AddPreferencesFromResource(Resource.Layout.App_Adv_Preferences);

                mInfoText       = FindPreference("info_text");
                mSourceClass    = FindPreference("class_source_preference");
                mSourcePlan     = FindPreference("all_source_preference");
                mSourceDelete   = FindPreference("delete_temp_data_preference");
                mDebug          = FindPreference("notification_debug_preference");

                mSourceClass.PreferenceClick    += SourcePreferenceClick;
                mSourcePlan.PreferenceClick     += SourcePreferenceClick;
                mSourceDelete.PreferenceClick   += SourceDelete_PreferenceClick;
                mDebug.PreferenceClick          += Debug_PreferenceClick;
            }

            private void Debug_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
            {
                //Starting background service to invoke new notifications
                Intent background = new Intent(Activity, typeof(Background.BackgroundService));
                Activity.StartService(background);
            }

            private void SourceDelete_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
            {
                var builder = new AlertDialog.Builder(Activity);
                var view = LayoutInflater.Inflate(Resource.Layout.alert_dialog_cache, null);
                builder.SetView(view)
                    .SetTitle("Cache leeren")
                    .SetPositiveButton("Löschen", (o, ev) => {
                        DeleteTempData();
                    })
                    .SetNegativeButton("Abbrechen", (o, ev) => { })
                    .Create()
                    .Show();
            }

            private void DeleteTempData()
            {
                var dataHandler = DataHandler.GetDataHandler();
                dataHandler.DeleteCache();
                new Thread(new ThreadStart(delegate
                {
                    dataHandler.GetClasses();
                    dataHandler.SaveState();
                })).Start();
            }

            private void SourcePreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
            {
                if (mSourceDialog == null || !mSourceDialog.IsShowing)
                {
                    var builder     = new AlertDialog.Builder(Activity);
                    var view        = LayoutInflater.Inflate(Resource.Layout.source_alert_dialog, null);
                    var config      = DataHandler.GetConfig();
                    string[] urlParts;
                    if (e.Preference.Key == "class_source_preference")
                    {
                        mSourceAll  = false;
                        urlParts    = config.urlSourceClass;
                    }         
                    else
                    {
                        mSourceAll  = true;
                        urlParts    = config.urlSourceAll;
                    }

                    mTextPartOne    = view.FindViewById<EditText>(Resource.Id.part1);
                    mTextPartTwo    = view.FindViewById<EditText>(Resource.Id.part2);
                    mTextPartThree  = view.FindViewById<EditText>(Resource.Id.part3);
                    mAssembledText  = view.FindViewById<TextView>(Resource.Id.endText);
                    mLinkIcon       = view.FindViewById<ImageView>(Resource.Id.icon_open_link);

                    mTextPartOne.Text = urlParts[0];
                    mTextPartTwo.Text = urlParts[1];
                    if (mSourceAll)
                        mTextPartThree.Visibility = ViewStates.Gone;
                    else mTextPartThree.Text = urlParts[2];
                    mAssembledText.Text = AssembleUrl();

                    mTextPartOne.AfterTextChanged   += AfterTextChanged;
                    mTextPartTwo.AfterTextChanged   += AfterTextChanged;
                    mTextPartThree.AfterTextChanged += AfterTextChanged;
                    mLinkIcon.Click                 += MLinkIcon_Click;

                    builder.SetTitle(e.Preference.Title)
                        .SetPositiveButton("Save", (o, ev) =>
                        {
                            if (mSourceAll)
                                config.urlSourceAll = new string[] { mTextPartOne.Text, mTextPartTwo.Text };
                            else config.urlSourceClass = new string[] { mTextPartOne.Text, mTextPartTwo.Text, mTextPartThree.Text };
                            DataHandler.SaveConfig(config);
                        })
                        .SetNeutralButton("Default", (o, ev) =>
                        {
                            if (mSourceAll) config.ResetSourceAll();
                            else config.ResetSourceClass();
                            DataHandler.SaveConfig(config);
                        })
                        .SetNegativeButton("Abbrechen", (o, ev) => 
                        {
                            mSourceDialog.Cancel();
                        });
                    builder.SetView(view);
                    mSourceDialog = builder.Create();
                    mSourceDialog.Show();
                }
            }

            private void MLinkIcon_Click(object sender, EventArgs e)
            {
                Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(mAssembledText.Text));
                StartActivity(Intent.CreateChooser(browserIntent, Resources.GetString(Resource.String.broswer_intent_selection)));
            }

            private string AssembleUrl()
            {
                if (mSourceAll) return mTextPartOne.Text + TimeHandler.GetCurrentWeek().ToString("D2") + mTextPartTwo.Text;
                return mTextPartOne.Text + TimeHandler.GetCurrentWeek().ToString("D2") + mTextPartTwo.Text + 25.ToString("D5") + mTextPartThree.Text;
            }

            private void AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
            {
                mAssembledText.Text = AssembleUrl();
            }
        }
    }
}