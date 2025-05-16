using UOMacroMobile.Services.Interfaces;
using MQTT.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UOMacroMobile.ViewModels
{
    public class ActionsViewModel : INotifyPropertyChanged
    {
        private readonly IMqqtService _mqqtService;
        private string _statusText;

        public bool IsConnected => _mqqtService.IsConnected;

        public string StatusText
        {
            get => _statusText;
            private set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public ActionsViewModel(IMqqtService mqqtService)
        {
            _mqqtService = mqqtService;
            UpdateStatus();

            // Sottoscrizione agli eventi
            _mqqtService.ConnectionStatusChanged += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(UpdateStatus);
            };
        }

        private void UpdateStatus()
        {
            if (IsConnected)
            {
                StatusText = "Connesso - L'applicativo è in esecuzione";
            }
            else
            {
                StatusText = "Non connesso - L'applicativo è fermo";
            }

            OnPropertyChanged(nameof(IsConnected));
        }

        public async Task Stop()
        {
            if (_mqqtService.IsConnected)
            {
                await _mqqtService.PublishNotificationAsync("STOP", "L'applicativo è stato fermato da un dispositivo mobile", MqttNotificationModel.NotificationSeverity.Info);
                await _mqqtService.DisconnectAsync();
            }

            UpdateStatus();
        }

        public async Task Start()
        {
            if (!_mqqtService.IsConnected)
            {
                // Assicurati che sia disponibile un ID dispositivo
                string deviceId = _mqqtService.CurrentDeviceId;
                if (string.IsNullOrEmpty(deviceId))
                {
                    deviceId = "default"; // O qualsiasi ID predefinito che vuoi usare
                }

                await _mqqtService.ConnectAsync(deviceId);
                await _mqqtService.PublishNotificationAsync("START", "L'applicativo è stato avviato da un dispositivo mobile", MqttNotificationModel.NotificationSeverity.Info);
            }

            UpdateStatus();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}