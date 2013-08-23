using System;
using System.Net;
using System.Windows;
using Microsoft.Phone.Notification;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using DavuxLibSL.Extensions;

namespace DavuxLibSL
{
    public class PushNotifications
    {

        public enum Status
        {
            Uninitialized,
            Opening, 
            OK,
            Found,
            Error,
            Off,
        }

        private static Collection<Uri> uris = new Collection<Uri> 
        { new Uri("http://www.daveamenta.com/") };

        // NOTE: guid and link are base64
        private static string suburl = 
            "http://www.daveamenta.com/wp7api/push.php?guid={0}&link={1}";
        private static string status_url =
            "http://www.daveamenta.com/wp7api/pushresult.php?guid={0}";

        // NOTE: options is base64
        private static string app_register_url =
            "http://www.daveamenta.com/wp7api/registerapp.php?guid={0}&channel={1}&options={2}";

        static private HttpNotificationChannel _channel = null;
        // FIXME: why is this the default?
		static public string ChannelName = "com.Davux.SendToWP7";

        public static event Action<string> OnRawNotification = delegate { };
        public static event Action<string, string> OnToastNotification = delegate { };
        public static event Action<string> ErrorOccurred = delegate { };
        public static event Action<string> Initialized = delegate { };
        public static event Action<string> PushURIUpdated;
        public static event Action<Status> StatusChanged = delegate { };
        public static event Action<bool> EnabledChanged = delegate { };
        public static event Action<string> LastPushNotificationStatusUpdated = delegate { };
        public static event Action<bool> LoadingStateChanged = delegate { };

