using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace ScheduleApp.Fragments
{
    public class Debug_Fragment : Android.Support.V4.App.Fragment
    {
        private void B_Click(object sender, System.EventArgs e)
        {
            Intent background = new Intent(Activity, typeof(Background.BackgroundService));
            Activity.StartService(background);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.frag_debug, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            this.Activity.Title = "DEBUG";
            Button b            = Activity.FindViewById<Button>(Resource.Id.buttonTest);
            b.Click             += B_Click;
        }
    }
}