using MQTT.Models;
using System.Text.Json;
using UOMacroMobile.Services.Interfaces;
using static MQTT.Models.MqttNotificationModel;

namespace UOMacroMobile.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        public async Task ShowNotificationAsync(MqttNotificationModel notification)
        {
            var notificationId = new Random().Next(100000);
            if(notification.Type!= NotificationSeverity.Info)
            {
#if ANDROID
                await ShowAndroidNotificationAsync(notification, notificationId);
#elif IOS
            await ShowiOSNotificationAsync(notification, notificationId);
#endif
            }
        }

#if ANDROID
        private Task ShowAndroidNotificationAsync(MqttNotificationModel notification, int notificationId)
        {
            Console.WriteLine($"ShowAndroidNotificationAsync: tipo={notification.Type}, priorità alta={IsHighPriorityNotification(notification.Type)}");

            try
            {
                var context = Android.App.Application.Context;
                var notificationManager = context.GetSystemService(Android.Content.Context.NotificationService) as Android.App.NotificationManager;

                if (notificationManager == null)
                {
                    Console.WriteLine("Errore: NotificationManager è null");
                    return Task.CompletedTask;
                }

                // Scegli il canale in base alla priorità
                string channelId = IsHighPriorityNotification(notification.Type)
                    ? "mqtt_high_priority"
                    : "mqtt_notifications";

                var applicationContext = Android.App.Application.Context;


                var largeIconBitmap = Android.Graphics.BitmapFactory.DecodeResource(
                    applicationContext.Resources,
                    Resource.Drawable.icon_round);

                // Costruisci la notifica
                var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
            .SetContentTitle(notification.Title)
            .SetContentText(notification.Message)
            .SetSmallIcon(Resource.Drawable.icon_round)
            .SetLargeIcon(largeIconBitmap)
            .SetAutoCancel(true);

                // Imposta priorità in base al tipo
                if (IsHighPriorityNotification(notification.Type))
                {
                    builder.SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh);
                    builder.SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm);
                    builder.SetVibrate(new long[] { 0, 250, 250, 250 });
                    Console.WriteLine("Impostata notifica ad alta priorità");
                }

                // Aggiunge dati personalizzati come intent extra
                var intent = new Android.Content.Intent(context, typeof(MainActivity));
                intent.PutExtra("notification_data", JsonSerializer.Serialize(notification));
                intent.AddFlags(Android.Content.ActivityFlags.ClearTop);

                var pendingIntentFlags = Android.App.PendingIntentFlags.UpdateCurrent;
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
                {
                    pendingIntentFlags |= Android.App.PendingIntentFlags.Immutable;
                }

                var pendingIntent = Android.App.PendingIntent.GetActivity(
                    context,
                    notificationId,
                    intent,
                    pendingIntentFlags);

                builder.SetContentIntent(pendingIntent);

                // Mostra la notifica
                notificationManager.Notify(notificationId, builder.Build());
                Console.WriteLine($"Notifica Android mostrata con ID {notificationId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la visualizzazione della notifica Android: {ex}");
            }

            return Task.CompletedTask;
        }
#endif

#if IOS
        private Task ShowiOSNotificationAsync(MqttNotificationModel notification, int notificationId)
        {
            // Richiedi autorizzazione per le notifiche (da chiamare all'avvio dell'app)
            UserNotifications.UNUserNotificationCenter.Current.RequestAuthorization(
                UserNotifications.UNAuthorizationOptions.Alert | 
                UserNotifications.UNAuthorizationOptions.Badge | 
                UserNotifications.UNAuthorizationOptions.Sound, 
                (approved, error) => { });
                
            // Crea il contenuto della notifica
            var content = new UserNotifications.UNMutableNotificationContent
            {
                Title = notification.Title,
                Body = notification.Message,
                Badge = 1,
                UserInfo = new Foundation.NSDictionary(
                    "data", 
                    new Foundation.NSString(JsonSerializer.Serialize(notification)))
            };
            
            // Imposta suono in base al tipo di notifica
            if (IsHighPriorityNotification(notification.Type))
            {
                content.Sound = UserNotifications.UNNotificationSound.DefaultCritical;
            }
            else
            {
                content.Sound = UserNotifications.UNNotificationSound.Default;
            }
            
            // Crea trigger per mostrare la notifica immediatamente
            var trigger = UserNotifications.UNTimeIntervalNotificationTrigger.CreateTrigger(0.1, false);
            
            // Crea la richiesta di notifica
            var request = UserNotifications.UNNotificationRequest.FromIdentifier(
                notificationId.ToString(), 
                content, 
                trigger);
                
            // Aggiungi la richiesta al centro notifiche
            UserNotifications.UNUserNotificationCenter.Current.AddNotificationRequest(
                request, 
                (error) => {
                    if (error != null)
                    {
                        Console.WriteLine($"Errore nella notifica iOS: {error}");
                    }
                });
                
            return Task.CompletedTask;
        }
#endif

        private bool IsHighPriorityNotification(NotificationSeverity type)
        {

            bool isHighPriority = type == NotificationSeverity.Warning || type == NotificationSeverity.Error;
            return isHighPriority;
        }
    }
}