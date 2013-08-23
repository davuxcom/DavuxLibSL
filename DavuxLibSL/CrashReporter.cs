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
using System.Windows.Navigation;

namespace DavuxLibSL
{
    public class ForcedExitException : Exception
    {
        public ForcedExitException(string msg) : base(msg) { }
    }

    public class CrashReporter
    {
        private static string URL = "http://www.daveamenta.com/wp7api/crash.php?guid={0}&trace={1}";

        public static void Enable()
        {
            /*
            Application.Current.UnhandledException += (_, x) =>
                {
                    if (x.ExceptionObject as ForcedExitException != null) return; // really exit

                    x.Handled = true;

                    WebClient wc = new WebClient();
                    wc.DownloadStringCompleted += (s, e) =>
                        {
                            try
                            {
                                
                            }
                            catch (Exception ex)
                            {

                            }

                        };
                    wc.DownloadStringAsync(new Uri(string.Format(URL, App.Key, Uri.EscapeUriString(ex.ExceptionObject.ToString()))));
                    
                };
             */
        }


    }
}
