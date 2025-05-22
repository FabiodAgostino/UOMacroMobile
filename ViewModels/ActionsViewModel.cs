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

        public async Task Start()
        {
            try
            {
                if (!_mqqtService.IsConnected)
                    await _mqqtService.ConnectAsync();

                // Invia il comando START
                bool success = await _mqqtService.PublishNotificationAsync(
                    "START",
                    "L'applicativo è stato avviato da un dispositivo mobile",
                    MqttNotificationModel.NotificationSeverity.Info);

                if (!success)
                {
                    throw new Exception("Impossibile inviare il comando START. Verifica la connessione.");
                }

                UpdateStatus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore Start: {ex.Message}");
                throw; // Rilancia l'eccezione per gestirla nell'UI
            }
        }

        public async Task Stop()
        {
            try
            {
                if (!_mqqtService.IsConnected)
                    await _mqqtService.ConnectAsync();

                // Invia il comando STOP
                bool success = await _mqqtService.PublishNotificationAsync(
                    "STOP",
                    "L'applicativo è stato fermato da un dispositivo mobile",
                    MqttNotificationModel.NotificationSeverity.Info);

                if (!success)
                {
                    throw new Exception("Impossibile inviare il comando STOP. Verifica la connessione.");
                }

                UpdateStatus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore Stop: {ex.Message}");
                throw; // Rilancia l'eccezione per gestirla nell'UI
            }
        }

        public async Task Logout()
        {
            try
            {
                if (!_mqqtService.IsConnected)
                    await _mqqtService.ConnectAsync();

                // Invia il comando LOGOUT
                bool success = await _mqqtService.PublishNotificationAsync(
                    "LOGOUT",
                    "TM Client è stato chiuso",
                    MqttNotificationModel.NotificationSeverity.Info);

                if (!success)
                {
                    throw new Exception("Impossibile inviare il comando LOGOUT. Verifica la connessione.");
                }

                UpdateStatus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore Logout: {ex.Message}");
                throw; // Rilancia l'eccezione per gestirla nell'UI
            }
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