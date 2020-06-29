using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CGSJDSportsNotification {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JDQueuePage : ContentPage {
        const string jdQueueUrl = "https://support.jdplc.com/rt4/Search/Results.html?Format=%27%3Cb%3E%3Ca%20href%3D%22__WebPath__%2FTicket%2FDisplay.html%3Fid%3D__id__%22%3E__id__%3C%2Fa%3E%3C%2Fb%3E%2FTITLE%3A%23%27%2C%0A%27%3Cb%3E%3Ca%20href%3D%22__WebPath__%2FTicket%2FDisplay.html%3Fid%3D__id__%22%3E__Subject__%3C%2Fa%3E%3C%2Fb%3E%2FTITLE%3ASubject%27%2C%0AStatus%2C%0AQueueName%2C%0AOwner%2C%0APriority%2C%0A%27__NEWLINE__%27%2C%0A%27__NBSP__%27%2C%0A%27%3Csmall%3E__Requestors__%3C%2Fsmall%3E%27%2C%0A%27%3Csmall%3E__CreatedRelative__%3C%2Fsmall%3E%27%2C%0A%27%3Csmall%3E__ToldRelative__%3C%2Fsmall%3E%27%2C%0A%27%3Csmall%3E__LastUpdatedRelative__%3C%2Fsmall%3E%27%2C%0A%27%3Csmall%3E__TimeLeft__%3C%2Fsmall%3E%27&Order=DESC%7CASC%7CASC%7CASC&OrderBy=LastUpdated%7C%7C%7C&Query=Queue%20%3D%20%27Service%20Desk%20-%20CGS%27%20AND%20(%20%20Status%20%3D%20%27new%27%20OR%20Status%20%3D%20%27open%27%20OR%20Status%20%3D%20%27stalled%27%20OR%20Status%20%3D%20%27deferred%27%20OR%20Status%20%3D%20%27open%20-%20awaiting%20requestor%27%20OR%20Status%20%3D%20%27open%20-%20awaiting%20third%20party%27%20)&RowsPerPage=0&SavedChartSearchId=new&SavedSearchId=new";
        // Used to check if we are on the main queue, if the user will press the back button will exit the browswer, otherwhise will go one page back
        bool jdQueueMainUrl = false;
        // Used to check if the webview loaded only the tktUrl in order to be able to go on the MainPage when the user clicks the back button
        bool firstLoad = true;
        bool canExit = false;

        public JDQueuePage(string tktUrl) {
            InitializeComponent();

            notificationJdqueue_webview.Source = tktUrl;
        }

        protected override void OnAppearing() {
            base.OnAppearing();

            MessagingCenter.Send(this, "JDQueuePageOrientation");
        }

        protected override bool OnBackButtonPressed() {
            base.OnBackButtonPressed();
            bool exit = false;

            if (jdQueueMainUrl || firstLoad == true) {
                if (Navigation.NavigationStack.Count == 1)
                    exit = true;
                else
                    Navigation.RemovePage(this);
            } else {
                if (canExit == false)
                    if (notificationJdqueue_webview.CanGoBack)
                        notificationJdqueue_webview.GoBack();
                    else
                        notificationJdqueue_webview.Source = jdQueueUrl;
                else
                    exit = true;
            }

            if (exit)
                return false;

            return true;
        }

        async void PageOnNavigated(object sender, WebNavigatedEventArgs args) {
            if (args.Url.Contains("Login.html") || args.Url.Equals("https://support.jdplc.com/rt4/")) {
                canExit = true;

                await notificationJdqueue_webview.EvaluateJavaScriptAsync($"document.getElementsByName('user')[0].value = '{SharedSettings.SecureEntries.Get("rtUser")}'");
                await notificationJdqueue_webview.EvaluateJavaScriptAsync($"document.getElementsByName('pass')[0].value = '{SharedSettings.SecureEntries.Get("rtPass")}'");

                await notificationJdqueue_webview.EvaluateJavaScriptAsync(@"document.getElementsByClassName('button')[0].click()");
            } else if (args.Url.Equals("https://support.jdplc.com/rt4/ServicePortal.html")) {
                notificationJdqueue_webview.Source = jdQueueUrl;
            } else {
                canExit = false;

                if (firstLoad == true)
                    firstLoad = false;

                if (args.Url.StartsWith("https://support.jdplc.com/rt4/Search/Results.html")) {
                    canExit = true;
                    jdQueueMainUrl = true;
                } else
                    jdQueueMainUrl = false;
            }
        }
    }
}