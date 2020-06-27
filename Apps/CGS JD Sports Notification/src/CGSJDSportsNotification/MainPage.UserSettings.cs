using Android.Content;
using Android.OS;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CGSJDSportsNotification {
    public partial class MainPage {
        class UserSettings {
            IForegroundService FgService { get { return DependencyService.Get<IForegroundService>(); } }

            public async void RequestIgnoreBatteryOptimization(bool bypassPreviousUserDecline = false) {
                if ((SharedSettings.Entries.Get.Bool("aospBgWorkerPolicyAccepted") || SharedSettings.Entries.Get.Bool("oemBgWorkerPolicyAccepted")) && bypassPreviousUserDecline == false)
                    return;

                // AOSP Battery optimization (doze)
                Intent intent = new Intent();
                PowerManager pm = (PowerManager)new ContextWrapper(Android.App.Application.Context).GetSystemService(Context.PowerService);
                // OEM Battery optimization app
                OEMBatteryWhitelist oemBatteryApp = new OEMBatteryWhitelist(Android.App.Application.Context);

                bool policyAccepted = true;

                if (pm.IsIgnoringBatteryOptimizations(AppInfo.PackageName) == false && Build.VERSION.SdkInt >= BuildVersionCodes.M) {
                    if (await _mainPage.DisplayAlert(
                        "Background Worker Policy",
                        "In order to be able to always fetch the tickets while your phone is in sleep-mode, you must disable the 'Battery Optimization' feature for this app",
                        "ACCEPT",
                        "DECLINE")
                        ) {
                        intent.SetFlags(ActivityFlags.NewTask);
                        intent.SetAction(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                        intent.SetData(Android.Net.Uri.Parse("package:" + AppInfo.PackageName));
                        Android.App.Application.Context.StartActivity(intent);
                    } else
                        policyAccepted = false;
                } else if(pm.IsIgnoringBatteryOptimizations(AppInfo.PackageName) == true && oemBatteryApp.CanRequestPermissions == false && bypassPreviousUserDecline == true) {
                    await _mainPage.DisplayAlert(
                        "Background Worker Policy",
                        "AOSP Battery Optimization already disabled",
                        "OK");
                }

                if (oemBatteryApp.CanRequestPermissions && (SharedSettings.Entries.Get.Bool("oemBgWorkerPolicyAccepted") == false || bypassPreviousUserDecline == true)) {
                    if (policyAccepted == true) {
                        if (await _mainPage.DisplayAlert(
                            "Background Worker Policy",
                            $"Looks like your phone manufacturer ({Build.Brand}) uses one more battery optimization app in order to prevent 3rd party apps to work in the background\r\n\r\n" +
                            "Please, tap 'ACCEPT' to open the app power settings and be sure to disable it for this app\r\n\r\n" +
                            "Beware that if you don't disable this restriction, the app WILL NOT work while your phone is in sleep-mode",
                            "ACCEPT",
                            "DECLINE")) {

                            Exception batteryWhitelistEx;

                            if ((batteryWhitelistEx = oemBatteryApp.RequestPermissions()) != null) {
                                SharedSettings.Entries.AddOrEdit.Bool("oemBgWorkerPolicyAccepted", true);
                                policyAccepted = false;

                                await _mainPage.DisplayAlert(
                                    "Background Worker Policy",
                                    "Unable to open the OEM battery optimization app...\r\n" +
                                    "Please send a screenshot with this error to Adi\r\n\r\n" +
                                    $"Exception: {batteryWhitelistEx.InnerException}\r\n\r\n" +
                                    $"{batteryWhitelistEx.Message}",
                                    "CONTINUE");
                            }
                        } else
                            policyAccepted = false;
                    }
                }

                if (policyAccepted == false)
                    await _mainPage.DisplayAlert(
                            "Background Worker Policy",
                            "You didn't accept the policy, thus the app may not work while in sleep-mode\r\n\r\n" +
                            "If you change your mind, just press the 'BACKGROUND WORKER POLICY' button in the main menu'",
                            "UNDERSTOOD");

                // If policy is set to true, the user did not declined the policy (so aospBgWorkerPolicyAccepted will be false)
                SharedSettings.Entries.AddOrEdit.Bool("aospBgWorkerPolicyAccepted", policyAccepted);
                SharedSettings.Entries.AddOrEdit.Bool("oemBgWorkerPolicyAccepted", policyAccepted);
            }

            public void Load() {
                if (SharedSettings.Entries.Exists("notFirstStart")) {
                    _mainPage.rtUser_entry.Text                    = SharedSettings.SecureEntries.Get("rtUser");
                    _mainPage.rtPass_entry.Text                    = SharedSettings.SecureEntries.Get("rtPass");
                    _mainPage.searchTimeframe_slide.Value          = SharedSettings.Entries.Get.Int32("searchTimeframe");
                    _mainPage.countries_picker.SelectedItem        = SharedSettings.Entries.Get.String("searchCountrySelected");
                    _mainPage.searchRefreshRate_slide.Value        = SharedSettings.Entries.Get.Int32("searchRefresh");
                    _mainPage.searchUnknownCountry_chbox.IsToggled = SharedSettings.Entries.Get.Bool("searchUnknownCountry");
                    _mainPage.appAutoUpdate_chbox.IsToggled        = SharedSettings.Entries.Get.Bool("appAutoUpdate");

                    // (bool)monitoringIsRunning
                    // (bool)monitoringRunningByUser - used for the boot action complete

                    if (SharedSettings.Entries.Get.Bool("monitoringIsRunning")) {
                        FgService.Start();

                        _mainPage.monitoring_btn.Text = "STOP";
                        ControlsEnableOrDisable(); ;
                    }
                }
            }

            public void Save() {
                if (SharedSettings.Entries.Exists("notFirstStart") == false)
                    SharedSettings.Entries.AddOrEdit.Bool("notFirstStart", true);

                if (((PowerManager)new ContextWrapper(Android.App.Application.Context).GetSystemService(Context.PowerService)).IsIgnoringBatteryOptimizations(AppInfo.PackageName) == false)
                    SharedSettings.Entries.AddOrEdit.Bool("aospBgWorkerPolicyAccepted", true);
                    

                for (int i = 0; i < _mainPage.countries_picker.Items.Count; i++)
                    SharedSettings.Entries.AddOrEdit.String($"searchCountryItem[{i}]", _mainPage.countries_picker.Items[i]);

                SharedSettings.SecureEntries.AddOrEdit("rtUser", _mainPage.rtUser_entry.Text);
                SharedSettings.SecureEntries.AddOrEdit("rtPass", _mainPage.rtPass_entry.Text);
                SharedSettings.Entries.AddOrEdit.Int32("searchTimeframe", (int)_mainPage.searchTimeframe_slide.Value);
                SharedSettings.Entries.AddOrEdit.String("searchCountrySelected", _mainPage.countries_picker.SelectedItem.ToString());
                SharedSettings.Entries.AddOrEdit.Int32("searchRefresh", (int)_mainPage.searchRefreshRate_slide.Value);
                SharedSettings.Entries.AddOrEdit.Bool("searchUnknownCountry", _mainPage.searchUnknownCountry_chbox.IsToggled);
                SharedSettings.Entries.AddOrEdit.Bool("appAutoUpdate", _mainPage.appAutoUpdate_chbox.IsToggled);

            }

            #region [CONTROLS]
            public bool ControlsValidate() {
                if (string.IsNullOrEmpty(_mainPage.rtUser_entry.Text) ||
                    string.IsNullOrEmpty(_mainPage.rtPass_entry.Text) ||
                    _mainPage.countries_picker.SelectedIndex == -1) {


                    _mainPage.DisplayAlert("[WARNING]", "You must complete all the fields...", "OK");

                    return false;
                }
                
                return true;
            }

            public void ControlsEnableOrDisable() {
                _mainPage.rtUser_entry.IsEnabled               = !_mainPage.rtUser_entry.IsEnabled;
                _mainPage.rtPass_entry.IsEnabled               = !_mainPage.rtPass_entry.IsEnabled;
                _mainPage.searchTimeframe_slide.IsEnabled      = !_mainPage.searchTimeframe_slide.IsEnabled;
                _mainPage.countryPickerSelect_btn.IsEnabled    = !_mainPage.countryPickerSelect_btn.IsEnabled;
                _mainPage.searchRefreshRate_slide.IsEnabled    = !_mainPage.searchRefreshRate_slide.IsEnabled;
                _mainPage.searchUnknownCountry_chbox.IsEnabled = !_mainPage.searchUnknownCountry_chbox.IsEnabled;
                _mainPage.backgroundworkerPolicy_btn.IsEnabled = !_mainPage.backgroundworkerPolicy_btn.IsEnabled;
            }
            #endregion
        }
    }
}
