using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Reflection;

namespace DavuxLibSL
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();

            lblDevID.Text = App.Key;
            lblVersion.Text = GetVersionNumber();
            lblName.Text = GetName();
        }

        private static string GetVersionNumber()
        {
            if (App.Assembly == null) return "";
            var parts = App.Assembly.FullName.Split(',');
            return parts[1].Split('=')[1];
        }

        private static string GetName()
        {
            if (App.Assembly == null) return "";
            var parts = App.Assembly.FullName.Split(',');
            return parts[0];
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.URL = "http://www.daveamenta.com/donate/";
            wbt.Show();
        }

        private void Email_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask ect = new EmailComposeTask();
            ect.To = "Dave@DaveAmenta.com";
            ect.Subject = "Support Request: " + GetName();
            ect.Body = "\n\n\n"
                + "----------------\n"
                + "Debug information, please do not remove:\n"
                + App.Key + "\n"
                + GetName() + "\n"
                + GetVersionNumber() + "\n";
            ect.Show();
        }

        private void Web_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.URL = "http://www.daveamenta.com/wp7";
            wbt.Show();
        }

    }
}