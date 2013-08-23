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

namespace DavuxLibSL
{
    public partial class PushWidget : UserControl
    {
        public PushWidget()
        {
            InitializeComponent();
            
            Loaded += (s, e) =>
                {
                    toggle.IsChecked = PushNotifications.Enabled;
                    lblError.Text = PushNotifications.LastError;
                    lblStatus.Text = PushNotifications.CurrentStatus.ToString();

                    PushNotifications.ErrorOccurred += err =>
                        Dispatcher.BeginInvoke(() => lblError.Text = err);
                    PushNotifications.EnabledChanged += enabled =>
                        Dispatcher.BeginInvoke(() => toggle.IsChecked = enabled);
                    PushNotifications.StatusChanged += status =>
                        Dispatcher.BeginInvoke(() => lblStatus.Text = status.ToString());
                    PushNotifications.LastPushNotificationStatusUpdated += status =>
                        Dispatcher.BeginInvoke(() => lblLastStatus.Text = status);
                    PushNotifications.LoadingStateChanged += newState =>
                        progress.IsIndeterminate = !newState;

                    PushNotifications.GetLastPushNotificationStatus();
                    // FIXME changed from IsLoading to IsIndeterminate
                    progress.IsIndeterminate = !PushNotifications.IsDoneLoading;
                }; 
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            PushNotifications.Enabled = (bool) toggle.IsChecked;
        }
    }
}
