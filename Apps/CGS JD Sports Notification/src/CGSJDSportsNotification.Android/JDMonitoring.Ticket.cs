using System;

namespace CGSJDSportsNotification.Droid {
    public partial class JDMonitoring {
        public class Ticket {
            string owner;
            string title;
            string store;
            string lastUpdated;
            string link;

            public string ID { get; set; }
            public string Owner {
                get {
                    return owner == null ? "Nobody" : owner;
                }
                set {
                    if (value.Contains("("))
                        owner = value.Remove(0, value.IndexOf('(') + 1).Replace(")", "");
                    else
                        owner = value;
                }
            }
            public string Title {
                get {
                    return title;
                }
                set {
                    string v = value;

                    if (v.StartsWith("(P"))
                        v = v.Remove(0, v.IndexOf(')') + 2);

                    if (v.StartsWith('*') || v.StartsWith('+'))
                        v = v.Remove(0, 1);

                    title = v;
                }
            }
            public string Status { get; set; }
            public string Store {
                get {
                    return store;
                }
                set {
                    store = value.Replace(@"\u003C", "").Replace(">", "");
                }
            }
            public string LastUpdated {
                get {
                    return lastUpdated;
                }
                set {
                    string v = "";

                    int time = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(value, @"[^\d]+", ""));

                    // Convert to minutes
                    if (value.Contains("hours") || value.Contains("hour")) {
                        time *= 60;

                        v = v.Insert(0, $"{time} minutes ago");
                    } else if (value.Contains("seconds") || value.Contains("second"))
                        v = v.Insert(0, $"{time} seconds ago");
                    else if (value.Contains("day") == false && value.Contains("days") == false)
                        v = v.Insert(0, $"{time} minutes ago");

                    lastUpdated = v;
                }
            }
            public string Link {
                get {
                    return link;
                }
                set {
                    link = "https://support.jdplc.com/" + value;
                }
            }
        }
    }
}