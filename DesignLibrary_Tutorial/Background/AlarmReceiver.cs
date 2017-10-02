using Android.App;
using Android.Content;
using Android.OS;

namespace DesignLibrary_Tutorial.Background
{
    [BroadcastReceiver]
    class AlarmReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Intent background = new Intent(context, typeof(BackgroundService));
            context.StartService(background);
        }

        public void SetAlarm(Context context)
        {
            AlarmManager am = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent i = new Intent(context, typeof(AlarmReceiver));
            PendingIntent pi = PendingIntent.GetBroadcast(context, 0, i, 0);

            bool alarmRunning = (PendingIntent.GetBroadcast(context, 0, i, PendingIntentFlags.NoCreate) != null);
            if (alarmRunning)
            {
                CancelAlarm(context);
            }
            am.SetRepeating(AlarmType.RtcWakeup, SystemClock.CurrentThreadTimeMillis(), 1000 * 20, pi); // Millisec * Second * Minute
        }

        public void CancelAlarm(Context context)
        {
            Intent intent = new Intent(context, this.Class);
            PendingIntent sender = PendingIntent.GetBroadcast(context, 0, intent, 0);
            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            alarmManager.Cancel(sender);
        }
    }
}