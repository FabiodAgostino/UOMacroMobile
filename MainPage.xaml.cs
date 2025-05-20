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

        private void ShowNotificheView()
        {
            // Ottieni una nuova istanza dalla DI
            var notificationsPage = IPlatformApplication.Current.Services.GetService<NotificationsPage>();
            MainContent.Content = new ContentView { Content = notificationsPage.Content };
        }

        private void ShowAzioniView()
        {
            // Ottieni una nuova istanza dalla DI
            var actionsPage = IPlatformApplication.Current.Services.GetService<ActionsPage>();
            MainContent.Content = new ContentView { Content = actionsPage.Content };
        }
    }
}