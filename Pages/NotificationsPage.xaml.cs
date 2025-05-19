using UOMacroMobile.Services.Implementations;
using UOMacroMobile.Services.Interfaces;
using UOMacroMobile.ViewModels;
using static MQTT.Models.MqttNotificationModel;

namespace UOMacroMobile.Pages;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel _viewModel;
    private readonly IMqqtService _mqttService;

    private readonly IQrScannerService _qrScannerService;

    public NotificationsPage()
    {
        _mqttService = IPlatformApplication.Current.Services.GetService<IMqqtService>();
        var notificationService = IPlatformApplication.Current.Services.GetService<INotificationService>();
        _qrScannerService = IPlatformApplication.Current.Services.GetService<IQrScannerService>();
        InitializeComponent();
        _viewModel = new NotificationsViewModel(_mqttService, notificationService);
        BindingContext = _viewModel;
        Dispatcher.Dispatch(async () =>
        {
            Console.WriteLine("Dispatcher.Dispatch eseguito");
            if (_mqttService != null)
            {
                try
                {
                    if(!String.IsNullOrEmpty(_mqttService.CurrentDeviceId))
                        await _mqttService.SubscribeNotifications();
                    _viewModel.UpdateConnectionStatus();
                    BindingContext = _viewModel;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore in Dispatcher: {ex.Message}");
                }
            }
        });
    }


    public NotificationsPage(IMqqtService mqttService, INotificationService notificationService, IQrScannerService qrScannerService)
    {
        InitializeComponent();

        _mqttService = mqttService;
        _viewModel = new NotificationsViewModel(mqttService, notificationService);
        BindingContext = _viewModel;
        _qrScannerService = qrScannerService;
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private void OnNotificationClosed(object sender, string notificationId)
    {
        _viewModel.RemoveNotification(notificationId);
    }

    private async void OnGlobeButtonClicked(object sender, EventArgs e)
    {
        if (_mqttService.SmartphoneConnected)
        {
            bool disconnect = await DisplayAlert(
                "Disconnessione",
                $"Sei connesso a {_mqttService.CurrentDeviceId}. Vuoi disconnetterti?",
                "Disconnetti", "Annulla");

            if (disconnect)
            {
                await _mqttService.DisconnectAsync();
            }
        }
        else
        {
            var result = await _qrScannerService.ScanQrCodeAsync();
        }
    }

    private async void DisconnectClicked(object sender, EventArgs e)
    {
        if (_mqttService.SmartphoneConnected)
        {
            bool disconnect = await DisplayAlert(
                "Disconnessione",
                $"Sei connesso a {_mqttService.CurrentDeviceId}. Vuoi disconnetterti? Questo eliminerà anche la connessione di default!",
                "Disconnetti", "Annulla");

            if (disconnect)
            {
                await _mqttService.DisconnectAsync();
                _mqttService.DeleteCurrentConnection();

            }
        }
    }

    // Mostra/nasconde la barra dei filtri
    private void OnFilterButtonClicked(object sender, EventArgs e)
    {
        FilterBar.IsVisible = !FilterBar.IsVisible;
        if (FilterBar.IsVisible)
        {
            FilterButton.Text = "Chiudi";
        }
        else
        {
            FilterButton.Text = "Filtra";
        }
    }

    // Reset del filtro
    private void OnResetFilterTapped(object sender, EventArgs e)
    {
        _viewModel.ResetFilters();
    }

    // Filtro per notifiche Info
    private void OnInfoFilterTapped(object sender, EventArgs e)
    {
        _viewModel.FilterBySeverity(NotificationSeverity.Info);
    }

    // Filtro per notifiche Warning
    private void OnWarningFilterTapped(object sender, EventArgs e)
    {
        _viewModel.FilterBySeverity(NotificationSeverity.Warning);
    }

    // Filtro per notifiche Error
    private void OnErrorFilterTapped(object sender, EventArgs e)
    {
        _viewModel.FilterBySeverity(NotificationSeverity.Error);
    }

}