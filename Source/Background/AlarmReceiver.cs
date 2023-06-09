﻿using Android.App;
using Android.Content;
using System;
using ScheduleApp.Handler;
using Android.OS;

namespace ScheduleApp.Background
{
    [BroadcastReceiver]
    class AlarmReceiver : BroadcastReceiver
    {
        public static int mOldSequence = 0;

        public override void OnReceive(Context context, Intent intent)
        {
            Intent background = new Intent(context, typeof(BackgroundService));
            context.StartService(background);
            SetNextAlarm(context);
        }

        public void SetUpAlarmService(Context context)
        {
            if (mOldSequence > 0)
            {
                CancelOldAlarm(context, mOldSequence);
                mOldSequence = 0;
            }

            AlarmManager am     = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent intent       = new Intent(context, typeof(AlarmReceiver));
            PendingIntent pi    = PendingIntent.GetBroadcast(context, 0, intent, 0);

            int sequence = DataHandler.GetConfig().mSettings.mUpdateSequence;
            if (sequence <= 0)
            {
                //log
                var config = DataHandler.GetConfig();
                config.mSettings.mUpdateSequence = 120;
                DataHandler.SaveConfig(config);
            }

            for (int t = 6 * 60; t <= 23 * 60; t += sequence)
            {
                pi = PendingIntent.GetBroadcast(context, t, intent, 0);
                if (PendingIntent.GetBroadcast(context, t, intent, PendingIntentFlags.NoCreate) != null)
                    am.Cancel(pi);
                long time = GetMilisecondsUntilNextCheckS((int)Math.Floor(t / 60.0), t % 60);
                SetAlarm(am, time, pi);
            }

            pi = PendingIntent.GetBroadcast(context, 420, intent, 0);
            if (PendingIntent.GetBroadcast(context, 420, intent, PendingIntentFlags.NoCreate) == null)
                SetAlarm(am, GetMilisecondsUntilNextCheckS(7, 0), pi);
        }

        public void SetNextAlarm(Context context)
        {
            int sequence = DataHandler.GetConfig().mSettings.mUpdateSequence;
            if (DateTime.Now.Hour * 60 + DateTime.Now.Minute % sequence == 0)
            {
                AlarmManager am     = (AlarmManager)context.GetSystemService(Context.AlarmService);
                Intent intent       = new Intent(context, typeof(AlarmReceiver));
                PendingIntent pi    = PendingIntent.GetBroadcast(context, 0, intent, 0);

                long t = GetMilisecondsUntilNextCheckS(DateTime.Now.Hour, DateTime.Now.Minute);
                SetAlarm(am, t, pi);
            }
        }

        private void SetAlarm(AlarmManager alarmManager, long time, PendingIntent pi)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                alarmManager.SetExact(AlarmType.RtcWakeup, time, pi);
            else
                alarmManager.Set(AlarmType.RtcWakeup, time, pi);
        }

        public static long GetMilisecondsUntilNextCheckS(int hour, int min)
        {
            DateTime now            = DateTime.Now;
            DateTime todayAtTime    = now.Date.AddHours(hour).AddMinutes(min);
            DateTime nextInstance   = now <= todayAtTime ? todayAtTime : todayAtTime.AddDays(1);
            TimeSpan span           = nextInstance - now;
            using (var cal = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default))
            {
                cal.Set(Java.Util.CalendarField.Millisecond, 0);
                cal.Add(Java.Util.CalendarField.Millisecond, (int)span.TotalMilliseconds);
                return cal.TimeInMillis;
            }
        }

        //public void CancelAlarm(Context context)
        //{
        //    Intent intent = new Intent(context, this.Class);
        //    PendingIntent sender = PendingIntent.GetBroadcast(context, 0, intent, 0);
        //    AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
        //    alarmManager.Cancel(sender);
        //}

        public static void CancelOldAlarm(Context context, int sequence)
        {
            AlarmManager am     = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent intent       = new Intent(context, typeof(AlarmReceiver));
            PendingIntent pi    = PendingIntent.GetBroadcast(context, 0, intent, 0);

            for (int t = 6 * 60; t <= 23 * 60; t += sequence)
            {
                pi = PendingIntent.GetBroadcast(context, t, intent, 0);
                if (PendingIntent.GetBroadcast(context, t, intent, PendingIntentFlags.NoCreate) != null)
                    am.Cancel(pi);
            }
        }
    }
}