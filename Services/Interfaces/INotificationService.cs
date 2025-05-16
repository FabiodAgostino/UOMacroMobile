using MQTT.Models;

namespace UOMacroMobile.Services.Interfaces
{
    public interface INotificationService
    {
        Task ShowNotificationAsync(MqttNotificationModel notification);
    }
}
