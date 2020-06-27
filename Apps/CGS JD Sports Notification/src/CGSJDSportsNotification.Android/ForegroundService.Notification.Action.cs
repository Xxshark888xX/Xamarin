using Android.App;
using Android.Content;
using Xamarin.Forms;

namespace CGSJDSportsNotification.Droid {
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "NOTIFICATION_MONITORING_STOP" })]
    public class ForegroundServiceNotificationAction : BroadcastReceiver {
        public override void OnReceive(Context context, Intent intent) {
            // ### Simulates the "STOP" button on the MainPage ### \\

            DependencyService.Get<IForegroundService>().Stop();

            SharedSettings.Entries.AddOrEdit.Bool("monitoringRunningByUser", false);

            // Used in order to change the button text and to enable the controls
            // Placed inside a try-catch because if the device was restarted, the service started without the GUI being initialized, thus an exception will be raised without the try-catch block
            try { MainPage.UI.MonitoringButtonStop(); } catch { }
        }
    }
}