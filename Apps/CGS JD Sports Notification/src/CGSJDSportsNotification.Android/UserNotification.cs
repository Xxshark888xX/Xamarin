using System;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace CGSJDSportsNotification.Droid {
    static class UserNotification {
        const string NOTIFICATION_SERVICE = "notification";

        public static void Remove(int id) {
            NotificationManager.FromContext(Application.Context).Cancel(id);
        }

        public static bool? ChannelCreate(string id, string name, NotificationImportance importance) {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return null;

            if (ChannelExists(id) == false)
                ((NotificationManager)(new ContextWrapper(Application.Context)).GetSystemService(NOTIFICATION_SERVICE)).CreateNotificationChannel(new NotificationChannel(id, name, importance));

            if (ChannelExists(id) == false)
                return false;

            return true;
        }

        public static bool ChannelExists(string id) {
            return ((NotificationManager)(new ContextWrapper(Application.Context)).GetSystemService(NOTIFICATION_SERVICE)).GetNotificationChannel(id) == null ? false : true;
        }
        public static bool ChannelDelete(string id) {
            if (ChannelExists(id))
                ((NotificationManager)(new ContextWrapper(Application.Context)).GetSystemService(NOTIFICATION_SERVICE)).DeleteNotificationChannel(id);

            if (ChannelExists(id))
                return false;

            return true;
        }

        public static void WarningShow(string categoryFlag, string quickTitle, string title, string message, int id = -1) {
            // Opens the main activity on click
            Intent intent = new Intent(Application.Context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);

            NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(Application.Context, "newWarningNotification_channel")
                .SetVisibility((int)NotificationVisibility.Public)
                .SetPriority((int)NotificationPriority.Max)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate | (int)NotificationDefaults.Lights)
                .SetGroup("warning")
                .SetSmallIcon(Resource.Drawable.warningNotification)
                .SetLargeIcon(Android.Graphics.BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.warningNotification))

                .SetSubText("Important Notification")
                .SetContentTitle($"{categoryFlag} - {quickTitle}")
                //.SetContentText(quickTitle)
                .SetStyle(new NotificationCompat.BigTextStyle()
                    .SetBigContentTitle(title)
                    .BigText($"\r\n{message}"))

                .SetAutoCancel(true);

            NotificationCompat.Builder notificationGroupBuilder = new NotificationCompat.Builder(Application.Context, "newWarningNotification_channel")
                .SetStyle(new NotificationCompat.BigTextStyle().SetSummaryText("Warning notifications"))
                .SetSmallIcon(Resource.Drawable.warningNotification)
                .SetGroup("warning")
                .SetGroupSummary(true)

                .SetAutoCancel(true);

            NotificationManager manager = NotificationManager.FromContext(Application.Context);

            manager.Notify(id == -1 ? Guid.NewGuid().GetHashCode() : id, notificationBuilder.Build());
            manager.Notify(0, notificationGroupBuilder.Build());
        }

        public static void TktShow(string tktId, string quickLastUpdate, string title, string message, string link, string icoCountry, int id = -1) {
            int countryFlag = Resource.Drawable.newTktUnknownCountry;

            switch (icoCountry) {
                case "italy":
                    countryFlag = Resource.Drawable.newTktItaly;
                    break;
                case "spain":
                    countryFlag = Resource.Drawable.newTktSpain;
                    break;
                case "germany":
                    countryFlag = Resource.Drawable.newTktGermany;
                    break;
                case "portugal":
                    countryFlag = Resource.Drawable.newTktPortugal;
                    break;
            }

            int notID = id == -1 ? Guid.NewGuid().GetHashCode() : id;

            // Opens the tkt page
            Intent _intent = new Intent(Application.Context, typeof(MainActivity));
            _intent.AddFlags(ActivityFlags.ClearTop);
            _intent.PutExtra("tktLink", link);
            PendingIntent contentIntent = PendingIntent.GetActivity(Application.Context, notID, _intent, 0);

            NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(Application.Context, "newTktNotification_channel")
                .SetVisibility((int)NotificationVisibility.Public)
                .SetPriority((int)NotificationPriority.High)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate | (int)NotificationDefaults.Lights)
                .SetGroup("tkts")
                .SetSmallIcon(Resource.Drawable.newTktNotification)
                .SetLargeIcon(Android.Graphics.BitmapFactory.DecodeResource(Application.Context.Resources, countryFlag))

                .SetSubText("JD Sports ticket available - Tap to check it!")
                .SetContentTitle($"{tktId} - {quickLastUpdate}")
                //.SetContentText($" | {title}")
                .SetStyle(new NotificationCompat.BigTextStyle()
                    .SetBigContentTitle(title)
                    .BigText(message))

                .SetAutoCancel(true)
                .SetContentIntent(contentIntent);

            NotificationCompat.Builder notificationGroupBuilder = new NotificationCompat.Builder(Application.Context, "newTktNotification_channel")
                .SetStyle(new NotificationCompat.BigTextStyle().SetSummaryText("Pending tickets"))
                .SetSmallIcon(Resource.Drawable.newTktNotification)
                .SetGroup("tkts")
                .SetGroupSummary(true)
                .SetAutoCancel(true);

            NotificationManager manager = NotificationManager.FromContext(Application.Context);

            manager.Notify(notID, notificationBuilder.Build());
            manager.Notify(220398, notificationGroupBuilder.Build());
        }
    }
}