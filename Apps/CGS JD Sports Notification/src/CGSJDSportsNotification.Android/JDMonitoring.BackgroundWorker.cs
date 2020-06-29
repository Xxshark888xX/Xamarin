using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Android.Content;
using Android.Util;

namespace CGSJDSportsNotification.Droid {
    public partial class JDMonitoring {
        [BroadcastReceiver]
        public partial class BackgroundWorker : BroadcastReceiver {
            Helper H { get; } = new Helper(60 * SharedSettings.Entries.Get.Int32("searchRefresh"));

            public override void OnReceive(Context context, Intent intent) { FetchTickets(); }

            async void FetchTickets() {
                if (await H.IsDoNotDisturbeTime())
                    return;

                Helper.WifiAcquire();

                // Reinitialize the Browser class
                if (H.browser == null) {
                    H.wc = new Browser.CustomWebViewClient();
                    H.wc.OnPageStart += BrowserOnPageStarted;
                    H.wc.OnPageLoaded += BrowserOnPageLoaded;
                    await H.BrowserInit();
                }

                if (await H.Net_Or_FgServiceAreNotActive())
                    goto CleanUpWebView;

                UserNotification.Remove(12029); // Removes the 'No Internet' notification when the connection is back
                UserNotification.Remove(503); // Removes the 'Server took too long to respond' notification when is back
                UserNotification.Remove(0); // Notification warning group id

                // Resets the process cycle
                H.ProcessPhase = 0;
                H.InDepthProcess = false;
                H.browser.LoadUrl(H.UrlQueuePage);

                // Waits for the log-in page to fully load
                while (H.ProcessPhase == 0) {
                    if (await H.Net_Or_FgServiceAreNotActive())
                        goto CleanUpWebView;

                    await Task.Delay(100);
                }

                bool? loginPage = await H.IsOnLoginPage();
                if (loginPage == true) {
                    bool? loginResponse = await H.Login();

                    if (loginResponse == false) {
                        if (await H.Net_Or_FgServiceAreNotActive())
                            goto CleanUpWebView;

                        H.BackgroundWorkerReset();
                        goto CleanUpWebView;

                    } else if (loginResponse == null)
                        // If Login function returns null means that there's no internet connection or the server is down
                        goto CleanUpWebView;

                } else if (loginPage == false) {
                    if (await H.Net_Or_FgServiceAreNotActive())
                        goto CleanUpWebView;

                    // If at this point ProcessPhase is 1 this means that the browser session was cleaned and the we loggedin again
                    if (H.ProcessPhase == 1)
                        H.browser.LoadUrl(H.UrlQueuePage);
                } else
                    // If null is returned, this means that there's no internet connection or the server is down
                    goto CleanUpWebView;

                // Waits for the queue page to fully load
                while (H.ProcessPhase == 1) {
                    if (await H.Net_Or_FgServiceAreNotActive())
                        goto CleanUpWebView;

                    await Task.Delay(100);
                }

                if (await H.JDServerIsOk() == false)
                    goto CleanUpWebView;

                // Checks if the queue HTML table element is present
                if (await H.browser.ElementExists("document.getElementsByClassName('ticket-list collection-as-table')[0]")) {
                    List<Ticket> tkts = await H.GetTickets();

                    string country = SharedSettings.Entries.Get.String("searchCountrySelected").ToLower();

                    // Fetch all the tickets inside the time frame chosen by the user
                    // Starts from the end of the list in order to show the recent tkts on top
                    for (int i = tkts.Count - 1; i >= 0; i--) {
                        Ticket t = tkts[i];

                        // Checks only the tkts with the 'open' or 'new' status
                        if (t.Status.ToLower() == "open" || t.Status.ToLower() == "new") {
                        //if (t.Status.Length > 0) { //-- Used for testing
                            // The first and fastest step is to check the tkt title
                            if (t.Title.ToLower().Contains(country)) {
                                H.DisplayNotification(t.ID, t.LastUpdated, t.Title, t.Status, t.Owner, country, t.Link);
                            } else if (H.TktIsWithoutCountry(t.Title)) {
                                if (await H.JDServerIsOk() == false)
                                    goto CleanUpWebView;

                                // Country not found in the title, we must dig deeper by opening the tkt link
                                H.InDepthProcess = true;

                                H.browser.LoadUrl(t.Link);

                                // Waits for the tkt url to load
                                while (H.ProcessPhase == 2) {
                                    if (await H.Net_Or_FgServiceAreNotActive())
                                        goto CleanUpWebView;

                                    await Task.Delay(100);
                                }

                                // Gets the page's body
                                string pBody = await H.browser.EvalJS("document.body.textContent");

                                // If the selected country by the user is present somewhere into the tkt page body
                                if (pBody.IndexOf(country, StringComparison.OrdinalIgnoreCase) > -1) {
                                    H.DisplayNotification(t.ID, t.LastUpdated, t.Title, t.Status, t.Owner, country, t.Link);
                                } else if (H.TktIsWithoutCountry(pBody)) {
                                    // If the country string isn't present and the user has chosen to check also for tkts with unknown countries
                                    if (SharedSettings.Entries.Get.Bool("searchUnknownCountry"))
                                        H.DisplayNotification(t.ID, t.LastUpdated, t.Title, t.Status, t.Owner, "unknown", t.Link);
                                }

                                // Ready for the next ticket
                                H.ProcessPhase = 2;
                            }
                        }
                    }
                }

            CleanUpWebView:
                // Tries to free the memory asap
                H.FreeMemory();
                // Reschedules the alarm
                H.BackgroundWorkerReset();
                Helper.WifiRelease();
            }

            public async void BrowserOnPageStarted(object sender, string url) {
                if (await H.Net_Or_FgServiceAreNotActive())
                    return;
            }

            async void BrowserOnPageLoaded(object sender, string url) {
                if (await H.Net_Or_FgServiceAreNotActive())
                    return;
                
                // When the login page has loaded
                if (url.StartsWith(H.UrlLoginPage)) {
                    Log.Info("jd_foo", $"LOGIN_PAGE = TRUE");

                    // Login again in order to access the tkt page
                    if (H.InDepthProcess) {
                        if (await H.IsOnLoginPage() == true)
                            await H.Login();
                    } else
                        // First login
                        H.ProcessPhase = 1;

                    // Saves the login session
                    Android.Webkit.CookieManager.Instance.Flush();
                }

                // When the queue page has loaded
                if (url.Equals(H.UrlQueuePage))
                    H.ProcessPhase = 2;

                // When the tkt page has loaded
                if (url.StartsWith("https://support.jdplc.com/rt4/Ticket/Display.html?id=") && H.InDepthProcess) {
                    while (await H.browser.EvalJS("document.getElementById('TitleBox--_Helpers_TicketHistory------SGlzdG9yeQ__---0')") == null) {
                        if (await H.Net_Or_FgServiceAreNotActive() || await H.JDServerIsOk() == false)
                            return;

                        await Task.Delay(1000);
                    }

                    H.ProcessPhase = 3;
                }
            }
        }
    }
}