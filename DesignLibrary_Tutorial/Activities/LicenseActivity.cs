using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Util;
using ScheduleApp.Handler;

namespace ScheduleApp.Activities
{
    [Activity(Label = "Rechtliche Hinweise")]
    public class LicenseActivity : AppCompatActivity
    {
        TextView mTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (DataHandler.GetDarkThemePref(this))
                SetTheme(Resource.Style.Theme_DarkTheme);
            else
                SetTheme(Resource.Style.Theme_DesignDemo);
            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.slide_from_right, Resource.Animation.slide_to_left);
            SetContentView(Resource.Layout.Activity_License);
            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);

            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);

            mTextView       = FindViewById<TextView>(Resource.Id.textViewLic);
            mTextView.Text  = GetLicenseText();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.slide_from_left, Resource.Animation.slide_to_right);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private string GetLicenseText()
        {
            string text = string.Empty;
            Scanner s = new Scanner(Assets.Open("LicenseText.txt")).UseDelimiter("\\A");
            text += s.HasNext ? s.Next() : "";
            s = new Scanner(Assets.Open("ApacheLicense.txt")).UseDelimiter("\\A");
            text += s.HasNext ? s.Next() : "";
            return text;
        }
    }
}