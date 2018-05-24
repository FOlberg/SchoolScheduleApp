using Android.App;
using Android.Content;
using Android.Support.V4.App;
using ScheduleApp.Activities;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Android.Runtime;
using ScheduleApp.Handler;
using System.Collections.Generic;
using System;
using ScheduleApp.Objects;
using Android.Graphics;

namespace ScheduleApp.Background
{
    [Service]
    public class BackgroundService : IntentService
    {
        private MessageHandler mMsgHandler;
        private const int BTN_CLICK_ID  = 1000;
        private const int VIBRATION_ITV = 500;

        protected override void OnHandleIntent(Intent intent)
        {
            mMsgHandler.UpdateAsync();
            //StartNotification(GetTempData()); for debugging
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            mMsgHandler = MessageHandler.GetMsgHandler();
            mMsgHandler.OnDataChanged += MMsgHandler_OnDataChanged;
            return base.OnStartCommand(intent, flags, startId);
        }

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
                    // Dismiss from the notif. area when clicked
                    .SetAutoCancel(true)
                    // Start 2nd activity whenca the intent is clicked.
                    .SetContentIntent(resultPendingIntent)
                    // Set its title
                    .SetContentTitle(GetSingleHeadline(messages[0]))      
                    .SetLights(Color.Indigo, 1500, 1500)
                    .SetColor(Color.Indigo)
                    // The message to display.
                    .SetContentText(GetSingleSubTextLine(messages[0]));

                //Priority must be above vibration
                if (config.mSettings.mPriority) 
                {
                    builder.SetVibrate(new long[0]);
                    builder.SetPriority((int)NotificationPriority.High);
                }
                if (config.mSettings.mVibration)
                    builder.SetVibrate(new long[] { VIBRATION_ITV, VIBRATION_ITV });

                if (messages[0].mItem.mSubject.mOmitted)
                    builder.SetSmallIcon(Resource.Drawable.ic_cal_remove);
                else
                    builder.SetSmallIcon(Resource.Drawable.ic_cal_alert);

