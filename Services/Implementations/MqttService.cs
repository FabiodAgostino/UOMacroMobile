using MQTT.Models;
using MQTTnet;
using MQTTnet.Client;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using UOMacroMobile.Services.Interfaces;
using static MQTT.Models.MqttNotificationModel;

namespace UOMacroMobile.Services.Implementations
{
    public class MqttService : IMqqtService
    {
        private IMqttClient _mqttClient;
        private string _currentDeviceId;
        private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);
        private const string DeviceIdKey = "CurrentMqttDeviceId";

        public ObservableCollection<MqttNotificationModel> Notifications { get; } = new();

        public event EventHandler<MqttNotificationModel> NotificationReceived;
        public event EventHandler<bool> ConnectionStatusChanged;
        public bool Subscribed { get; set; } = false;
        public bool SmartphoneConnected { get; set; }
        public bool IsConnected => _mqttClient?.IsConnected ?? false;
        public string CurrentDeviceId
        {
            get
            {
                // Se non c'è un device ID corrente, prova a recuperarlo da Preferences
                if (string.IsNullOrEmpty(_currentDeviceId))
                {
                    _currentDeviceId = Preferences.Get(DeviceIdKey, string.Empty);
                }
                return _currentDeviceId;
            }
            private set
            {
                // Quando viene impostato un nuovo device ID, salvalo in Preferences
                _currentDeviceId = value;
                Preferences.Set(DeviceIdKey, value);
            }
        }

        public MqttService()
        {
            _mqttClient = new MqttFactory().CreateMqttClient();
            _currentDeviceId = Preferences.Get(DeviceIdKey, string.Empty);

            // Handler per i messaggi ricevuti
            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                    var notification = JsonSerializer.Deserialize<MqttNotificationModel>(payload);

                    if (notification != null)
                    {
                        if(notification.Message == "CONNECT-OK")
                        {
                            var startDate = Preferences.Get("WaitResponseConnect", new DateTime());
                            if(startDate != new DateTime() && startDate.AddMinutes(2)>DateTime.Now)
                            {
                                SmartphoneConnected = true;
                            }
                            else
                            {
                                SmartphoneConnected = false;
                            }
                            NotificationReceived?.Invoke(this, notification);
                        }
                        else
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                if (!Notifications.Any(n => n.Id == notification.Id))
                                {
                                    Notifications.Insert(0, notification);
                                    NotificationReceived?.Invoke(this, notification);
                                }
                                else
                                {
                                    Console.WriteLine($"Notifica duplicata ignorata: ID {notification.Id}");
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore nella gestione del messaggio: {ex.Message}");
                }

                return Task.CompletedTask;
            };

            // Handler per disconnessione
            _mqttClient.DisconnectedAsync += args =>
            {
                ConnectionStatusChanged?.Invoke(this, false);
                return Task.CompletedTask;
            };
        }

        public void DeleteCurrentConnection()
        {
            Preferences.Remove(DeviceIdKey);
        }

        public async Task SubscribeNotifications()
        {
            if(IsConnected && !Subscribed)
            {
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                          .WithTopic($"uom/notifications/{CurrentDeviceId}")
                          .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                          .Build());
                Subscribed = true;
            }
        }

        public async Task<bool> ConnectAsync()
        {
            if (!IsConnected)
            {
                await _connectionSemaphore.WaitAsync();
                try
                {
                    if (_mqttClient.IsConnected)
                    {
                        await _mqttClient.DisconnectAsync();
                    }

                    var options = new MqttClientOptionsBuilder()
                        .WithTcpServer("broker.hivemq.com", 1883)
                        .WithClientId($"rot_subscriber_{Guid.NewGuid()}")
                        .WithCleanSession()
                        .Build();

                    var result = await _mqttClient.ConnectAsync(options);

                    if (result.ResultCode == MqttClientConnectResultCode.Success)
                    {
                        ConnectionStatusChanged?.Invoke(this, true);
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore di connessione: {ex.Message}");
                    return false;
                }
                finally
                {
                    _connectionSemaphore.Release();
                }
            }
            return false;
        }

        public async Task DisconnectAsync()
        {
            if (_mqttClient?.IsConnected == true)
            {
                await _mqttClient.DisconnectAsync();
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        public async Task<bool> PublishNotificationAsync(string title, string message, NotificationSeverity type)
        {
            if (!IsConnected) return false;

            try
            {
                var notification = new MqttNotificationModel
                {
                    DeviceId = _currentDeviceId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Timestamp = DateTime.UtcNow
                };

                string jsonPayload = JsonSerializer.Serialize(notification);

                // Crea il messaggio MQTT
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic($"uom/startstop/{_currentDeviceId}")
                    .WithPayload(Encoding.UTF8.GetBytes(jsonPayload))
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                var result = await _mqttClient.PublishAsync(mqttMessage);
                return result.IsSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nell'invio della notifica: {ex.Message}");
                return false;
            }
        }

        public async Task SmartphoneIsAvailable()
        {
            Preferences.Set("WaitResponseConnect", DateTime.Now);
            await PublishNotificationAsync("CONNECT", "CONNECT", MQTT.Models.MqttNotificationModel.NotificationSeverity.Info);
        }
    }
}
