using Android.App;
using Android.Content;
using AndroidX.Core.App;
using CGSJDSportsNotification.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(ForegroundServiceNotification))]
namespace CGSJDSportsNotification.Droid {
    class ForegroundServiceNotification {
        const string channelID = "foregroundServiceNotification_channel";

        public Notification ReturnNotif() {
            // Opens the main activity on click
            Intent activityIntent = new Intent(Application.Context, typeof(MainActivity));
            activityIntent.AddFlags(ActivityFlags.SingleTop);
            PendingIntent activityPendingIntent = PendingIntent.GetActivity(Application.Context, 0, activityIntent, PendingIntentFlags.UpdateCurrent);

            // Stop Monitoring button action
            Intent stopIntent = new Intent(Application.Context, typeof(ForegroundServiceNotificationAction));
            stopIntent.SetAction("NOTIFICATION_MONITORING_STOP");
            PendingIntent stopPendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, stopIntent, 0);

            var notifBuilder = new NotificationCompat.Builder(Application.Context, channelID)
                    .SetVisibility((int)NotificationVisibility.Public)
                    .SetPriority((int)NotificationPriority.Min)
                    .SetShowWhen(false)

                    .SetSmallIcon(Resource.Drawable.foregroundServiceNotificationIcon)
                    .SetLargeIcon(Android.Graphics.BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.foregroundServiceNotificationIcon))

                    .SetContentTitle("Monitoring the JD Queue")

                    .AddAction(Resource.Drawable.newTktStopMonitoring, "STOP", stopPendingIntent)

                    .SetProgress(0, 0, true)
                    .SetOngoing(true)
                    .SetContentIntent(activityPendingIntent);

            // Channel required after API 26
            if (UserNotification.ChannelExists(channelID) == false)
                UserNotification.ChannelCreate(channelID, "Foreground Service Notification Channel", NotificationImportance.Min);

            return notifBuilder.Build();
        }
    }
}