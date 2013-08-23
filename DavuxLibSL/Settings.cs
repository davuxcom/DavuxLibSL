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

namespace DavuxLibSL
{
    public class Settings
    {
        static System.IO.IsolatedStorage.IsolatedStorageSettings dict = null;

        static Settings()
        {
            try
            {
                dict = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
            }
            catch (Exception ex) { }

        }


        public static string Get(string key, string def = null)
        {
            return Get<string>(key, def);
        }

        public static void Set(string key, string val)
        {
            Set<string>(key, val);
        }

        public static T Get<T>(string key, T def)
        {
            if (dict == null) return default(T);
            if (!dict.Contains(key))
            {
                dict[key] = def;
                dict.Save();
            }
            return (T)dict[key];
        }

        public static void Set<T>(string key, T val)
        {
            dict[key] = val;
            dict.Save();
        }
    }

}

