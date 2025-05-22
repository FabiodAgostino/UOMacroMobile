// Pages/QrScannerPage.xaml.cs
using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
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
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // CORREZIONE: Chiama l'inizializzazione asincrona
            await _viewModel.InitializeAsync();

            // Inizializza il WebView SOLO dopo che i permessi sono stati verificati
            if (_viewModel.IsScanning && !_viewModel.CameraDenied)
            {
                InitializeScanner();
            }
        }

        // Sposta la sottoscrizione agli eventi del ViewModel
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (_viewModel != null)
            {
                // Sottoscrivi ai cambiamenti di IsScanning
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.IsScanning))
            {
                if (_viewModel.IsScanning && _webView == null)
                {
                    InitializeScanner();
                }
            }
        }

        private void InitializeScanner()
        {
            if (_webView != null)
                return;

            _webView = new WebView
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Black
            };

#if ANDROID
            // Configurazione avanzata per Android
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("ConfigureWebView", (handler, view) =>
            {
                if (handler.PlatformView is Android.Webkit.WebView androidWebView)
                {
                    var settings = androidWebView.Settings;

                    // Abilita JavaScript
                    settings.JavaScriptEnabled = true;

                    // Permessi per media e geolocalizzazione
                    settings.MediaPlaybackRequiresUserGesture = false;
                    settings.SetGeolocationEnabled(true);

                    // Accesso ai file e contenuti
                    settings.AllowContentAccess = true;
                    settings.AllowFileAccess = true;
                    settings.AllowFileAccessFromFileURLs = true;
                    settings.AllowUniversalAccessFromFileURLs = true;

                    // Database e storage
                    settings.DatabaseEnabled = true;
                    settings.DomStorageEnabled = true;

                    // Supporto per media moderni
                    settings.SetPluginState(Android.Webkit.WebSettings.PluginState.On);

                    // CRITICO: Imposta il WebChromeClient personalizzato
                    androidWebView.SetWebChromeClient(new MyChromeClient());

                    Console.WriteLine("WebView Android configurato per fotocamera");
                }
            });
#endif

            // HTML migliorato con gestione errori dettagliata
            string html = GetScannerHtml();

            _webView.Source = new HtmlWebViewSource { Html = html };
            _webView.Navigating += ScannerWebView_Navigating;

            webViewContainer.Content = _webView;
        }

        private async void ScannerWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("qrcode://"))
            {
                e.Cancel = true;
                string result = Uri.UnescapeDataString(e.Url.Substring(9));

                // Ferma lo scanner
                if (_webView != null)
                {
                    try
                    {
                        await _webView.EvaluateJavaScriptAsync("stopScanning()");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Errore nel fermare lo scanner: {ex.Message}");
                    }
                }

                // Processa il risultato
                _viewModel.ProcessQrResult(result);

                await Navigation.PopModalAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Cleanup
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            if (_webView != null)
            {
                try
                {
                    _webView.Navigating -= ScannerWebView_Navigating;
                    _webView.EvaluateJavaScriptAsync("stopScanning()");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore nel cleanup: {ex.Message}");
                }
            }
        }

        private string GetScannerHtml()
        {
            return @"
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
            overflow: hidden;
        }
        
        #reader { 
            width: 100vw; 
            height: 100vh; 
            position: relative;
        }
        
        #reader video {
            width: 100% !important;
            height: 100% !important;
            object-fit: contain !important;
            position: absolute !important;
            top: 0 !important;
            left: 0 !important;
            transform: none !important;
        }
        
        /* Nascondi TUTTI gli angoli quadrati */
        #reader canvas,
        #reader div[style*='position: absolute'][style*='border'],
        #reader div[style*='border'][style*='absolute'],
        .qr-code-region-highlight-svg,
        .qr-code-region-highlight {
            display: none !important;
        }
        
        /* Nascondi testi inutili */
        #reader > div > div:last-child {
            display: none !important;
        }
    </style>
</head>
<body>
    <div id='reader'></div>
    
    <script>
        async function initScanner() {
            try {
                if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                    return;
                }
                
                const testStream = await navigator.mediaDevices.getUserMedia({ 
                    video: { 
                        facingMode: 'environment'
                    } 
                });
                testStream.getTracks().forEach(track => track.stop());
                
                const html5QrCode = new Html5Qrcode('reader');
                
                const config = { 
                    fps: 10,
                    qrbox: { width: 250, height: 250 },
                    aspectRatio: 1.0,
                    disableFlip: false,
                    showTorchButtonIfSupported: false,
                    showZoomSliderIfSupported: false,
                    defaultZoomValueIfSupported: 1
                };
                
                await html5QrCode.start(
                    { facingMode: 'environment' },
                    config,
                    (decodedText, decodedResult) => {
                        html5QrCode.stop().then(() => {
                            window.location.href = 'qrcode://' + encodeURIComponent(decodedText);
                        });
                    },
                    (errorMessage) => {
                        // Errori ignorati
                    }
                );
                
                setTimeout(() => {
                    removeUnwantedElements();
                }, 1500);
                
                window.stopScanning = () => {
                    if (html5QrCode.isScanning) {
                        html5QrCode.stop();
                    }
                };
                
            } catch (error) {
                console.error('Scanner error:', error.message);
            }
        }
        
        function removeUnwantedElements() {
            const reader = document.getElementById('reader');
            const video = reader.querySelector('video');
            
            if (video) {
                // IMPORTANTE: object-fit contain per evitare zoom
                video.style.width = '100%';
                video.style.height = '100%';
                video.style.objectFit = 'contain';
                video.style.position = 'absolute';
                video.style.top = '0';
                video.style.left = '0';
                video.style.transform = 'none';
                video.style.zoom = '1';
            }
            
            // Rimuovi TUTTI gli elementi che potrebbero essere angoli
            const elementsToRemove = reader.querySelectorAll('canvas, svg, div[style*=border]');
            elementsToRemove.forEach(element => {
                const style = element.getAttribute('style') || '';
                if (style.includes('position: absolute') || 
                    style.includes('border') || 
                    element.tagName === 'CANVAS' ||
                    element.tagName === 'SVG') {
                    element.style.display = 'none';
                }
            });
            
            // Nascondi testi
            const textElements = reader.querySelectorAll('div');
            textElements.forEach(element => {
                if (element.textContent && (
                    element.textContent.includes('Unable') || 
                    element.textContent.includes('Scanning') ||
                    element.textContent.includes('Code'))) {
                    element.style.display = 'none';
                }
            });
        }
        
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initScanner);
        } else {
            initScanner();
        }
    </script>
</body>
</html>";
        }

    }

#if ANDROID
    public class MyChromeClient : Android.Webkit.WebChromeClient
    {
        public override void OnPermissionRequest(Android.Webkit.PermissionRequest request)
        {
            // Concedi esplicitamente tutti i permessi richiesti
            var resources = request.GetResources();
            Console.WriteLine($"WebView richiede permessi: {string.Join(", ", resources)}");
            request.Grant(resources);
        }

        // Gestione permessi per versioni precedenti di Android
        public override void OnPermissionRequestCanceled(Android.Webkit.PermissionRequest request)
        {
            base.OnPermissionRequestCanceled(request);
            Console.WriteLine("Permessi WebView cancellati");
        }
    }
#endif
}