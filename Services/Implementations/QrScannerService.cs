// Services/Implementations/QrScannerService.cs
using System;
using System.Threading.Tasks;
using UOMacroMobile.Pages;
using UOMacroMobile.Services.Interfaces;

namespace UOMacroMobile.Services.Implementations
{
    public class QrScannerService : IQrScannerService
    {
        private TaskCompletionSource<string> _scanResultTcs;

        public Task<string> ScanQrCodeAsync()
        {
            _scanResultTcs = new TaskCompletionSource<string>();

            // Registra per ricevere il risultato della scansione
            MessagingCenter.Subscribe<QrScannerPage, string>(this, "QrCodeScanned", OnQrCodeScanned);

            // Registra per la cancellazione
            MessagingCenter.Subscribe<QrScannerPage>(this, "QrCodeScanCancelled", OnQrCodeScanCancelled);

            // Mostra la pagina di scansione
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new QrScannerPage());
                }
                catch (Exception ex)
                {
                    _scanResultTcs.TrySetResult($"Errore nell'apertura della pagina di scansione: {ex.Message}");
                    CleanupSubscriptions();
                }
            });

            return _scanResultTcs.Task;
        }

        private void OnQrCodeScanned(QrScannerPage sender, string result)
        {
            // Completa il task con il risultato
            _scanResultTcs.TrySetResult(result);
            CleanupSubscriptions();
        }

        private void OnQrCodeScanCancelled(QrScannerPage sender)
        {
            // Completa il task con un messaggio di cancellazione
            _scanResultTcs.TrySetResult("Scansione annullata dall'utente");
            CleanupSubscriptions();
        }

        private void CleanupSubscriptions()
        {
            try
            {
                MessagingCenter.Unsubscribe<QrScannerPage, string>(this, "QrCodeScanned");
                MessagingCenter.Unsubscribe<QrScannerPage>(this, "QrCodeScanCancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la pulizia delle sottoscrizioni: {ex.Message}");
            }
        }
    }
}