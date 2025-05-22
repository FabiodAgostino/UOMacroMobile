using UOMacroMobile.Services.Interfaces;
using UOMacroMobile.ViewModels;

namespace UOMacroMobile.Pages
{
    public partial class ActionsPage : ContentPage
    {
        private ActionsViewModel _viewModel;

        public ActionsPage()
        {
            InitializeComponent();

            // Ottieni il servizio MQTT e crea il ViewModel
            var mqttService = IPlatformApplication.Current.Services.GetService<IMqqtService>();
            _viewModel = new ActionsViewModel(mqttService);
            BindingContext = _viewModel;
        }

        private async void OnStartClicked(object sender, EventArgs e)
        {
            try
            {
                // Disabilita il pulsante durante l'operazione
                if (sender is Button button)
                {
                    button.IsEnabled = false;
                    button.Text = "Avvio...";
                }

                await _viewModel.Start();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", $"Errore durante l'avvio: {ex.Message}", "OK");
            }
            finally
            {
                // Riabilita il pulsante
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                    button.Text = "START";
                }
            }
        }

        private async void OnStopClicked(object sender, EventArgs e)
        {
            try
            {
                // Disabilita il pulsante durante l'operazione
                if (sender is Button button)
                {
                    button.IsEnabled = false;
                    button.Text = "Arresto...";
                }

                await _viewModel.Stop();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", $"Errore durante l'arresto: {ex.Message}", "OK");
            }
            finally
            {
                // Riabilita il pulsante
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                    button.Text = "STOP";
                }
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                // Mostra una conferma prima di procedere
                bool confirm = await DisplayAlert(
                    "Conferma Logout",
                    "Sei sicuro di voler chiudere TM Client? Questa azione fermerà completamente l'applicativo.",
                    "Sì, Logout",
                    "Annulla");

                if (!confirm)
                    return;

                // Disabilita il pulsante durante l'operazione
                if (sender is Button button)
                {
                    button.IsEnabled = false;
                    button.Text = "Logout...";
                }

                await _viewModel.Logout();

                // Mostra messaggio di conferma
                await DisplayAlert("Logout Completato", "TM Client è stato chiuso con successo.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", $"Errore durante il logout: {ex.Message}", "OK");
            }
            finally
            {
                // Riabilita il pulsante
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                    button.Text = "LOGOUT";
                }
            }
        }
    }
}