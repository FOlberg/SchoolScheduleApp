using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Helper.Header;
using Android.Content.Res;
using Java.Util;

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

            mTextView = FindViewById<TextView>(Resource.Id.textViewLic);
            mTextView.Text = GetLicenseText();
            // Create your application here
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
            string text = "Vertretungsplan App THG für Android\n\n";
            text += "Copyright © 2017 Frederik Olberg\n\n";
            text += "Das THG Logo unterliegt den Bildrechten des Theodor-Heuss-Gymnasiums Göttingen / www.thg-goettingen.de\n\n";
            text += "Die Vertretungsplan App für Android wurde mithilfe von Open Source Software entwickelt:\n\n";
            text += "\t• Android Open Source Project\n";
            text += "\t• Mono runtime\n";
            text += "\t• Newtonsoft.Json Json.NET\n";
            text += "\t• XAMARIN SOFTWARE\n\n";
            text += "\n\n";
            text += "The MIT License (MIT)\n\nCopyright(c) 2007 James Newton-King\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the \"Software\"),"+
                "to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/ or sell copies of the Software," +
                "and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\n\n" +
                "The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\n\n" +
                "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT." +
                "IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY," +
                "WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.\n";
            text += "\n\n\n";
            Scanner s = new Scanner(Assets.Open("ApacheLicense.txt")).UseDelimiter("\\A");
            text += s.HasNext ? s.Next() : "";



            return text;
        }
    }
}