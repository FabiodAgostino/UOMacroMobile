// ViewModels/QrScannerViewModel.cs
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
        private bool _isScanning = true;

        [ObservableProperty]
        private bool _cameraDenied;
        private IMqqtService _mqttService;

        public QrScannerViewModel(IMqqtService mqqtService)
        {
            _mqttService = mqqtService;
            StatusMessage = "Posiziona il QR Code nel riquadro";
            IsBusy = false;
            CameraDenied = false;

            // Verifica i permessi della fotocamera
            MainThread.BeginInvokeOnMainThread(async () => await RequestCameraPermission());
        }

        private async Task RequestCameraPermission()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        StatusMessage = "È necessario abilitare i permessi della fotocamera";
                        CameraDenied = true;
                        IsScanning = false;
                    }
                }
                else
                {
                    CameraDenied = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore: {ex.Message}";
            }
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

                // Usa MediaPicker per catturare una foto
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

                // Leggi la foto
                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                // Decodifica il QR code
                var result = await DecodeQrCodeAsync(imageBytes);

                if (!string.IsNullOrEmpty(result))
                {
                    // Invia il risultato
                    MessagingCenter.Send(Application.Current.MainPage, "QrCodeScanned", result);

                    // Chiudi la pagina
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
                    // Crea una funzione che converte byte[] in LuminanceSource
                    Func<byte[], LuminanceSource> createLuminanceSource = (data) =>
                        new RGBLuminanceSource(data, 0, 0); // Nota: dovresti specificare larghezza e altezza reali

                    // Crea il lettore con la funzione di conversione
                    var reader = new BarcodeReader<byte[]>(createLuminanceSource)
                    {
                        AutoRotate = true,
                        Options = new ZXing.Common.DecodingOptions
                        {
                            TryHarder = true,
                            PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                        }
                    };

                    // Decodifica l'immagine
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

            // Invia il risultato e chiudi la pagina
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await _mqttService.ConnectAsync(result);
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