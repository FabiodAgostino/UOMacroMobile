using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UOMacroMobile.Services.Interfaces;
using ZXing;

namespace UOMacroMobile.ViewModels
{
    public partial class QrScannerViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private bool _isScanning = false; // Inizia come false

        [ObservableProperty]
        private bool _cameraDenied;

        [ObservableProperty]
        private bool _permissionsChecked = false;

        private IMqqtService _mqttService;

        public QrScannerViewModel(IMqqtService mqqtService)
        {
            _mqttService = mqqtService;
            StatusMessage = "Verifica permessi fotocamera...";
            IsBusy = true;
            CameraDenied = false;
        }

        // Metodo pubblico per inizializzare i permessi quando la pagina è completamente caricata
        public async Task InitializeAsync()
        {
            await RequestCameraPermissionWithRetry();
        }

        private async Task RequestCameraPermissionWithRetry(int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    StatusMessage = $"Controllo permessi fotocamera (tentativo {attempt}/{maxRetries})...";

                    var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

                    if (status == PermissionStatus.Granted)
                    {
                        await EnableScanner();
                        return;
                    }

                    if (status == PermissionStatus.Unknown || status == PermissionStatus.Denied)
                    {
                        StatusMessage = "Richiesta permesso fotocamera...";
                        status = await Permissions.RequestAsync<Permissions.Camera>();

                        if (status == PermissionStatus.Granted)
                        {
                            await EnableScanner();
                            return;
                        }
                    }

                    // Se il permesso è stato negato definitivamente
                    if (status == PermissionStatus.Disabled || status == PermissionStatus.Restricted)
                    {
                        ShowPermissionDeniedState();
                        return;
                    }

                    // Aspetta un po' prima del prossimo tentativo
                    if (attempt < maxRetries)
                    {
                        StatusMessage = $"Tentativo fallito, riprovo tra 2 secondi...";
                        await Task.Delay(2000);
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Errore controllo permessi: {ex.Message}";
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(2000);
                    }
                }
            }

            // Se arriviamo qui, tutti i tentativi sono falliti
            ShowPermissionDeniedState();
        }

        private async Task EnableScanner()
        {
            StatusMessage = "Permessi concessi! Inizializzazione scanner...";
            PermissionsChecked = true;
            CameraDenied = false;

            // Aspetta un momento per assicurarsi che tutto sia pronto
            await Task.Delay(500);

            IsScanning = true;
            IsBusy = false;
            StatusMessage = "Posiziona il QR Code nel riquadro";
        }

        private void ShowPermissionDeniedState()
        {
            StatusMessage = "Permessi fotocamera negati";
            CameraDenied = true;
            IsScanning = false;
            IsBusy = false;
            PermissionsChecked = true;
        }

        [RelayCommand]
        public async Task RetryPermissions()
        {
            CameraDenied = false;
            IsBusy = true;
            PermissionsChecked = false;
            await RequestCameraPermissionWithRetry();
        }

        [RelayCommand]
        public Task Close()
        {
            MessagingCenter.Send(Application.Current.MainPage, "QrCodeScanCancelled");
            return Application.Current.MainPage.Navigation.PopModalAsync();
        }

        [RelayCommand]
        public async Task CaptureImage()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                IsScanning = false;
                StatusMessage = "Elaborazione in corso...";

                var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Scansiona un QR code"
                });

                if (photo == null)
                {
                    StatusMessage = "Nessuna foto acquisita";
                    IsBusy = false;
                    IsScanning = true;
                    return;
                }

                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                var result = await DecodeQrCodeAsync(imageBytes);

                if (!string.IsNullOrEmpty(result))
                {
                    MessagingCenter.Send(Application.Current.MainPage, "QrCodeScanned", result);
                    await Application.Current.MainPage.Navigation.PopModalAsync();
                }
                else
                {
                    StatusMessage = "Nessun QR Code trovato nell'immagine";
                    IsBusy = false;
                    IsScanning = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore: {ex.Message}";
                IsBusy = false;
                IsScanning = true;
            }
        }

        private Task<string?> DecodeQrCodeAsync(byte[] imageBytes)
        {
            return Task.Run(() =>
            {
                try
                {
                    Func<byte[], LuminanceSource> createLuminanceSource = (data) =>
                        new RGBLuminanceSource(data, 0, 0);

                    var reader = new BarcodeReader<byte[]>(createLuminanceSource)
                    {
                        AutoRotate = true,
                        Options = new ZXing.Common.DecodingOptions
                        {
                            TryHarder = true,
                            PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                        }
                    };

                    var result = reader.Decode(imageBytes);
                    return result?.Text;
                }
                catch (Exception ex)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusMessage = $"Errore nella decodifica: {ex.Message}";
                    });
                    return null;
                }
            });
        }

        public void ProcessQrResult(string result)
        {
            if (string.IsNullOrEmpty(result))
                return;

            IsScanning = false;
            IsBusy = true;
            StatusMessage = "QR Code rilevato!";

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    _mqttService.CurrentDeviceId = result;
                    if (!_mqttService.IsConnected)
                        await _mqttService.ConnectAsync(true);
                    else
                    {
                        await _mqttService.SubscribeNotifications(true);
                        await _mqttService.SmartphoneIsAvailable();
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Errore: {ex.Message}";
                    IsBusy = false;
                    IsScanning = true;
                }
            });
        }
    }
}