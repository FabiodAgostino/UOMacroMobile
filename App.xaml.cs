using System.Diagnostics;
using UOMacroMobile.Services.Interfaces;

namespace UOMacroMobile
{
    public partial class App : Application
    {
        private IMqqtService _mqttService;
        public App()
        {
            try
            {
                InitializeComponent();
                MainPage = new AppShell();
                _mqttService = IPlatformApplication.Current.Services.GetService<IMqqtService>();
            }
            catch (Exception ex)
            {
                // Logga l'eccezione su un file o mostrala in un alert
                System.Diagnostics.Debug.WriteLine($"Errore all'avvio: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        protected override void OnResume()
        {
            _mqttService.SmartphoneIsAvailable();
            base.OnResume();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            StartBackgroundService();

            window.Created += async (s, e) =>
            {
                try
                {
                    await _mqttService.ConnectAsync(true);
                    Console.WriteLine("Connessione MQTT completata con successo");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore durante la connessione MQTT: {ex.Message}");
                }
            };

            return window;
        }

        private void StartBackgroundService()
        {
#if ANDROID
            var intent = new Android.Content.Intent(Android.App.Application.Context, typeof(Platforms.Android.MqttBackgroundService));
            Android.App.Application.Context.StartForegroundService(intent);
#endif
        }
    }
}
