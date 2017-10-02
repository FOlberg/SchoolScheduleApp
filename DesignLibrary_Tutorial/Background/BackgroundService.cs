using Android.App;
using Android.Content;
using Android.Support.V4.App;
using DesignLibrary_Tutorial.Activities;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Android.Runtime;
using DesignLibrary_Tutorial.Handler;

namespace DesignLibrary_Tutorial.Background
{
    [Service]
    public class BackgroundService : IntentService
    {
        private Android.OS.Handler mHandler;
        private MessageHandler mMsgHandler;
        private static readonly int ButtonClickNotificationId = 1000;
        private static int count = 0;


        protected override void OnHandleIntent(Intent intent)
        {
            
            mMsgHandler.UpdateAsync();
            StartNotification();
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
            StartNotification();
        }

        public void StartNotification()
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

            // Build the notification:
            NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
                .SetAutoCancel(true)                    // Dismiss from the notif. area when clicked
                .SetContentIntent(resultPendingIntent)  // Start 2nd activity whenca the intent is clicked.
                .SetContentTitle("Content Title")      // Set its title
                .SetNumber(count++)                       // Display the count in the Content Info
                .SetSmallIcon(Resource.Drawable.ic_calendar_multi) // Display this icon
                .SetContentText("Content Text"); // The message to display.

            // Finally, publish the notification:
            NotificationManager notificationManager =
                (NotificationManager)GetSystemService(Context.NotificationService);
            notificationManager.Notify(ButtonClickNotificationId, builder.Build());

        }
    }
}