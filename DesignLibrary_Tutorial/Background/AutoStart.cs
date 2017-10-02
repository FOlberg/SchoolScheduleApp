using Android.Content;
using Android.Widget;
using DesignLibrary_Tutorial.Helpers;

namespace DesignLibrary_Tutorial.Background
{
    [BroadcastReceiver]
    public class AutoStart : BroadcastReceiver
    {
        AlarmReceiver alarm = new AlarmReceiver();
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, "Received intent! AutoStart", ToastLength.Short).Show();
            if (intent.Action.Equals("android.intent.action.BOOT_COMPLETED"))
            {
                alarm.SetAlarm(context);
            }
        }
    }
}