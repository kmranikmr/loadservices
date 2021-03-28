using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Actors.System
{
    public class Notifier
    {
        public event EventHandler<NotificationArgumet> OnNotification;


        public void Notify(string message)
        {
            if (OnNotification != null)
            {
                OnNotification(this, new NotificationArgumet() { Information = message });
            }
        }
    }

    public class NotificationArgumet
    {
        public string Information { get; set; }
    }
}
