using System;
using System.Net;
using System.Diagnostics;

namespace DavuxLibSL
{
    public class WebPair
    {
        private const string connect_url = "http://www.daveamenta.com/wp7api/pair.php?guid={0}&legacy={1}";

        public string PairCode { get; private set; }

        public event Action<string> PairCodeArrived = delegate { };

        /// <summary>
        /// Return true to retry
        /// </summary>
        public event Func<string, bool> ErrorOccured;

        public WebPair()
        {
            PairCode = Settings.Get("__PairCode__", null);

            Debug.WriteLine("WebPair: UID: " + App.Key + " PC: " + PairCode);
        }

        public void Register()
        {
            if (PairCode == null)
            {
                Debug.WriteLine("Attempting to get pair code...");
                var wc = new WebClient();
                wc.DownloadStringCompleted += (s, e) =>
                    {
                        
                        try
                        {
                            if (e.Error != null)
                            {
                                Debug.WriteLine("Pair Error: " + e.Error.Message);
                                if (ErrorOccured != null)
                                {
                                    if (ErrorOccured(e.Error.Message))
                                    {
                                        Register();
                                    }
                                    else
                                    {
                                        // don't retry
                                    }
                                }
                                else
                                {
                                    // don't retry
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Pair Response: " + e.Result);
                                if (e.Result.Length < 10)
                                {
                                    PairCode = e.Result;
                                    PairCodeArrived(PairCode);
                                }
                                else
                                {
                                    // error from service
                                    if (ErrorOccured != null)
                                    {
                                        if (ErrorOccured(e.Error.Message))
                                        {
                                            Register();
                                        }
                                        else
                                        {
                                            // don't retry
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // error from service
                            if (ErrorOccured != null)
                            {
                                if (ErrorOccured(ex.Message))
                                {
                                    Register();
                                }
                                else
                                {
                                    // don't retry
                                }
                            }
                        }
                    };
                wc.DownloadStringAsync(new Uri(string.Format(connect_url, App.Key, Settings.Get("PairCode", 0).ToString())));
            }
            else
            {
                PairCodeArrived(PairCode);
            }
        }
    }
}
