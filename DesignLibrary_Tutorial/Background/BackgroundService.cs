﻿using Android.App;
using Android.Content;
using Android.Support.V4.App;
using DesignLibrary_Tutorial.Activities;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Android.Runtime;
using DesignLibrary_Tutorial.Handler;
using System.Collections.Generic;
using System;
using AppTestProzesse.Header;
using Android.Graphics;
using System.Linq;

namespace DesignLibrary_Tutorial.Background
{
    [Service]
    public class BackgroundService : IntentService
    {
        private Android.OS.Handler mHandler;
        private MessageHandler mMsgHandler;
        private static readonly int ButtonClickNotificationId = 1000;


        protected override void OnHandleIntent(Intent intent)
        {
            mMsgHandler.UpdateAsync();
            //StartNotification(GetTempData());
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            mHandler = new Android.OS.Handler();
            mMsgHandler = MessageHandler.GetMsgHandler();
            mMsgHandler.OnDataChanged += MMsgHandler_OnDataChanged;
            return base.OnStartCommand(intent, flags, startId);
        }

        private void MMsgHandler_OnDataChanged(object sender, System.EventArgs e)
        {
            var messages = sender as List<LData>;
            if (messages == null)
            {
                //Log Error
            }
            StartNotification(messages);
            //Check MessageArgs for being true
        }

        //private List<LData> GetTempData()
        //{
        //    List<LData> list = new List<LData>();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        list.Add(new LData(DateTime.Now.Date.AddDays(i + 1), new Helpers.CardList(new AppTestProzesse.Header.Subject("DE", "C11", true), new AppTestProzesse.Header.Hours[] { AppTestProzesse.Header.Hours.first, AppTestProzesse.Header.Hours.second })));

        //    }
        //    list.Add(new LData(DateTime.Now.Date.AddDays(8), new Helpers.CardList(new AppTestProzesse.Header.Subject("DE", "C11", true), new AppTestProzesse.Header.Hours[] { AppTestProzesse.Header.Hours.first, AppTestProzesse.Header.Hours.second })));
        //    return list;
        //}

        //public void test()
        //{
        //    var list1 = GetTempData();
        //    var list2 = GetTempData();
        //    list2.RemoveAt(0);
        //    var list3 = list1.Except(list2).ToList();
        //    var list4 = Custom(list1, list2);
        //    var list5 = list1.Where(i => !list2[0].Equals(i)).ToList();
        //}

