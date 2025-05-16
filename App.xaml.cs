namespace UOMacroMobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
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
