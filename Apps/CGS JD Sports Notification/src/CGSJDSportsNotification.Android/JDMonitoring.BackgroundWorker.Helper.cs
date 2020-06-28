using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CGSJDSportsNotification.Droid {
    public partial class JDMonitoring {
        public partial class BackgroundWorker {
            public class Helper {
                public IForegroundService FgService { get { return DependencyService.Get<IForegroundService>(); } }

                const string URL_LOGIN_PAGE = "https://support.jdplc.com/rt4/NoAuth/Login.html";
                const string URL_QUEUE_PAGE = "https://support.jdplc.com/rt4/Search/Results.html?Format=%27%3Cb%3E%3Ca%20href%3D%22__WebPath__%2FTicket%2FDisplay.html%3Fid%3D__id__%22%3E__id__%3C%2Fa%3E%3C%2Fb%3E%2FTITLE%3A%23%27%2C%0A%27%3Cb%3E%3Ca%20href%3D%22__WebPath__%2FTicket%2FDisplay.html%3Fid%3D__id__%22%3E__Subject__%3C%2Fa%3E%3C%2Fb%3E%2FTITLE%3ASubject%27%2C%0AStatus%2C%0AQueueName%2C%0AOwner%2C%0APriority%2C%0A%27__NEWLINE__%27%2C%0A%27__NBSP__%27%2C%0A%27%3Csmall%3E__Requestors__%3C%2Fsmall%3E%27%2C%0A%27%3Csmall%3E__CreatedRelative__%3C%2Fsmall%3E%27%2C%0A%27%3Csmall%3E__ToldRelative__%3C%2Fsmall%3E%27%2C%0A%27%3Csmall%3E__LastUpdatedRelative__%3C%2Fsmall%3E%27%2C%0A%27%3Csmall%3E__TimeLeft__%3C%2Fsmall%3E%27&Order=DESC%7CASC%7CASC%7CASC&OrderBy=LastUpdated%7C%7C%7C&Query=Queue%20%3D%20%27Service%20Desk%20-%20CGS%27%20AND%20(%20%20Status%20%3D%20%27new%27%20OR%20Status%20%3D%20%27open%27%20OR%20Status%20%3D%20%27stalled%27%20OR%20Status%20%3D%20%27deferred%27%20OR%20Status%20%3D%20%27open%20-%20awaiting%20requestor%27%20OR%20Status%20%3D%20%27open%20-%20awaiting%20third%20party%27%20)&RowsPerPage=0&SavedChartSearchId=new&SavedSearchId=new";

                public static Context AndroidContext { get { return Android.App.Application.Context; } }
                public static WifiManager.WifiLock Wifi { get; } = ((WifiManager)(new ContextWrapper(AndroidContext)).GetSystemService(Context.WifiService)).CreateWifiLock(Android.Net.WifiMode.Full, "WifiLock");
                public string UrlLoginPage { get { return URL_LOGIN_PAGE; } }
                public string UrlQueuePage { get { return URL_QUEUE_PAGE; } }

                int refreshRate = 0;
                public int RefreshRate {
                    get { return refreshRate; }
                    set { refreshRate = 1000 * value; }
                }
                public int ProcessPhase { get; set; } = 0;
                public bool InDepthProcess { get; set; } = false;

                public Browser browser;
                public Browser.CustomWebViewClient wc;

                public Helper(int refreshRateSec) {
                    if (Forms.IsInitialized == false)
                        Forms.Init(AndroidContext, new Bundle());

                    RefreshRate = refreshRateSec;
                }

                public void Dispose() {
                    BackgroundWorkerStop();
                    FreeMemory();
                }

                public async Task<bool> IsDoNotDisturbeTime() {
                    TimeSpan doNotDisturbTimeStart = TimeSpan.Parse(SharedSettings.Entries.Get.String("doNotDisturbStart"));
                    TimeSpan doNotDisturbTimeEnd   = TimeSpan.Parse(SharedSettings.Entries.Get.String("doNotDisturbEnd"));
                    TimeSpan timeNow               = DateTime.Now.TimeOfDay;

                    if (timeNow >= doNotDisturbTimeStart && timeNow < doNotDisturbTimeEnd) {
                        // If the current time is between the do not disturb timespan, the alarm will be stopped and scheduled to start at the end of the do not disturb end value
                        BackgroundWorkerReset(((doNotDisturbTimeEnd - timeNow).Minutes + 1) * 60);

                        return true;
                    }

                    return false;
                }

                public async Task<bool> BrowserInit() {
                    Device.BeginInvokeOnMainThread(() => {
                        browser = new Browser(wc);
                    });

                    while (browser == null)
                        await Task.Delay(100);

                    return true;
                }
                public void FreeMemory() {
                    Device.BeginInvokeOnMainThread(() => {
                        try {
                            browser.WB.ClearHistory();
                            browser.WB.ClearCache(false);
                            browser.WB.LoadUrl("about:blank");
                            browser.WB.OnPause();
                            browser.WB.RemoveAllViews();
                            browser.WB.Destroy();
                            browser.WB.Dispose();
                            browser.WB = null;

                            wc.Dispose();
                            wc = null;

                            browser = null;

                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            GC.WaitForFullGCComplete();
                            GC.Collect();
                        } catch { }
                    });
                }

                public static void BackgroundWorkerFire(int alarmMilliseconds) {
                    ((AlarmManager)(new ContextWrapper(AndroidContext)).GetSystemService(Context.AlarmService))
                        .SetExactAndAllowWhileIdle(
                            AlarmType.ElapsedRealtimeWakeup,
                            SystemClock.ElapsedRealtime() + alarmMilliseconds,
                            PendingIntent.GetBroadcast(
                                AndroidContext,
                                98,
                                new Intent(
                                    AndroidContext,
                                    typeof(JDMonitoring.BackgroundWorker)),
                                PendingIntentFlags.UpdateCurrent));
                }

                public void BackgroundWorkerReset(int seconds = -1) {
                    BackgroundWorkerStop();
                    BackgroundWorkerFire(seconds == -1 ? RefreshRate : 1000 * seconds);
                }

                public static void BackgroundWorkerStop() {
                    ((AlarmManager)(new ContextWrapper(AndroidContext)).GetSystemService(Context.AlarmService))
                        .Cancel(PendingIntent.GetBroadcast(
                                AndroidContext,
                                98,
                                new Intent(
                                    AndroidContext,
                                    typeof(JDMonitoring.BackgroundWorker)),
                                PendingIntentFlags.UpdateCurrent));
                }

                public bool IsConnectionAvailable() {
                    if (CrossConnectivity.Current.IsConnected)
                        return true;

                    return false;
                }

                public async Task<bool> IsFgServiceActive() {
                    if (FgService.IsRunning() == false) {
                        Dispose();

                        return false;
                    }

                    return true;
                }

                public async Task<bool?> Login() {
                    if (await JDServerIsOk() == false || await CheckNetConnection() == false)
                        return null;

                    // Inserts the RT username
                    await browser.EvalJS($"document.getElementsByName('user')[0].value = '{SharedSettings.SecureEntries.Get("rtUser")}'");
                    // Inserts the RT password
                    await browser.EvalJS($"document.getElementsByName('pass')[0].value = '{SharedSettings.SecureEntries.Get("rtPass")}'");
                    // Clicks the log-in button
                    await browser.EvalJS("document.getElementsByClassName('button')[0].click()");

                    // If this element exists (wrong credentials error) the log-in failed
                    if (await browser.ElementExists("document.getElementsByClassName('action-results')[0].innerText", "your username or password is incorrect", true, 5)) {
                        UserNotification.WarningShow(
                            "[RT]",
                            "Login failed...",
                            "Username or password could be wrong",
                            "If the problem persists, please send a DM to Adi\r\n\r\n" +
                            "The app wil retry again as soon as possible",
                            12029);

                        return false;
                    }

                    return true;
                }

                public async Task<bool?> IsOnLoginPage() {
                    if (await JDServerIsOk() == false || await Net_Or_FgServiceAreNotActive())
                        return null;

                    // Checks for the 'Login' label
                    if (await browser.ElementExists("document.getElementById('header').textContent", "Login", false, 5))
                        return true;

                    return false;
                }

                public async Task<List<Ticket>> GetTickets() {
                    List<Ticket> tkts = new List<Ticket>();

                    if (await JDServerIsOk() == false)
                        return tkts;

                    // Queue tkts index (multiple of 2)
                    int index = 2;

                    while (await browser.ElementExists($"document.getElementsByClassName('ticket-list collection-as-table')[0].getElementsByTagName('tbody')[0].getElementsByTagName('tr')[{index}].textContent")) {
                        Ticket tkt = new Ticket();

                        tkt.LastUpdated = await browser.EvalJS($"document.getElementsByClassName('ticket-list collection-as-table')[0].getElementsByTagName('tbody')[0].getElementsByTagName('tr')[{index + 1}].getElementsByTagName('td')[4].textContent");

                        // Gets only the tkts which are not older than the value selected by the user (in minutes)
                        if (tkt.LastUpdated.Contains("hours") || tkt.LastUpdated.Contains("minutes")) {
                            int time = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(tkt.LastUpdated, @"[^\d]+", ""));

                            if (time > SharedSettings.Entries.Get.Int32("searchTimeframe"))
                                break;
                        }

                        tkt.ID     = await browser.EvalJS($"document.getElementsByClassName('ticket-list collection-as-table')[0].getElementsByTagName('tbody')[0].getElementsByTagName('tr')[{index}].getElementsByTagName('td')[0].textContent");
                        tkt.Owner  = await browser.EvalJS($"document.getElementsByClassName('ticket-list collection-as-table')[0].getElementsByTagName('tbody')[0].getElementsByTagName('tr')[{index}].getElementsByTagName('td')[4].textContent");
                        tkt.Title  = await browser.EvalJS($"document.getElementsByClassName('ticket-list collection-as-table')[0].getElementsByTagName('tbody')[0].getElementsByTagName('tr')[{index}].getElementsByTagName('td')[1].textContent");
                        tkt.Status = await browser.EvalJS($"document.getElementsByClassName('ticket-list collection-as-table')[0].getElementsByTagName('tbody')[0].getElementsByTagName('tr')[{index}].getElementsByTagName('td')[2].textContent");
                        tkt.Store  = await browser.EvalJS($"document.getElementsByClassName('ticket-list collection-as-table')[0].getElementsByTagName('tbody')[0].getElementsByTagName('tr')[{index + 1}].getElementsByTagName('td')[1].textContent");
                        tkt.Link   = await browser.EvalJS($"document.getElementsByClassName('ticket-list collection-as-table')[0].getElementsByTagName('tbody')[0].getElementsByTagName('tr')[{index}].getElementsByTagName('td')[1].getElementsByTagName('a')[0].getAttribute('href')");

                        tkts.Add(tkt);
                        index += 2;
                    }

                    return tkts;
                }

                // Function used to check if the current tkt which is in In-Deapth Search does not have any of the countries available
                public bool TktIsWithoutCountry(string str) {
                    string selectedCountry = SharedSettings.Entries.Get.String("searchCountrySelected");

                    // If the selected country exists into the string, the tkt has a defined country
                    if (str.IndexOf(selectedCountry, StringComparison.OrdinalIgnoreCase) > -1)
                        return false;


                    int i = 0;
                    while (SharedSettings.Entries.Exists($"searchCountryItem[{i}]")) {
                        string _c = SharedSettings.Entries.Get.String($"searchCountryItem[{i}]");

                        if (_c != selectedCountry)
                            // If the current country string into the list is found inside the str, the tkt has a defined country
                            if (str.IndexOf(_c, StringComparison.OrdinalIgnoreCase) > -1)
                                return false;

                        i++;
                    }

                    return true;
                }

                public async Task<bool> JDServerIsOk() {
                    if (await browser.ElementExists("document.getElementById('logo').getElementsByTagName('a')[0].href", "https://support.jdplc.com/rt4/MyRT.html", false, 5) == false) {
                        if (await Net_Or_FgServiceAreNotActive())
                            return false;

                        UserNotification.WarningShow(
                            "[SERVER]",
                            "support.jdplc.com does not respond...",
                            "Unable to retrieve the data from the JD server!",
                            "Please try to connect to the JD queue page:\r\n\r\n" +
                            " * If on your browser it's working fine, please send a DM to Adi\r\n" +
                            " * If on your browser isn't working as well, there's a problem with the JD server...\r\n\r\n" +
                            "The application will try to fetch again the data as soon as possible",
                            503);

                        BackgroundWorkerReset(60);
                        return false;
                    }

                    return true;
                }

                public async Task<bool> CheckNetConnection() {
                    if (IsConnectionAvailable() == false) {
                        UserNotification.WarningShow(
                            "[NET]",
                            "Unable to check the queue...",
                            "No Internet connection!",
                            "The application will try to fetch again the data as soon as possible",
                            12029);

                        if (await IsFgServiceActive() == false)
                            return false;

                        BackgroundWorkerReset(60);

                        return false;
                    }

                    return true;
                }

                public async Task<bool> Net_Or_FgServiceAreNotActive() {
                    if (await IsFgServiceActive() == false || await CheckNetConnection() == false)
                        return true;

                    return false;
                }

                public void DisplayNotification(string id, string lastUpdated, string title, string status, string owner, string country, string link) {
                    int time = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(lastUpdated, @"[^\d]+", ""));
                    string lUpdated = "";

                    if (lastUpdated.Contains("hours"))
                        lUpdated += $"{time / 60} hour(s) ago";
                    else if (lastUpdated.Contains("seconds"))
                        lUpdated += $"{time} second(s) ago";
                    else {
                        // The JD Last Updated value will display minutes until at certain point and then will show in hours
                        // So we use this to avoid to display to the user a message like: 356 minutes ago
                        if (time > 60) {
                            int hr = (int)Math.Round((decimal)(time / 60));
                            lUpdated += $"{hr} hour(s) and {time % 60} minute(s) ago";
                        } else
                            lUpdated += $"{time} minute(s). ago";
                    }

                    if (IsFgServiceActive().Result)
                        UserNotification.TktShow(
                            $"[{id}]",
                            lUpdated,
                            title,
                            $"Status:             {status}\r\n" +
                            $"Owner:             {owner}\r\n" +
                            $"Last updated: {lUpdated}",
                            link,
                            country);
                }
            }
        }
    }
}