        private void StartNotification(List<LData> messages)
        {
            // When the user clicks the notification, SecondActivity will start up.
            Intent resultIntent = new Intent(this, typeof(MainActivity));

            // Construct a back stack for cross-task navigation:
            TaskStackBuilder stackBuilder = TaskStackBuilder.Create(this);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
            stackBuilder.AddNextIntent(resultIntent);

            // Create the PendingIntent with the back stack:            
            PendingIntent resultPendingIntent =
                stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

            var config = DataHandler.GetConfig();

            // Build the notification:
            if (messages.Count == 1)
            {
                NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
                    .SetAutoCancel(true)                    // Dismiss from the notif. area when clicked
                    .SetContentIntent(resultPendingIntent)  // Start 2nd activity whenca the intent is clicked.
                    .SetContentTitle("Vertretungsplan")      // Set its title
                    .SetLights(Color.DarkRed, 1500, 1500)
                    .SetColor(Color.DarkRed)
                    .SetContentText(GetTextLine(messages[0])); // The message to display.

                if (config.vibration)
                {
                    builder.SetVibrate(new long[] { 1000, 1000});
                }

                if (messages[0].item.mSubject.omitted)
                {
                    builder.SetSmallIcon(Resource.Drawable.ic_cal_remove);
                }
                else
                {
                    builder.SetSmallIcon(Resource.Drawable.ic_cal_alert);
                }

                NotificationManager notificationManager =
                    (NotificationManager)GetSystemService(Context.NotificationService);
                notificationManager.Notify(ButtonClickNotificationId, builder.Build());
            }
            else if (messages.Count > 1)
            {
                Notification.Builder builder = new Notification.Builder(this)
                    .SetAutoCancel(true)
                    .SetSmallIcon(Resource.Drawable.ic_calendar_multi)
                    .SetContentIntent(resultPendingIntent)
                    .SetLights(Color.DarkRed, 1500, 1500)
                    .SetColor(Color.DarkRed)
                    .SetContentTitle(messages.Count + " neue Nachrichten");

                if (config.vibration)
                {
                    builder.SetVibrate(new long[] { 1000, 1000 });
                }

                //Adding Content Text
                List<string> subjects = new List<string>();
                for (int i = 0; i < messages.Count; i++)
                {
                    if (subjects.Count == 0 || !subjects.Exists(s => s == messages[i].item.mSubject.name))
                    {
                        subjects.Add(messages[i].item.mSubject.name);
                    }
                }
                string contentText;
                if (subjects.Count > 1)
                {
                    contentText = "Betroffen sind: ";
                }
                else
                {
                    contentText = "Betroffen ist: ";
                }
                for (int i = 0; i < subjects.Count - 1; i++)
                {
                    contentText += subjects[i];
                    if (i == messages.Count - 2)
                    {
                        contentText += " und ";
                    }
                    else
                    {
                        contentText += ", ";
                    }
                }
                contentText += messages[messages.Count - 1].item.mSubject.name;
                builder.SetContentText(contentText);

                Notification.InboxStyle inboxStyle = new Notification.InboxStyle();
                for (int i = 0; i < messages.Count; i++)
                {
                    inboxStyle.AddLine(GetTextLine(messages[i]));
                }

                builder.SetStyle(inboxStyle);

                NotificationManager notificationManager =
                    (NotificationManager)GetSystemService(Context.NotificationService);
                notificationManager.Notify(ButtonClickNotificationId, builder.Build());
            }
        }

        private string GetTextLine(LData data)
        {
            string line =
                GetDisplayedDay(data.date) + " " +
                GetDisplayedHour(data.item.h) + ": ";
            if (data.item.mSubject.ev != null)
            {
                line += data.item.mSubject.ev.Describtion;
            }
            else if (data.item.mSubject.omitted)
            {
                line += data.item.mSubject.name + " Entfall";
            }
            else if (data.item.mSubject.change != null)
            {
                line += data.item.mSubject.name + " " +
                    data.item.mSubject.change.type;
                if (data.item.mSubject.change.newSubject != null && !data.item.mSubject.change.newSubject.Contains(data.item.mSubject.name)) //!data.item.mSubject.change.newRoom.Contains("-") && 
                {
                    line += ": Neues Fach ist " + data.item.mSubject.change.newSubject;
                }
                if (data.item.mSubject.change.newRoom != null && data.item.mSubject.change.newRoom != data.item.mSubject.room) //!data.item.mSubject.change.newRoom.Contains("-") && 
                {
                    line += " in Raum " + data.item.mSubject.change.newRoom;
                }
            }
            return line;
        }

        private string GetDisplayedDay(DateTime date)
        {
            DateTime now = DateTime.Now.Date;
            if (date.Date == now)
            {
                return "Heute";
            }
            if (date.Date == now.AddDays(1))
            {
                return "Morgen";
            }
            string nextWeek = "";
            if (date.Date >= now.AddDays(7))
            {
                nextWeek = " [" + date.Day + "." + date.Month + ".]";
            }
            var culture = new System.Globalization.CultureInfo("de-DE");
            return culture.DateTimeFormat.GetDayName(date.DayOfWeek) + nextWeek;
        }

        private string GetDisplayedHour(Hours[] hours)
        {
            if (hours.Length == 1)
            {
                return TimeHandler.HourIndex[(int)hours[0]];
            }
            else if (hours.Length == 2)
            {
                if (hours[0] + 1 == hours[1])
                {
                    return TimeHandler.HourIndex[(int)hours[0]] + "/" + TimeHandler.HourIndex[(int)hours[1]];
                }
                else
                {
                    return TimeHandler.HourIndex[(int)hours[0]] + "-" + TimeHandler.HourIndex[(int)hours[1]];
                }
            }
            return "";
        }
    }
}