                NotificationManager notificationManager =
                    (NotificationManager)GetSystemService(Context.NotificationService);
                notificationManager.Notify(BTN_CLICK_ID, builder.Build());
            }
            else if (messages.Count > 1)
            {
                Notification.Builder builder = new Notification.Builder(this)
                    .SetAutoCancel(true)
                    .SetSmallIcon(Resource.Drawable.ic_calendar_multi)
                    .SetContentIntent(resultPendingIntent)
                    .SetLights(Color.Indigo, 1500, 1500)
                    .SetColor(Color.Indigo)
                    .SetContentTitle(messages.Count + " neue Änderungen");

                //Priority must be above vibration
                if (config.mSettings.mPriority) 
                {
                    builder.SetVibrate(new long[0]);
                    builder.SetPriority((int)NotificationPriority.High);
                }
                if (config.mSettings.mVibration)
                    builder.SetVibrate(new long[] { VIBRATION_ITV, VIBRATION_ITV });

                //Adding Content Text
                List<string> subjects = new List<string>();
                for (int i = 0; i < messages.Count; i++)
                {
                    if (subjects.Count == 0 || !subjects.Exists(s => s == messages[i].mItem.mSubject.mName))
                        subjects.Add(messages[i].mItem.mSubject.mName);
                }

                string contentText;
                if (subjects.Count > 1)
                    contentText = "Betroffen sind ";
                else
                    contentText = "Betroffen ist ";

                for (int i = 0; i < subjects.Count - 1; i++)
                {
                    contentText += subjects[i];
                    if (i == messages.Count - 2)
                        contentText += " und ";
                    else
                        contentText += ", ";
                }
                contentText += messages[messages.Count - 1].mItem.mSubject.mName;
                builder.SetContentText(contentText);

                Notification.InboxStyle inboxStyle = new Notification.InboxStyle();
                for (int i = 0; i < messages.Count; i++)
                {
                    inboxStyle.AddLine(GetSubTextLine(messages[i]));
                }

                builder.SetStyle(inboxStyle);

                NotificationManager notificationManager =
                    (NotificationManager)GetSystemService(NotificationService);
                notificationManager.Notify(BTN_CLICK_ID, builder.Build());
            }
        }

        private string GetSubTextLine(LData data)
        {
            string line = GetDisplayedDay(data.mDate) + " " + GetDisplayedHour(data.mItem.mHour) + ": ";

            if (data.mItem.mSubject.mEvent != null)
                line += data.mItem.mSubject.mEvent.mDescribtion;

            else if (data.mItem.mSubject.mOmitted)
                line += data.mItem.mSubject.mName + " Entfall";

            else if (data.mItem.mSubject.mChange != null)
            {
                line += data.mItem.mSubject.mName + " " + data.mItem.mSubject.mChange.mType;

                if (data.mItem.mSubject.mChange.mNewSubject != null
                    && data.mItem.mSubject.mChange.mNewSubject != ""
                    && !data.mItem.mSubject.mChange.mNewSubject.Contains(data.mItem.mSubject.mName)) 
                    line += ": Neues Fach ist " + data.mItem.mSubject.mChange.mNewSubject;

                if (data.mItem.mSubject.mChange.mNewRoom != null
                    && data.mItem.mSubject.mChange.mNewRoom != ""
                    && !data.mItem.mSubject.mChange.mNewRoom.Contains(data.mItem.mSubject.mRoom))
                    line += " in Raum " + data.mItem.mSubject.mChange.mNewRoom;
            }
            return line;
        }

        private string GetSingleSubTextLine(LData data)
        {
            string line =
                GetDisplayedDay(data.mDate, true) + " " +
                GetDisplayedHour(data.mItem.mHour) + " Stunde";
            if (data.mItem.mSubject.mChange != null)
            {
                if (data.mItem.mSubject.mChange.mNewSubject != null 
                    && data.mItem.mSubject.mChange.mNewSubject != "" 
                    && !data.mItem.mSubject.mChange.mNewSubject.Contains(data.mItem.mSubject.mName))
                    line += ": Neues Fach ist " + data.mItem.mSubject.mChange.mNewSubject;

                if (data.mItem.mSubject.mChange.mNewRoom != null
                    && data.mItem.mSubject.mChange.mNewRoom != ""
                    && !data.mItem.mSubject.mChange.mNewRoom.Contains(data.mItem.mSubject.mRoom))
                    line += " in Raum " + data.mItem.mSubject.mChange.mNewRoom;
            }
            return line;
        }

        private string GetSingleHeadline(LData data)
        {
            if (data.mItem.mSubject.mEvent != null)
                return data.mItem.mSubject.mEvent.mDescribtion;

            if (data.mItem.mSubject.mOmitted)
                return data.mItem.mSubject.mName + " Entfall";

            if (data.mItem.mSubject.mChange != null)
                return data.mItem.mSubject.mName + " " + data.mItem.mSubject.mChange.mType;

            return string.Empty;
        }

        private string GetDisplayedDay(DateTime date, bool singleLine = false)
        {
            DateTime now = DateTime.Now.Date;
            if (date.Date == now)
                return "Heute";

            if (date.Date == now.AddDays(1))
                return "Morgen";

            string nextWeek = "";
            if (date.Date >= now.AddDays(7))
                nextWeek = " [" + date.Day + "." + date.Month + ".]";

            string preposition  = singleLine ? "Am " : "";
            var culture         = new System.Globalization.CultureInfo("de-DE");
            return preposition + culture.DateTimeFormat.GetDayName(date.DayOfWeek) + nextWeek;
        }

        private string GetDisplayedHour(Hours[] hours)
        {
            if (hours.Length == 1)
                return TimeHandler.HourIndex[(int)hours[0]] + ".";

            else if (hours.Length == 2)
            {
                if (hours[0] + 1 == hours[1])
                    return TimeHandler.HourIndex[(int)hours[0]] + "/" + TimeHandler.HourIndex[(int)hours[1]];
                return TimeHandler.HourIndex[(int)hours[0]] + "-" + TimeHandler.HourIndex[(int)hours[1]];
            }
            return "";
        }

        //Can be optimized or shortend
        private void MMsgHandler_OnDataChanged(object sender, EventArgs e)
        {
            var messages = sender as List<LData>;
            if (messages == null)
            {
                //Log Error
            }
            try
            {
                //var arg = e as MessageArgs;
                //if (arg.EmptyList)
                //{
                //    //testS = "*";
                //}
            }
            catch (Exception) { }
            StartNotification(messages);
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
    }
}