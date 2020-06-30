using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Forms;
using Android.Content;
using System;

[assembly: Dependency(typeof(CGSJDSportsNotification.Droid.MainActivity))]
namespace CGSJDSportsNotification.Droid {
    [Activity(Label = "CGS - JD Sports Queue Notification", Icon = "@drawable/appIcon", Theme = "@style/MainTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTask, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.User)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        string JDQueueTktURL { get; set; }

        protected override void OnCreate(Bundle savedInstanceState) {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);

            MessagingCenter.Subscribe<JDQueuePage>(this, "JDQueuePageOrientation", sender => {
                RequestedOrientation = ScreenOrientation.UserLandscape;
            });

            MessagingCenter.Subscribe<MainPage>(this, "MainPageOrientation", sender => {
                RequestedOrientation = ScreenOrientation.User;
            });

            JDQueueTktURL = Intent.GetStringExtra("tktLink");

            // When the MainPage is closed and the user clicks on a new tkt notification, will load directly the jd tkt page
            LoadApplication(JDQueueTktURL == null ? new App() : new App(JDQueueTktURL));

            NotificationCreateChannels();
        }

        protected override void OnNewIntent(Intent intent) {
            base.OnNewIntent(intent);

            JDQueueTktURL = intent.GetStringExtra("tktLink");

            if (JDQueueTktURL != null) {
                Xamarin.Forms.Application.Current.MainPage.Navigation.PushAsync(new JDQueuePage(JDQueueTktURL));
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults) {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void NotificationCreateChannels() {
            UserNotification.ChannelCreate("newTktNotification_channel", "JD Sports Queue Channel", NotificationImportance.High);
            UserNotification.ChannelCreate("newWarningNotification_channel", "Application Related Channel", NotificationImportance.Max);
        }
    }
}