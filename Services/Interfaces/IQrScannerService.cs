namespace UOMacroMobile.Services.Interfaces
{
    public interface IQrScannerService
    {
        Task<string> ScanQrCodeAsync();
    }
}
