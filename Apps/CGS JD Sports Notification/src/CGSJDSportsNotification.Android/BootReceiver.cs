using Android.App;
using Android.Content;

namespace CGSJDSportsNotification.Droid {
    [BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
    [IntentFilter(new string[] {
        Intent.ActionBootCompleted,   
        Intent.ActionLockedBootCompleted,   
        Intent.ActionMyPackageReplaced,
        Intent.ActionUserInitialize,
        "android.intent.action.QUICKBOOT_POWERON",
        "com.htc.intent.action.QUICKBOOT_POWERON",
        },
        Categories = new[] { Intent.CategoryDefault }
    )]
    public class BootReceiver : BroadcastReceiver {
        public override void OnReceive(Context context, Intent intent) {
            if (intent.Action != null) {
                if (intent.Action.Equals(Intent.ActionBootCompleted))
                    StartFgService();
            }
        }

        void StartFgService() {
            if (SharedSettings.Entries.Exists("monitoringRunningByUser") && SharedSettings.Entries.Get.Bool("monitoringRunningByUser")) {
                Intent _intent = new Intent(Application.Context, typeof(ForegroundService));
                _intent.AddFlags(ActivityFlags.NewTask);

                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                    Application.Context.StartForegroundService(_intent);
                else
                    Application.Context.StartService(_intent);
            }
        }
    }
}