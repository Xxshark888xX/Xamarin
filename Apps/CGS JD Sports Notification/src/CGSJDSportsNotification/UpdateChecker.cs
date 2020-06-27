using System;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CGSJDSportsNotification {
    public class UpdateChecker {
        static IForegroundService FgService { get { return DependencyService.Get<IForegroundService>(); } }

        const string ERR_NO_INTERNET  = "ERR::INTERNET_DISCONNECTED";
        const string URL_FORCE_UPDATE = "https://raw.githubusercontent.com/Xxshark888xX/Xamarin/master/Apps/CGS%20JD%20Sports%20Notification/UpdateChecker/force.update";
        const string URL_FILE_VERSION = "https://raw.githubusercontent.com/Xxshark888xX/Xamarin/master/Apps/CGS%20JD%20Sports%20Notification/UpdateChecker/app.version";
        const string URL_APK          = "https://raw.githubusercontent.com/Xxshark888xX/Xamarin/master/Apps/CGS%20JD%20Sports%20Notification/UpdateChecker/apk.link";

        static WebClient wc;

        public UpdateChecker() {
            wc = new WebClient();
            wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            wc.Headers.Add("Cache-Control", "no-cache");
        }

        class Helper {
            public static async Task<string> DownloadStringAsync(string url, bool noCache = true) {
                if (noCache)
                    return await wc.DownloadStringTaskAsync($"{url}?nocache={DateTime.UtcNow}");
                else
                    return await wc.DownloadStringTaskAsync(url);
            }
            public static string DownloadString(string url, bool noCache = true) {
                if (noCache)
                    return wc.DownloadString($"{url}?nocache={DateTime.UtcNow}");
                else
                    return wc.DownloadString(url);
            }

            public class FileParser {
                string fileVersion;

                public FileParser() {
                    try {
                        fileVersion = DownloadString(URL_FILE_VERSION);
                    } catch {
                        fileVersion = ERR_NO_INTERNET;
                    }
                }

                public string GetValue(string id) {
                    if (fileVersion == ERR_NO_INTERNET)
                        return ERR_NO_INTERNET;

                    id = $"[{id}]";

                    int sectionIndex = fileVersion.IndexOf(id);

                    if (sectionIndex == -1)
                        return null;

                    string fv = fileVersion;

                    fv = fv.Substring(sectionIndex + id.Length + Environment.NewLine.Length);
                    if (fv.Substring(0, 2) == "<#") { // Long string
                        fv = fv.Substring(2, fv.IndexOf("#>") - 2);
                    } else
                        fv = fv.Substring(0, fv.IndexOf(Environment.NewLine));

                    return fv;
                }
            }
        }

        public async void FetchLastVersion() {
            // Used during the first launch of the app
            if (SharedSettings.Entries.Exists("appAutoUpdate") == false)
                SharedSettings.Entries.AddOrEdit.Bool("appAutoUpdate", true);

            bool forceUpdate;
            // Force update will bypass the user preference about receiving the 'New Update' DialogAlert if an update it's marked as important
            try { forceUpdate = Convert.ToBoolean(await Helper.DownloadStringAsync(URL_FORCE_UPDATE)); } catch { return; } // Used for when the device does not have any internet connection

            if (SharedSettings.Entries.Get.Bool("appAutoUpdate") == true || forceUpdate == true) {
                Helper.FileParser fParser = new Helper.FileParser();

                // Check connection status
                if (fParser.GetValue("version") == ERR_NO_INTERNET)
                    return;

                string currentVersion = VersionTracking.CurrentVersion;
                string currentBuild   = VersionTracking.CurrentBuild;

                string timestamp      = fParser.GetValue("time_stamp");
                string newVersion     = fParser.GetValue("version");
                string newBuild       = fParser.GetValue("build");
                bool eraseSettings    = Convert.ToBoolean(fParser.GetValue("erase_settings"));
                string changeLog      = fParser.GetValue("change_log");

                if (newVersion != currentVersion || newBuild != currentBuild) {
                    if (await MainPage.UI.DisplayAlert(
                        forceUpdate == true ? "New IMPORTANT Update Available!" : "New Update Available!",
                        $"{timestamp}\r\n\r\n" +
                        "[VERSION]\r\n" +
                        $" > {newVersion} (installed: {currentVersion})" +
                        "\r\n\r\n" +
                        "[BUILD]\r\n" +
                        $" > {newBuild} (current: {currentBuild})" +
                        "\r\n\r\n\r\n" +
                        "[CHANGE LOG]" +
                        "\r\n\r\n" +
                        $"{changeLog}",
                        "DOWNLOAD", "LATER")) 
                    {
                        Device.OpenUri(new Uri(await Helper.DownloadStringAsync(URL_APK)));

                        if (FgService.IsRunning()) {
                            FgService.Stop();
                            try { MainPage.UI.MonitoringButtonStop(); } catch { }
                        }

                        if (eraseSettings)
                            SharedSettings.ClearAll();
                    }
                }
            }
        }
    }
}