using Xamarin.Forms;

namespace CGSJDSportsNotification {
    public partial class App : Application {
        public App(string tktUrl = "") {
            InitializeComponent();

            // When the MainPage is closed and the user clicks on a new tkt notification, will load directly the jd tkt page
            MainPage = tktUrl == "" ? new NavigationPage(new MainPage()) : new NavigationPage(new JDQueuePage(tktUrl));
        }

        protected override void OnStart() {
        }

        protected override void OnSleep() {
        }

        protected override void OnResume() {
        }
    }
}