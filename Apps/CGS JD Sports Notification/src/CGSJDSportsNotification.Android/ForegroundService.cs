using Android.App;
using Android.Content;
using Android.OS;

namespace CGSJDSportsNotification.Droid {
    [Service]
    class ForegroundService : Service {
        public override IBinder OnBind(Intent intent) { return null; }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
            // Starts the Foreground Service and the notification channel
            StartForeground(9869, new ForegroundServiceNotification().ReturnNotif());

            Android.Widget.Toast.MakeText(Application.Context, "JD Queue - Monitoring started!", Android.Widget.ToastLength.Long).Show();

            JDMonitoring.BackgroundWorker.Helper.Wifi.Acquire();
            // First alarm scheduler
            JDMonitoring.BackgroundWorker.Helper.BackgroundWorkerFire((1000 * 60) * SharedSettings.Entries.Get.Int32("searchRefresh"));

            
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy() {
            Android.Widget.Toast.MakeText(Application.Context, "JD Queue - Monitoring stopped!", Android.Widget.ToastLength.Long).Show();

            JDMonitoring.BackgroundWorker.Helper.BackgroundWorkerStop();

            JDMonitoring.BackgroundWorker.Helper.Wifi.Release();

            SharedSettings.Entries.AddOrEdit.Bool("monitoringIsRunning", false);

            base.OnDestroy();
        }

        public override bool StopService(Intent name) {
            return base.StopService(name);
        }
    }
}