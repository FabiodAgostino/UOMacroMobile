using Microsoft.Maui.Controls;
using MQTT.Models;
using System;

namespace UOMacroMobile.Controls
{
    public partial class NotificationCardView : ContentView
    {
        public static readonly BindableProperty NotificationProperty =
            BindableProperty.Create(nameof(Notification), typeof(MqttNotificationModel), typeof(NotificationCardView));

        public MqttNotificationModel Notification
        {
            get => (MqttNotificationModel)GetValue(NotificationProperty);
            set => SetValue(NotificationProperty, value);
        }

        public event EventHandler<string> OnNotificationClosed;

        public NotificationCardView()
        {
            InitializeComponent();
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            if (Notification != null)
            {
                OnNotificationClosed?.Invoke(this, Notification.Id);
            }
        }
    }
}