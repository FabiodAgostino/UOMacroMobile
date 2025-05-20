namespace UOMacroMobile.Services.Interfaces
{
    public interface IDialogService
    {
        Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel);
        Task DisplayAlertAsync(string title, string message, string cancel);
        Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons);
    }
}
