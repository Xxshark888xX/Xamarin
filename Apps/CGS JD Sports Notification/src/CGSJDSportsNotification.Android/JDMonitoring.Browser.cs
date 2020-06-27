using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Android.Webkit;
using Xamarin.Forms;

namespace CGSJDSportsNotification.Droid {
    public partial class JDMonitoring {
        public class Browser {
            public Android.Webkit.WebView WB;

            // Used inside the '.ElementExists' method
            Stopwatch timer;

            public class CustomWebViewClient : WebViewClient {
                public event EventHandler<string> OnPageStart;
                public event EventHandler<string> OnPageLoaded;

                public override void OnPageStarted(Android.Webkit.WebView view, string url, Android.Graphics.Bitmap favicon) {
                    OnPageStart?.Invoke(this, url);
                }
                public override void OnPageFinished(Android.Webkit.WebView view, string url) {
                    OnPageLoaded?.Invoke(this, url);
                }
            }

            public Browser(CustomWebViewClient wc, string url = "", bool clearAllDataFromLastSession = true) {
                WB = new Android.Webkit.WebView(BackgroundWorker.Helper.AndroidContext);
                WB.SetWebViewClient(wc);

                if (clearAllDataFromLastSession)
                    ClearAll();

                WB.Settings.SetAppCacheEnabled(true);
                WB.Settings.JavaScriptEnabled = true;
                WB.Settings.DomStorageEnabled = true;
                //WB.Settings.BlockNetworkLoads = true;
                WB.Settings.BlockNetworkImage = true;
                WB.Settings.MixedContentMode  = MixedContentHandling.CompatibilityMode;
                WB.Settings.UserAgentString   = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246";


                WB.SetWebChromeClient(new WebChromeClient());

                if (url != "")
                    WB.LoadUrl(url);
            }

            public void ClearAll() {
                try {
                    WebStorage.Instance.DeleteAllData();
                    CookieManager.Instance.RemoveAllCookies(null);
                    CookieManager.Instance.Flush();

                    Device.BeginInvokeOnMainThread(() => {
                        try {
                            WB.ClearCache(true);
                            WB.ClearFormData();
                            WB.ClearHistory();
                            WB.ClearSslPreferences();
                            //WB.Settings.SetAppCacheEnabled(false);
                            //WB.Settings.CacheMode = CacheModes.NoCache;
                        } catch (Exception e) { Debug.WriteLine(e); }
                    });
                } catch(Exception e) { Debug.WriteLine(e); }
            }

            public void LoadUrl(string url) {
                try {
                    Device.BeginInvokeOnMainThread(() => {
                        try { WB.LoadUrl(url); } catch (Exception e) { Debug.WriteLine(e); }
                    });
                } catch (Exception e) { Debug.WriteLine(e); }
            }

            public async Task<string> EvalJS(string js, bool returnNullObjectWhenNull = true) {
                try {
                    string JSResult = "";
                    ManualResetEvent reset = new ManualResetEvent(false);

                    Device.BeginInvokeOnMainThread(() => {
                        try {
                            WB?.EvaluateJavascript($"javascript:(function() {{ return {js}; }})()", new JSInterface((r) => {
                                JSResult = r;
                                reset.Set();
                            }));
                        } catch (Exception e) { Debug.WriteLine(e); }
                    });

                    await Task.Run(() => { reset.WaitOne(); });
                    return JSResult == "null" ? returnNullObjectWhenNull ? null : "null" : JSResult;
                } catch (Exception e) { Debug.WriteLine(e); return null; }
            }

            class JSInterface : Java.Lang.Object, IValueCallback {
                private Action<string> _callback;

                public JSInterface(Action<string> callback) {
                    try { _callback = callback; } catch (Exception e) { Debug.WriteLine(e); }
                }

                public void OnReceiveValue(Java.Lang.Object value) {
                    try {
                        string v = value.ToString();

                        if (v.StartsWith('"') && v.EndsWith('"'))
                            v = v.Remove(0, 1).Remove(v.Length - 2, 1);

                        _callback?.Invoke(v);
                    } catch (Exception e) { Debug.WriteLine(e); }
                }
            }

            public async Task<bool> ElementExists(string js, string valueToCompare = null, bool valueToCompareCaseSensitive = true, int timeoutSec = 30) {
                try {
                    timer = new Stopwatch();
                    timer.Start();

                    if (valueToCompare != null) {
                        if (valueToCompareCaseSensitive == false)
                            valueToCompare.ToLower();
                    }

                    // valueToCompare it's used to wait until the element returns the specific value
                    while (valueToCompare == null ?

                        /* valueToCompare  = null */
                        /* while */ await EvalJS(js) == null :

                        /* valueToCompare != null */ 
                        valueToCompareCaseSensitive ?

                        /* caseSensitive   = true*/
                        /* while */ (await EvalJS(js, false)).ToLower() != valueToCompare :

                        /* caseSensitive   = false*/
                        /* while */ await EvalJS(js, false) != valueToCompare) 
                    {
                        await Task.Delay(100);

                        if (timer.ElapsedMilliseconds > 1000 * timeoutSec)
                            return false;
                    }

                    timer.Stop();
                    timer = null;

                    return true;
                } catch (Exception e) { Debug.WriteLine(e); return false; }
            }
        }
    }
}