        private static Status _Status = Status.Uninitialized;
        public static Status CurrentStatus
        {
            get
            {
                return _Status;
            }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    StatusChanged(_Status);
                }
            }
        }


        private static bool _IsDoneLoading = false;
        public static bool IsDoneLoading
        {
            get
            {
                return _IsDoneLoading;
            }
            set
            {
                if (_IsDoneLoading != value)
                {
                    _IsDoneLoading = value;
                    LoadingStateChanged(value);
                }
            }
        }

        private static string _LastError = "";
        public static string LastError
        {
            get { return _LastError; }
            set
            {
                if (_LastError != value)
                {
                    _LastError = value;
                    if (value != "") ErrorOccurred(value);
                }
            }
        }

        /// <summary>
        /// Return true to enable push notifications, false to cancel
        /// </summary>
        public static event Func<bool> PromptForEnable; // attach a default handler if the user doesn't

        public static void GetLastPushNotificationStatus()
        {
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (se, ee) =>
            {
                if (ee.Error != null)
                {
                    LastPushNotificationStatusUpdated("Error: " + ee.Error);
                    return;
                }
                Debug.WriteLine("Last Notification Status Result: " + ee.Result);
                LastPushNotificationStatusUpdated(ee.Result);
            };
            wc.DownloadStringAsync(new Uri(string.Format(status_url, App.Key)));
        }

        public static void Initialize()
        {
            // testing: Settings.Set("__TryPushEnabled__", true);

            ErrorOccurred += err =>
                {
                    Debug.WriteLine("Push Error: " + err);
                    CurrentStatus = Status.Error;
                    LastError = err;
                };

            StatusChanged += s => Debug.WriteLine("Push Status: " + s);


            // default 'would you like to setup push?' handler
            if (PromptForEnable == null)
            {
                PromptForEnable += () =>
                    MessageBox.Show("Would you like to receive push notifications for this application?",
                     "Notification Setup", MessageBoxButton.OKCancel) == MessageBoxResult.OK;
            }

            // default cloud handler
            if (PushURIUpdated == null)
            {
                PushURIUpdated += url =>
                    {
                        Debug.WriteLine("Push URL Updated: " + url);
                        WebClient wc = new WebClient();
                        wc.DownloadStringCompleted += (se, ee) =>
                        {
                            if (ee.Error != null)
                            {
                                MessageBox.Show("Error registering for push notifications: " + ee.Error.Message);
                                return;
                            }
                            Debug.WriteLine("Channel Cloud Registration: " + ee.Result);
                        };
                        wc.DownloadStringAsync(new Uri(
                            string.Format(suburl, App.Key.ToBase64(), url.ToBase64())
                            ));
                    };
            }

            if (Enabled) Enable();
        }

        public static void RegisterApp(string channel, string tag, string options, Action<bool, string> Callback)
        {
            Debug.WriteLine("RegisterApp: " + channel);
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (se, ee) =>
            {
                if (ee.Error != null)
                {
                    MessageBox.Show("Error registering for application service: " + ee.Error.Message);
                    Callback(false, ee.Error.Message);
                    return;
                }
                Debug.WriteLine("App Registration: " + ee.Result);
                Callback(true, ee.Result);
            };
            wc.DownloadStringAsync(new Uri(
                string.Format(app_register_url, App.Key + "-" + tag, channel, options.ToBase64())
                ));
        }

        public static bool Enabled
        {
            get
            {
                try
                {
                    return Settings.Get("__PushEnabled__", false);
                }
                catch // The designer will call this and crash. :/
                {
                    return false; 
                }
            }
            set
            {
                if (value != Enabled)
                {
                    bool newValue = value;
                    if (value)
                    {
                        newValue = Enable(); // the user can cancel
                    }
                    else
                    {
                        Disable();
                    }
                    Settings.Set("__PushEnabled__", newValue);
                    EnabledChanged(Enabled);
                }
            }
        }

        public static void TryEnableOnce()
        {
            if (Settings.Get("__TryPushEnabled__", true) == true)
            {
                Enabled = true;
            }
            Settings.Set("__TryPushEnabled__", false);
        }

        private static bool Enable()
        {
            bool enable = Enabled || PromptForEnable();
            if (enable)
            {
                Settings.Set("__PushEnabled__", true);
                LastError = "";
                SetupNotificationChannel();
            }
            return enable;
        }

        private static void Disable()
        {
            if (_channel != null)
            {
                try
                {
                    CurrentStatus = Status.Off;
                    _channel.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error closing push channel: " + ex.Message);
                }
            }
        }

        public static void RemoveOldChannel(string channelName)
        {
            try
            {
                var channel = HttpNotificationChannel.Find(channelName);
                if (channel != null)
                {
                    try
                    {
                        channel.Close();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
            catch (Exception exo)
            {
                Debug.WriteLine(exo);
            }
        }

		private static void SetupNotificationChannel()
		{
            try
            {
                _channel = HttpNotificationChannel.Find(ChannelName);

                if (_channel == null)
                {
                    try
                    {
                        _channel = new HttpNotificationChannel(ChannelName);
                        _channel.ChannelUriUpdated += (s, e) =>
                            {
                                _channel = HttpNotificationChannel.Find(ChannelName);

                                if (!_channel.IsShellTileBound) _channel.BindToShellTile(uris);
                                if (!_channel.IsShellToastBound) _channel.BindToShellToast();

                                RegisterForNotifications();
                            };
                        _channel.ErrorOccurred += (s, e) => LastError = e.Message;
                        CurrentStatus = Status.Opening;
                        _channel.Open();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            _channel.Close();
                        }
                        catch (Exception) { }
                        // MessageBox.Show("Failed to open Push Notification Channel: " + ex.Message);
                        CurrentStatus = Status.Error;
                        LastError = ex.Message;
                    }
                }
                else
                {
                    RegisterForNotifications();
                }
            }
            catch (Exception exx)
            {
                CurrentStatus = Status.Error;
                LastError = exx.Message;
            }
		}

		private static void RegisterForNotifications()
		{
            CurrentStatus = Status.Found;
            if (_channel.ChannelUri == null)
            {
                Debug.WriteLine("URI null");
                return;
            }

            _channel.ShellToastNotificationReceived += (s, e) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Debug.WriteLine("Shell Toast: " + e.Collection["wp:Text1"]);
                        OnToastNotification(e.Collection["wp:Text1"], e.Collection["wp:Text2"]);
                    });
                };

            _channel.HttpNotificationReceived += (s, e) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    using (var reader = new StreamReader(e.Notification.Body))
                    {
                        Debug.WriteLine("Raw Message");
                        OnRawNotification(reader.ReadToEnd());
                    }
                });
            };

			_channel.ErrorOccurred += (s, e) => LastError = e.Message;

            OnInitialized(_channel.ChannelUri.ToString());
            CurrentStatus = Status.OK;
        }

        private static void OnInitialized(string url)
        {
            Debug.WriteLine("Push URL: " + url);
            if (url != Settings.Get("__PushURL__", ""))
            {
                PushURIUpdated(url);
                Settings.Set("__PushURL__", url);
            }
            Initialized(url);
        }
    }
}
