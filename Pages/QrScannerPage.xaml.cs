// Pages/QrScannerPage.xaml.cs
using Microsoft.Maui.Controls;
using System;
using UOMacroMobile.Services.Interfaces;
using UOMacroMobile.ViewModels;

namespace UOMacroMobile.Pages
{
    public partial class QrScannerPage : ContentPage
    {
        private QrScannerViewModel _viewModel;
        private WebView _webView;

        public QrScannerPage()
        {
            InitializeComponent();
            _viewModel = new QrScannerViewModel(IPlatformApplication.Current.Services.GetService<IMqqtService>());
            BindingContext = _viewModel;

            // Inizializza il WebView per la scansione QR
            InitializeScanner();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void InitializeScanner()
        {
            // Crea un WebView per la scansione QR
            _webView = new WebView
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Black
            };

            // HTML per la scansione QR code - versione corretta senza zoom eccessivo
            string html = @"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1'>
    <title>QR Scanner</title>
    <script src='https://unpkg.com/html5-qrcode@2.3.8/html5-qrcode.min.js'></script>
    <style>
        body, html { 
            margin: 0; 
            padding: 0; 
            height: 100vh; 
            width: 100vw; 
            overflow: hidden; 
            background-color: #000; 
            display: flex;
            justify-content: center;
            align-items: center;
        }
        
        #reader { 
            width: 100vw; 
            height: 100vh;
            max-width: 100%;
            max-height: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        
        /* Stili per nascondere elementi non necessari */
        #reader__dashboard_section {
            display: none !important;
        }
        
        #reader__scan_region {
            width: 100% !important;
            height: 100% !important;
            min-height: 100vh !important;
        }
        
        video {
            width: 100% !important;
            height: 100vh !important;
            object-fit: contain !important; /* Cambiato da 'cover' a 'contain' */
            background: black;
        }
    </style>
</head>
<body>
    <div id='reader'></div>
    
    <script>
        // Configurazione migliorata
        const html5QrCode = new Html5Qrcode('reader');
        const config = { 
            fps: 15,                 // Ridotto per migliori prestazioni
            qrbox: { width: 250, height: 250 },
            disableFlip: false,
            formatsToSupport: [ Html5QrcodeSupportedFormats.QR_CODE ]
        };
        
        // Rimuove elementi UI indesiderati e sistema il video
        function adjustVideoSettings() {
            const elements = document.querySelectorAll('#reader__dashboard_section, #reader__header_message');
            elements.forEach(el => {
                if (el) el.style.display = 'none';
            });
            
            // Migliora la regione di scansione
            const scanRegion = document.querySelector('#reader__scan_region');
            if (scanRegion) {
                scanRegion.style.width = '100%';
                scanRegion.style.height = '100vh';
            }
            
            // Regola il video senza zoom
            const video = document.querySelector('video');
            if (video) {
                video.style.width = '100%';
                video.style.height = '100vh';
                video.style.objectFit = 'contain'; // Fondamentale: usa 'contain' invece di 'cover'
                video.style.background = 'black';
            }
        }
        
        // Avvia la scansione
        html5QrCode.start(
            { facingMode: 'environment' }, 
            config, 
            (decodedText) => {
                // Invia il risultato all'app
                window.location.href = 'qrcode://' + encodeURIComponent(decodedText);
            },
            (errorMessage) => {
                // Ignora gli errori di scansione (è normale)
            }
        ).then(() => {
            console.log('Scanner started successfully');
            // Applica le impostazioni del video
            setTimeout(adjustVideoSettings, 500);
            setInterval(adjustVideoSettings, 2000); // Mantieni le impostazioni
        }).catch(err => {
            console.error('Error starting scanner:', err);
        });
        
        // Funzione per fermare lo scanner
        window.stopScanning = () => {
            if (html5QrCode.isScanning) {
                html5QrCode.stop();
            }
        };
    </script>
</body>
</html>";

            // Configura il WebView 
#if ANDROID
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("EnableCamera", (handler, view) =>
            {
                if (handler.PlatformView is Android.Webkit.WebView webView)
                {
                    webView.Settings.JavaScriptEnabled = true;
                    webView.Settings.MediaPlaybackRequiresUserGesture = false;
                    webView.Settings.SetGeolocationEnabled(true);
                    webView.Settings.AllowContentAccess = true;
                    webView.Settings.AllowFileAccess = true;
                    webView.Settings.DatabaseEnabled = true;
                    webView.Settings.DomStorageEnabled = true;
                    webView.Settings.SetPluginState(Android.Webkit.WebSettings.PluginState.On);

                    // Imposta un client personalizzato per gestire le richieste di permesso
                    webView.SetWebChromeClient(new MyChromeClient());
                }
            });
#endif

            _webView.Source = new HtmlWebViewSource { Html = html };
            _webView.Navigating += ScannerWebView_Navigating;

            // Aggiungi il WebView al container
            webViewContainer.Content = _webView;
        }

        private async void ScannerWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("qrcode://"))
            {
                e.Cancel = true;
                string result = Uri.UnescapeDataString(e.Url.Substring(9));

                await _webView.EvaluateJavaScriptAsync("stopScanning()");

                if (sender is WebView scannerWebView)
                {
                    scannerWebView.IsVisible = false;
                }

                webViewContainer.IsVisible = false;

                // Processa il risultato del QR code
                _viewModel.ProcessQrResult(result);

                await Navigation.PopModalAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Pulisci gli eventi
            if (_webView != null)
            {
                _webView.Navigating -= ScannerWebView_Navigating;

                // Ferma lo scanner
                _webView.EvaluateJavaScriptAsync("stopScanning()");
            }
        }
    }

#if ANDROID
    // Aggiungi questa classe nella stessa pagina o in un file separato
    public class MyChromeClient : Android.Webkit.WebChromeClient
    {
        public override void OnPermissionRequest(Android.Webkit.PermissionRequest request)
        {
            request.Grant(request.GetResources());
        }
    }
#endif
}