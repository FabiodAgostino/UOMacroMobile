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

            // Registra tutti i ViewModels
            builder.Services.AddSingleton<NotificationsViewModel>();
            builder.Services.AddSingleton<QrScannerViewModel>();
            builder.Services.AddSingleton<ActionsViewModel>();

            // Registra tutte le pagine
            builder.Services.AddTransient<QrScannerPage>();
            builder.Services.AddTransient<NotificationsPage>();
            builder.Services.AddTransient<ActionsPage>();

            // Altri servizi
            builder.Services.AddSingleton<IMqqtService, MqttService>();
            builder.Services.AddSingleton<IQrScannerService, QrScannerService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();

            return builder.Build();
        }
    }
}
