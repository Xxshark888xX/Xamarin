using Xamarin.Essentials;
using System;

namespace CGSJDSportsNotification {
    public static class SharedSettings {
        public class Entries {
            public class AddOrEdit {
                public static bool Bool(string id, bool value) {
                    Preferences.Set(id, value);

                    if (Exists(id))
                        return true;
                    else
                        return false;
                }

                public static bool Double(string id, double value) {
                    Preferences.Set(id, value);

                    if (Exists(id))
                        return true;
                    else
                        return false;
                }

                public static bool Int32(string id, int value) {
                    Preferences.Set(id, value);

                    if (Exists(id))
                        return true;
                    else
                        return false;
                }

                public static bool Float(string id, float value) {
                    Preferences.Set(id, value);

                    if (Exists(id))
                        return true;
                    else
                        return false;
                }

                public static bool Int64(string id, long value) {
                    Preferences.Set(id, value);

                    if (Exists(id))
                        return true;
                    else
                        return false;
                }

                public static bool String(string id, string value) {
                    Preferences.Set(id, value);

                    if (Exists(id))
                        return true;
                    else
                        return false;
                }

                public static bool DateTime(string id, DateTime value) {
                    Preferences.Set(id, value);

                    if (Exists(id))
                        return true;
                    else
                        return false;
                }
            }

            public class Get {
                public static bool Bool(string id) {
                    return Preferences.Get(id, false);
                }

                public static double Double(string id) {
                    return Preferences.Get(id, 0.0d);
                }

                public static int Int32(string id) {
                    return Preferences.Get(id, 0);
                }

                public static float Float(string id) {
                    return Preferences.Get(id, 0.0f);
                }

                public static long Int64(string id) {
                    return Preferences.Get(id, 0);
                }

                public static string String(string id) {
                    return Preferences.Get(id, "");
                }

                public static DateTime DateTime(string id) {
                    return Preferences.Get(id, new DateTime(0));
                }
            }

            public static bool Exists(string id) {
                if (Preferences.ContainsKey(id))
                    return true;
                else
                    return false;
            }

            public static bool Remove(string id) {
                Preferences.Remove(id);

                if (Exists(id))
                    return false;

                return true;
            }
        }

        public class SecureEntries {
            public static bool Exists(string id) {
                if (SecureStorage.GetAsync(id).Result == null)
                    return false;

                return true;
            }
            public static void AddOrEdit(string id, string value) {
                SecureStorage.SetAsync(id, value);
            }
            public static string Get(string id) {
                return SecureStorage.GetAsync(id).Result;
            }
            public static void Remove(string id) {
                SecureStorage.Remove(id);
            }
        }

        public static void ClearAll(bool removeSecureEntries = true) {
            Preferences.Clear();
            
            if (removeSecureEntries)
                SecureStorage.RemoveAll();
        }
    }
}