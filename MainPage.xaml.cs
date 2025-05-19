using UOMacroMobile.Pages;

namespace UOMacroMobile
{
    public partial class MainPage : ContentPage
    {
        // Dichiarazione come variabili locali
        private NotificationsPage _notificationsView;
        private ActionsPage _actionsView;

        public MainPage()
        {
            InitializeComponent();

            // Inizializza e mostra la vista predefinita
            ShowNotificheView();
        }

        private void OnNotificheTapped(object sender, EventArgs e)
        {
            ShowNotificheView();
        }

        private void OnAzioniTapped(object sender, EventArgs e)
        {
            ShowAzioniView();
        }

        private async void ShowNotificheView()
        {
            // Lazy initialization
            if (_notificationsView == null)
            {
                _notificationsView = IPlatformApplication.Current.Services.GetService<NotificationsPage>();
            }

            // Assegna semplicemente la view
            MainContent.Content = _notificationsView.Content;
        }

        private void ShowAzioniView()
        {
            if (_actionsView == null)
            {
                _actionsView = IPlatformApplication.Current.Services.GetService<ActionsPage>();
            }

            MainContent.Content = _actionsView.Content;
        }
    }
}