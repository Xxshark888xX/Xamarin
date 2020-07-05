using Android.App;
using Android.Content;
using Xamarin.Forms;

namespace CGSJDSportsNotification.Droid {
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "NOTIFICATION_MONITORING_STOP" })]
    public class ForegroundServiceNotificationAction : BroadcastReceiver {
        public override void OnReceive(Context context, Intent intent) {
            // ### Simulates the "STOP" button on the MainPage ### \\

            // Firstly initializes the Forms, otherwise when calling the DependencyService class the app will crash
            if (Forms.IsInitialized == false)
                Forms.Init(Android.App.Application.Context, new Android.OS.Bundle());

            DependencyService.Get<IForegroundService>().Stop();

            SharedSettings.Entries.AddOrEdit.Bool("monitoringRunningByUser", false);

            // Used in order to change the button text and to enable the controls
            MainPage.UI.MonitoringButtonStop();
        }
    }
}