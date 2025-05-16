using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using UOMacroMobile.Pages;
using UOMacroMobile.Services.Implementations;
using UOMacroMobile.Services.Interfaces;
using UOMacroMobile.ViewModels;

namespace UOMacroMobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<NotificationsViewModel>();
            builder.Services.AddSingleton<QrScannerViewModel>();
            builder.Services.AddSingleton<IQrScannerService, QrScannerService>();

            builder.Services.AddTransient<QrScannerPage>();

            builder.Services.AddSingleton<IMqqtService, MqttService>();
            // Registrazione delle pagine
            builder.Services.AddSingleton<INotificationService, NotificationService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
