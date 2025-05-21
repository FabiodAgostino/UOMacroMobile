using MQTT.Models;
using System.Collections.ObjectModel;
using static MQTT.Models.MqttNotificationModel;

namespace UOMacroMobile.Services.Interfaces
{
    public interface IMqqtService
    {
        Task<bool> ConnectAsync(bool autoReconnect=false);
        Task DisconnectAsync();
        Task<bool> PublishNotificationAsync(string title, string message, NotificationSeverity type );
        public event EventHandler<MqttNotificationModel> NotificationReceived;
        public event EventHandler<bool> ConnectionStatusChanged;
        public ObservableCollection<MqttNotificationModel> Notifications { get; }
        public bool IsConnected { get; }
        bool SmartphoneConnected { get; set; }
        public string CurrentDeviceId { get; set; }
        void DeleteCurrentConnection();
        Task SmartphoneIsAvailable();
        Task SubscribeNotifications(bool force = false);
    }
}
