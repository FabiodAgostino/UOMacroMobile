using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Startup;
using UOMacroMobile.Platforms.Android;

namespace UOMacroMobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private bool _initialized = false;  
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (!_initialized)
            {
                _initialized = true;

                // Richiedi i permessi di notifica per Android 13+ (API 33+)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    // Verifica se abbiamo già il permesso
                    if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.PostNotifications)
                        != Permission.Granted)
                    {
                        // Richiedi il permesso
                        ActivityCompat.RequestPermissions(this,
                            new string[] { Android.Manifest.Permission.PostNotifications },
                            100); // ID richiesta arbitrario
                    }
                }

                // Inizializza il canale di notifica
                CreateNotificationChannel();

                // Avvia il servizio MQTT in background (prima volta)
                StartMqttBackgroundService(true);
            }
           
        }

        // NUOVO: Gestione dello stato quando l'app va in foreground
        protected override void OnResume()
        {
            base.OnResume();

            // Imposta lo stato dell'app come in foreground
            AppStateManager.IsAppInForeground = true;

            // Informa il servizio dello stato aggiornato
            UpdateMqttServiceStatus(true);
        }

        // NUOVO: Gestione dello stato quando l'app va in background
        protected override void OnPause()
        {
            base.OnPause();

            // Imposta lo stato dell'app come in background
            AppStateManager.IsAppInForeground = false;

            // Informa il servizio dello stato aggiornato
            UpdateMqttServiceStatus(false);
        }

        // NUOVO: Metodo per avviare il servizio MQTT in background
        private void StartMqttBackgroundService(bool isAppInForeground)
        {
            var intent = new Intent(this, typeof(Platforms.Android.MqttBackgroundService));
            intent.PutExtra("isAppInForeground", isAppInForeground);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                StartForegroundService(intent);
            }
            else
            {
                StartService(intent);
            }
        }

        // NUOVO: Metodo per aggiornare lo stato nel servizio MQTT
        private void UpdateMqttServiceStatus(bool isAppInForeground)
        {
            var intent = new Intent(this, typeof(Platforms.Android.MqttBackgroundService));
            intent.PutExtra("isAppInForeground", isAppInForeground);
            StartService(intent);
        }

        // Metodo per creare il canale di notifica all'avvio dell'app
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                // Crea il canale per notifiche standard
                var channelId = "mqtt_notifications";
                var channelName = "Notifiche MQTT";
                var channel = new NotificationChannel(
                    channelId,
                    channelName,
                    NotificationImportance.Default);
                channel.EnableLights(true);
                channel.EnableVibration(true);

                // Registra il canale nel sistema
                var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                notificationManager?.CreateNotificationChannel(channel);

                // Crea anche un canale ad alta priorità per warning ed errori
                var highPriorityChannelId = "mqtt_high_priority";
                var highPriorityChannelName = "Notifiche MQTT Importanti";
                var highPriorityChannel = new NotificationChannel(
                    highPriorityChannelId,
                    highPriorityChannelName,
                    NotificationImportance.High);
                highPriorityChannel.EnableLights(true);
                highPriorityChannel.EnableVibration(true);
                notificationManager?.CreateNotificationChannel(highPriorityChannel);
            }
        }

        // Gestione della risposta alla richiesta dei permessi
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == 100)
            {
                // Verifica se il permesso è stato concesso
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    System.Console.WriteLine("Permesso notifiche concesso");
                }
                else
                {
                    System.Console.WriteLine("Permesso notifiche negato");
                    // Qui potresti mostrare un messaggio all'utente spiegando 
                    // che le notifiche sono importanti per l'app
                }
            }
        }
    }
}