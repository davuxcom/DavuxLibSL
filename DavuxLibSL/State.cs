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
using Microsoft.Phone.Shell;

namespace DavuxLibSL
{
    public class State
    {
        static PhoneApplicationService svc = PhoneApplicationService.Current;

        public static T Get<T>(string key, T def)
        {
            if (!svc.State.Keys.Contains(key))
            {
                svc.State[key] = def;
            }
            return (T)svc.State[key];
        }

        public static void Set<T>(string key, T value)
        {
            svc.State[key] = value;
        }
    }
}
