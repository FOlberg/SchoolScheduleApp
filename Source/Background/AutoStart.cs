using Android.App;
using Android.Content;

namespace ScheduleApp.Background
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class AutoStart : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionBootCompleted || intent.Action.Equals("android.intent.action.BOOT_COMPLETED"))
            {
                AlarmReceiver alarm = new AlarmReceiver();
                alarm.SetUpAlarmService(context);
            }
        }
    }
}