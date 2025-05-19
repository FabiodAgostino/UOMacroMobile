using MQTT.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UOMacroMobile.Services.Implementations;
using UOMacroMobile.Services.Interfaces;
using static MQTT.Models.MqttNotificationModel;

namespace UOMacroMobile.ViewModels
{
    public class NotificationsViewModel : BaseViewModel
    {
        private readonly IMqqtService _mqttService;
        private readonly INotificationService _notificationService;

        private string _statusText = "Disconnesso";
        private string _statusTextDevice = "Disconnesso";

        private Color _statusColor = Colors.Red;
        private Color _statusColorDevice = Colors.Red;

        private string _runtimeText = "00:00:00";
        private string _lastActivityText = "Never";
        private DateTime _startTime;
        private bool _isRunning;
        private string _searchText;
        private NotificationSeverity? _selectedSeverityFilter;
        private ObservableCollection<MqttNotificationModel> _allNotifications;
        private ObservableCollection<MqttNotificationModel> _filteredNotifications;
        public bool SmartphoneConnected => _mqttService.SmartphoneConnected;    
        public bool IsConnected => _mqttService.IsConnected;
        public string CurrentDeviceId => _mqttService.CurrentDeviceId;

        // Cambia la proprietà Notifications per usare la collezione filtrata
        public ObservableCollection<MqttNotificationModel> Notifications => _filteredNotifications;

        // Proprietà per il filtro testuale
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        // Proprietà per il filtro di gravità
        public NotificationSeverity? SelectedSeverityFilter
        {
            get => _selectedSeverityFilter;
            set
            {
                if (SetProperty(ref _selectedSeverityFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public Color StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public string StatusTextDevice
        {
            get => _statusTextDevice;
            set => SetProperty(ref _statusTextDevice, value);
        }

        public Color StatusColorDevice
        {
            get => _statusColorDevice;
            set => SetProperty(ref _statusColorDevice, value);
        }

        public string RuntimeText
        {
            get => _runtimeText;
            set => SetProperty(ref _runtimeText, value);
        }

        public string LastActivityText
        {
            get => _lastActivityText;
            set => SetProperty(ref _lastActivityText, value);
        }

        public NotificationsViewModel(IMqqtService mqttService, INotificationService notificationService)
        {
            _mqttService = mqttService;
            _notificationService = notificationService;

            // Inizializza le collezioni
            _allNotifications = new ObservableCollection<MqttNotificationModel>();
            _filteredNotifications = new ObservableCollection<MqttNotificationModel>();

            // Configura gli eventi
            _mqttService.NotificationReceived += OnNotificationReceived;
            _mqttService.ConnectionStatusChanged += OnConnectionStatusChanged;

            // Inizializza le notifiche dall'MQTT service
            foreach (var notification in _mqttService.Notifications)
            {
                _allNotifications.Add(notification);
            }

            // Applica i filtri iniziali
            ApplyFilters();

            // Aggiorna lo stato
            UpdateConnectionStatus();
            UpdateConnectionStatusDevice();

            // Avvia il timer per il runtime
            StartRuntimeTimer();
        }

        // Metodo per applicare i filtri
        private void ApplyFilters()
        {
            var filteredItems = _allNotifications.AsEnumerable();

            // Applica filtro di testo
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                filteredItems = filteredItems.Where(n =>
                    (n.Title?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (n.Message?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Applica filtro di gravità
            if (_selectedSeverityFilter.HasValue)
            {
                filteredItems = filteredItems.Where(n => n.Type == _selectedSeverityFilter.Value);
            }

            // Aggiorna la collezione filtrata
            _filteredNotifications.Clear();
            foreach (var item in filteredItems.OrderByDescending(n => n.Timestamp))
            {
                _filteredNotifications.Add(item);
            }

            OnPropertyChanged(nameof(Notifications));
        }

        private async void OnNotificationReceived(object sender, MqttNotificationModel notification)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!_allNotifications.Any(n => n.Id == notification.Id) && notification.Message!="CONNECT-OK")
                {
                    _allNotifications.Insert(0, notification);
                    ApplyFilters(); // Applica i filtri quando arriva una nuova notifica
                }

                LastActivityText = "adesso";
                UpdateConnectionStatus();
                UpdateConnectionStatusDevice();
                // Mostra notifica push
                if(notification.Message != "CONNECT-OK")
                    await _notificationService.ShowNotificationAsync(notification);
            });
        }

        private void OnConnectionStatusChanged(object sender, bool isConnected)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateConnectionStatus();
                UpdateConnectionStatusDevice();
            });
        }

        public void UpdateConnectionStatus()
        {
            if (_mqttService.IsConnected)
            {
                StatusText = "Connesso";
                StatusColor = Colors.Green;
                _isRunning = true;
            }
            else
            {
                StatusText = "Disconnesso";
                StatusColor = Colors.Red;
                _isRunning = false;
            }
        }
        public void UpdateConnectionStatusDevice()
        {
            if (_mqttService.SmartphoneConnected)
            {
                StatusTextDevice = "Connesso";
                StatusColorDevice = Colors.Green;
            }
            else
            {
                StatusTextDevice = "Disconnesso";
                StatusColorDevice = Colors.Red;
            }
        }

        private void StartRuntimeTimer()
        {
            _startTime = DateTime.Now;

            // Inizia a contare il tempo
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (_isRunning)
                {
                    var elapsed = DateTime.Now - _startTime;
                    RuntimeText = $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
                }

                return true; // Continua il timer
            });
        }

        public void RemoveNotification(string id)
        {
            var notification = _allNotifications.FirstOrDefault(n => n.Id == id);
            if (notification != null)
            {
                _allNotifications.Remove(notification);
                ApplyFilters(); // Riapplica i filtri dopo la rimozione
            }
        }

        // Metodo per reimpostare tutti i filtri
        public void ResetFilters()
        {
            _searchText = null;
            _selectedSeverityFilter = null;
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(SelectedSeverityFilter));
            ApplyFilters();
        }

        // Metodo per filtrare per gravità
        public void FilterBySeverity(NotificationSeverity severity)
        {
            // Toggle del filtro: se è già selezionato lo stesso filtro, lo rimuove
            if (_selectedSeverityFilter == severity)
            {
                SelectedSeverityFilter = null;
            }
            else
            {
                SelectedSeverityFilter = severity;
            }
        }

        public async Task DisconnectAsync() => await _mqttService.DisconnectAsync();
        public async Task ConnectAsync(string deviceId) => await _mqttService.ConnectAsync();

        // Metodo di test per creare notifiche con diverse gravità
        public void SendNotification()
        {
            _notificationService.ShowNotificationAsync(new MqttNotificationModel
            {
                Title = "Test Notification",
                Message = "This is a test notification.",
            });
        }
    }
}