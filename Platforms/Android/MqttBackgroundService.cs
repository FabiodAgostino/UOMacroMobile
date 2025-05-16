using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using UOMacroMobile.Services.Implementations;
using UOMacroMobile.Services.Interfaces;

namespace UOMacroMobile.Platforms.Android
{
    [Service(Exported = true)]
    public class MqttBackgroundService : Service
    {
        private const int NotificationId = 1000;
        private const string ChannelId = "mqtt_service_channel";
        private const string ChannelName = "MQTT Service";
        private IMqqtService _mqttService;
        private INotificationService _notificationService;

        // Aggiungi un campo statico per tenere traccia dello stato del servizio
        public static bool IsServiceRunning { get; private set; }

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _mqttService = MauiApplication.Current.Services.GetService<IMqqtService>();
            _notificationService = new NotificationService();

            // Imposta lo stato del servizio
            IsServiceRunning = true;

            // Controlla se l'app è in foreground dall'intent (questo è un controllo aggiuntivo)
            if (intent != null && intent.HasExtra("isAppInForeground"))
            {
                bool intentForegroundState = intent.GetBooleanExtra("isAppInForeground", false);
                // Aggiorna lo stato globale solo se necessario
                if (AppStateManager.IsAppInForeground != intentForegroundState)
                {
                    AppStateManager.IsAppInForeground = intentForegroundState;
                }
            }

            // Gestione delle notifiche ricevute
            _mqttService.NotificationReceived += async (s, notification) =>
            {
                // Mostra notifiche solo se l'app è in background
                if (!AppStateManager.IsAppInForeground)
                {
                    await _notificationService.ShowNotificationAsync(notification);
                }
            };

            // Crea il canale di notifica (solo per Android ≥ O)
            CreateNotificationChannel();

            // Prepara la notifica
            var notification = new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle("ROTMobile")
                .SetContentText("Servizio di monitoraggio attivo")
                .SetSmallIcon(Resource.Drawable.notification_icon_background)
                .SetOngoing(true)
                .Build();

            // Avvia il servizio in foreground, specificando il tipo solo su Android ≥ 10
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                // ForegroundServiceType.DataSync categorizza il servizio come data-sync
                StartForeground(NotificationId, notification, ForegroundService.TypeDataSync);
            }
            else
            {
                StartForeground(NotificationId, notification);
            }

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            IsServiceRunning = false;
            base.OnDestroy();
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var channel = new NotificationChannel(
                ChannelId,
                ChannelName,
                NotificationImportance.Low);

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}