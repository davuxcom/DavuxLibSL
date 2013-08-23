using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reflection;
using System.Diagnostics;
using DavuxLibSL.Extensions;

namespace DavuxLibSL
{
    public class App
    {
        public static event Action<bool> CloudRegistrationComplete = delegate { };

        private static string URL = "http://www.daveamenta.com/wp7api/registerapp.php?guid={0}&channel={1}&options={2}";

        public static void RegisterService(string options)
        {
            Debug.WriteLine("Registration Service: " + options);
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (se, ee) =>
            {
                try
                {
                    if (ee.Error != null)
                    {
                        MessageBox.Show("Error connecting to cloud service: " + ee.Error.Message);
                        return;
                    }
                    Debug.WriteLine("Cloud App Registration: " + ee.Result);
                    
                    if (ee.Result == "OK") Settings.Set("__CloudRegisteredOnce__", true);
                    CloudRegistrationComplete(ee.Result == "OK");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error connecting to cloud service: " + ex.Message);
                    return;
                }
            };
            wc.DownloadStringAsync(new Uri(
                string.Format(URL, App.Key, PushNotifications.ChannelName, options.ToBase64())));
        }

        public static bool IsCluodServiceRegistered
        {
            get { return Settings.Get("__CloudRegisteredOnce__", false); }
        }

        public static string Key
        {
            get
            {
                return Settings.Get("__AppKey__", Guid.NewGuid().ToString());
            }
        }

        public static Assembly Assembly { get; set; }
        /*
        public static string Name
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Name;
            }
        }


        public static string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        */

        public static void Activated()
        {
            App.Assembly = Assembly.GetCallingAssembly();
           // if (App.Assembly == null) App.Assembly = DavuxLibSL.State.Get("__Assembly__", App.Assembly);
        }

        public static void Deactivated()
        {
           // State.Set("__Assembly__", App.Assembly);
        }
    }
}
