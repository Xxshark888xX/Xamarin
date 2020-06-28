using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace CGSJDSportsNotification {
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage {
        IForegroundService FgService { get { return DependencyService.Get<IForegroundService>(); } }
        static MainPage   _mainPage; // Used to stop the foreground service from the notification stop button
        UserSettings      userSettings;

        public MainPage() {
            InitializeComponent();
            _mainPage = this;
            
            new UpdateChecker().FetchLastVersion();

            // App version label
            copyrightAppVersion_lbl.Text += $"{VersionTracking.CurrentVersion} (build {VersionTracking.CurrentBuild})";

            userSettings = new UserSettings();
            userSettings.RequestIgnoreBatteryOptimization();
            userSettings.Load();
        }

        public class UI {
            // Used inside the ForegroundService.Notification.Action
            public static void MonitoringButtonStop() {
                _mainPage.monitoring_btn.Text = "START";

                _mainPage.userSettings.ControlsEnableOrDisable();
            }

            public static async Task<bool> DisplayAlert(string title, string message, string onPositiveClick, string onNegativeClick) {
                return await _mainPage.DisplayAlert(title, message, onPositiveClick, onNegativeClick);
            }
        }

        #region [PAGE_ORIENTATION]
        protected override void OnAppearing() {
            base.OnAppearing();

            MessagingCenter.Send(this, "MainPageOrientation");
        }
        #endregion

        #region [GUI]

        void BackgroundWorkerPolicyButtonOnClick(object sender, EventArgs e) {
            userSettings.RequestIgnoreBatteryOptimization(true);
        }

        void SettingsMonitoringOnClick(object sender, EventArgs e) {
            if (monitoring_btn.Text == "START") {
                if (userSettings.ControlsValidate() == true) {
                    monitoring_btn.Text = "STOP";

                    userSettings.ControlsEnableOrDisable();

                    userSettings.Save();

                    FgService.Start();

                    SharedSettings.Entries.AddOrEdit.Bool("monitoringRunningByUser", true);
                }
            } else {
                monitoring_btn.Text = "START";

                userSettings.ControlsEnableOrDisable();

                FgService.Stop();

                SharedSettings.Entries.AddOrEdit.Bool("monitoringRunningByUser", false);
            }
        }
        void SettingsMonitoringChanged(object sender, EventArgs e) {
            if (monitoring_btn.Text == "START")
                monitoring_btn.BackgroundColor = Color.LightGreen;
            else
                monitoring_btn.BackgroundColor = Color.IndianRed;
        }

        void SearchTimeFrameValueChanged(object sender, ValueChangedEventArgs e) {
            double stepValue = 5.0;

            if (e.OldValue > 60)
                stepValue = 60;
            else if (e.OldValue < 10)
                stepValue = 1.0;

            double newStep = Math.Round(e.NewValue / stepValue);

            searchTimeframe_slide.Value = newStep * stepValue;

            int timeFrame = (int)searchTimeframe_slide.Value;
            if (timeFrame > 60)
                searchTimeFrame_lbl.Text = $"{timeFrame / 60} hour(s)";
            else
                searchTimeFrame_lbl.Text = String.Format("{0} {1}", timeFrame, timeFrame == 1 ? "minute" : "minutes");
        }
        
        void SearchRefreshRateValueChanged(object sender, ValueChangedEventArgs e) {
            double stepValue = 1.0;

            double newStep = Math.Round(e.NewValue / stepValue);

            searchRefreshRate_slide.Value = newStep * stepValue;

            int timeFrame = (int)searchRefreshRate_slide.Value;
            if (timeFrame > 60) {
                int t = timeFrame / 60;
                
                if (timeFrame % 60 == 0)
                    searchRefreshRate_lbl.Text = String.Format("{0} {1}", t, t == 1 ? "hour" : "hours");
                else
                    searchRefreshRate_lbl.Text = String.Format("{0} {1} and {2} minutes", t, t == 1 ? "hour" : "hours", timeFrame % 60);
            } else
                searchRefreshRate_lbl.Text = $"{timeFrame} minutes";
        }

        void DoNotDisturbStartButtonOnClick(object sender, EventArgs e) {
            doNotDisturbStart_timep.Focus();
        }

        void DoNotDisturbStartTimePickerChanged(object sender, EventArgs e) {
            doNotDisturbStart_btn.Text = doNotDisturbStart_timep.Time.ToString(@"hh\:mm");
        }

        void DoNotDisturbEndButtonOnClick(object sender, EventArgs e) {
            doNotDisturbEnd_timep.Focus();
        }

        void DoNotDisturbEndTimePickerChanged(object sender, EventArgs e) {
            doNotDisturbEnd_btn.Text = doNotDisturbEnd_timep.Time.ToString(@"hh\:mm");
        }

        void CountryPickerSelectButtonOnClick(object sender, EventArgs e) {
            countries_picker.Focus();
        }
        void CountryPickerSelectedIndexChanged(object sender, EventArgs e) {
            countryPickerSelect_btn.Text = countries_picker.SelectedItem.ToString();
        }

        #endregion
    }
}