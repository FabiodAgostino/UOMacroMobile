namespace UOMacroMobile
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();
                MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                // Logga l'eccezione su un file o mostrala in un alert
                System.Diagnostics.Debug.WriteLine($"Errore all'avvio: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Created += (s, e) =>
            {
                // Avvia il servizio in background
                StartBackgroundService();
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
