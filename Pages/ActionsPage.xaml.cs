using UOMacroMobile.ViewModels;
using UOMacroMobile.Services.Interfaces;

namespace UOMacroMobile.Pages
{
    public partial class ActionsPage : ContentPage
    {
        private readonly ActionsViewModel _viewModel;

        public ActionsPage(ActionsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
        private async void OnStartClicked(object sender, EventArgs e)
        {
            try
            {
                // Disabilitiamo i pulsanti durante l'operazione
                SetButtonsEnabled(false);

                // Chiamiamo il metodo Start del ViewModel
                await _viewModel.Start();

                // Mostriamo un messaggio di conferma
                await DisplayAlert("Applicativo Avviato", "L'applicativo è stato avviato con successo.", "OK");
            }
            catch (Exception ex)
            {
                // Gestiamo eventuali errori
                await DisplayAlert("Errore", $"Si è verificato un errore durante l'avvio: {ex.Message}", "OK");
            }
            finally
            {
                // Riabilitiamo i pulsanti
                SetButtonsEnabled(true);
            }
        }

        private async void OnStopClicked(object sender, EventArgs e)
        {
            try
            {
                // Disabilitiamo i pulsanti durante l'operazione
                SetButtonsEnabled(false);

                // Chiediamo conferma prima di fermare l'applicativo
                bool confirm = await DisplayAlert("Conferma", "Sei sicuro di voler fermare l'applicativo?", "Sì", "No");

                if (confirm)
                {
                    // Chiamiamo il metodo Stop del ViewModel
                    await _viewModel.Stop();

                    // Mostriamo un messaggio di conferma
                    await DisplayAlert("Applicativo Fermato", "L'applicativo è stato fermato con successo.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Gestiamo eventuali errori
                await DisplayAlert("Errore", $"Si è verificato un errore durante l'arresto: {ex.Message}", "OK");
            }
            finally
            {
                // Riabilitiamo i pulsanti
                SetButtonsEnabled(true);
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            // Metodo di utilità per abilitare/disabilitare i pulsanti
            var startButton = this.FindByName<Button>("StartButton");
            var stopButton = this.FindByName<Button>("StopButton");

            if (startButton != null) startButton.IsEnabled = enabled;
            if (stopButton != null) stopButton.IsEnabled = enabled;
        }
    }
}