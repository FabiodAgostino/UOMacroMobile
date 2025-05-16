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

            // HTML per la scansione QR code
            string html = @"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>QR Scanner</title>
    <script src='https://unpkg.com/html5-qrcode@2.3.8/html5-qrcode.min.js'></script>
    <style>
        body, html { 
            margin: 0; 
            padding: 0; 
            height: 100%; 
            width: 100%; 
            background-color: #000; 
        }
        
        #reader { 
            width: 100%; 
            height: 100%; 
        }
    </style>
</head>
<body>
    <div id='reader'></div>
    
    <script>
        // Configurazione minima
        const html5QrCode = new Html5Qrcode('reader');
        const config = { 
            fps: 10, 
            qrbox: 250,
        };
        
        // Avvia la scansione
        html5QrCode.start(
            { facingMode: 'environment' }, 
            config, 
            (decodedText) => {
                // Invia il risultato all'app
                window.location.href = 'qrcode://' + encodeURIComponent(decodedText);
            }
        ).catch(err => {
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