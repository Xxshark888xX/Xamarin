using Android.App;
using Android.Content;
using Android.Runtime;
using CGSJDSportsNotification.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(ForegroundServiceHelper))]
namespace CGSJDSportsNotification.Droid {
    class ForegroundServiceHelper : IForegroundService {
        public void Start() {
            if (!IsRunning()) {
                SharedSettings.Entries.AddOrEdit.Bool("monitoringIsRunning", true);

                Intent intent = new Intent(Application.Context, typeof(ForegroundService));
                intent.AddFlags(ActivityFlags.NewTask);

                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O) {
                    Application.Context.StartForegroundService(intent);
                } else {
                    Application.Context.StartService(intent);
                }
            }
        }

        public void Stop() {
            var intent = new Intent(Application.Context, typeof(ForegroundService));
            Application.Context.StopService(intent);
        }

        public bool IsRunning() {
            ActivityManager mngr = (new ContextWrapper(Application.Context)).GetSystemService(Context.ActivityService).JavaCast<ActivityManager>();

            foreach (var service in mngr.GetRunningServices(int.MaxValue))
                if (service.Service.ClassName.Equals(Java.Lang.Class.FromType(typeof(ForegroundService)).CanonicalName))
                    return true;
            
            return false;
        }

        /*public void NotificationNewTicket(string title, string message, string link, string icoCountry, string bigTitle, int id = -1) {
            UserNotification.TktShow(title, message, link, icoCountry, bigTitle, id);
        }
        public void NotificationWarning(string title, string message, string bigTitle, int id = -1) {
            UserNotification.WarningShow(title, message, bigTitle, id);
        }
        public void NotificationRemove(int id) {
            UserNotification.Remove(id);
        }*/
    }
}