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
                SetButtonsEnabled(false);

                bool confirm = await DisplayAlert("Conferma", "Sei sicuro di voler avviare l'applicativo?", "Sì", "No");
                if(confirm)
                    await _viewModel.Start();
            }
            catch (Exception ex)
            {
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
                SetButtonsEnabled(false);

                bool confirm = await DisplayAlert("Conferma", "Sei sicuro di voler fermare l'applicativo?", "Sì", "No");
                if (confirm)
                    await _viewModel.Stop();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", $"Si è verificato un errore durante l'arresto: {ex.Message}", "OK");
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            var startButton = this.FindByName<Button>("StartButton");
            var stopButton = this.FindByName<Button>("StopButton");

            if (startButton != null) startButton.IsEnabled = enabled;
            if (stopButton != null) stopButton.IsEnabled = enabled;
        }
    }
}