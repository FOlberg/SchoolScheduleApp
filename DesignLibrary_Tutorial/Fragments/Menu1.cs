using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace DesignLibrary_Tutorial.Fragments
{
    public class Menu1 : Android.Support.V4.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            //Do not handle events here! like button click etc -> because OnCreate will be called before OnCreatedView
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            //Toast.MakeText(Activity, "Button pressed!", ToastLength.Long).Show();
            Intent iActivity = new Intent(Activity, typeof(Activities.TimetableSetupActivity)); //Activities.TimetableWeekActivity
            StartActivity(iActivity);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.menu1, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            Activity.Title = "Menu1";

            Button btnTest = View.FindViewById<Button>(Resource.Id.button1);
            btnTest.Click += BtnTest_Click;
        }
    }
}