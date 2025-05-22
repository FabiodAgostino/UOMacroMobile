using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android.Content;
using UOMacroMobile.Platforms.Android;

namespace UOMacroMobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private const int CAMERA_PERMISSION_REQUEST_CODE = 100;
        private const int NOTIFICATION_PERMISSION_REQUEST_CODE = 101;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Richiedi i permessi in modo asincrono per non bloccare l'avvio
            RequestPermissionsAsync();

            // Inizializza il canale di notifica
            CreateNotificationChannel();

            // Avvia il servizio MQTT in background
            StartMqttBackgroundService(true);
        }

        private async void RequestPermissionsAsync()
        {
            // Lista dei permessi da richiedere
            var permissionsToRequest = new List<string>();

            // Controlla permesso fotocamera
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Camera) != Permission.Granted)
            {
                permissionsToRequest.Add(Android.Manifest.Permission.Camera);
            }

            // Controlla permesso notifiche per Android 13+
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.PostNotifications) != Permission.Granted)
                {
                    permissionsToRequest.Add(Android.Manifest.Permission.PostNotifications);
                }
            }

            // Richiedi i permessi se necessario
            if (permissionsToRequest.Count > 0)
            {
                // Aspetta un momento per permettere all'app di inizializzarsi completamente
                await Task.Delay(1000);

                ActivityCompat.RequestPermissions(this, permissionsToRequest.ToArray(), CAMERA_PERMISSION_REQUEST_CODE);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            AppStateManager.IsAppInForeground = true;
            UpdateMqttServiceStatus(true);
        }

        protected override void OnPause()
        {
            base.OnPause();
            AppStateManager.IsAppInForeground = false;
            UpdateMqttServiceStatus(false);
        }

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

        private void UpdateMqttServiceStatus(bool isAppInForeground)
        {
            var intent = new Intent(this, typeof(Platforms.Android.MqttBackgroundService));
            intent.PutExtra("isAppInForeground", isAppInForeground);
            StartService(intent);
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                // Canale per notifiche standard
                var channelId = "mqtt_notifications";
                var channelName = "Notifiche MQTT";
                var channel = new NotificationChannel(
                    channelId,
                    channelName,
                    NotificationImportance.Default);
                channel.EnableLights(true);
                channel.EnableVibration(true);

                var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                notificationManager?.CreateNotificationChannel(channel);

                // Canale ad alta priorità per warning ed errori
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == CAMERA_PERMISSION_REQUEST_CODE)
            {
                var cameraPermissionGranted = false;
                var notificationPermissionGranted = false;

                for (int i = 0; i < permissions.Length; i++)
                {
                    if (permissions[i] == Android.Manifest.Permission.Camera)
                    {
                        cameraPermissionGranted = grantResults[i] == Permission.Granted;
                        if (cameraPermissionGranted)
                        {
                            System.Console.WriteLine("✅ Permesso fotocamera concesso");
                        }
                        else
                        {
                            System.Console.WriteLine("❌ Permesso fotocamera negato");

                            // Controlla se l'utente ha selezionato "Non chiedere più"
                            bool shouldShowRationale = ActivityCompat.ShouldShowRequestPermissionRationale(this, Android.Manifest.Permission.Camera);
                            if (!shouldShowRationale)
                            {
                                System.Console.WriteLine("⚠️ L'utente ha negato definitivamente il permesso fotocamera");
                                ShowPermissionDeniedMessage();
                            }
                        }
                    }
                    else if (permissions[i] == Android.Manifest.Permission.PostNotifications)
                    {
                        notificationPermissionGranted = grantResults[i] == Permission.Granted;
                        if (notificationPermissionGranted)
                        {
                            System.Console.WriteLine("✅ Permesso notifiche concesso");
                        }
                        else
                        {
                            System.Console.WriteLine("❌ Permesso notifiche negato");
                        }
                    }
                }
            }
        }

        private void ShowPermissionDeniedMessage()
        {
            // Mostra un messaggio all'utente spiegando come abilitare manualmente i permessi
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            builder.SetTitle("Permesso Fotocamera Richiesto");
            builder.SetMessage("Per utilizzare lo scanner QR è necessario abilitare il permesso fotocamera dalle impostazioni dell'app.\n\nVuoi aprire le impostazioni ora?");
            builder.SetPositiveButton("Apri Impostazioni", (sender, e) =>
            {
                // Apri le impostazioni dell'app
                var intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                var uri = Android.Net.Uri.FromParts("package", PackageName, null);
                intent.SetData(uri);
                StartActivity(intent);
            });
            builder.SetNegativeButton("Annulla", (sender, e) => { });
            builder.Show();
        }

        // Metodo pubblico per verificare se i permessi della fotocamera sono concessi
        public bool IsCameraPermissionGranted()
        {
            return ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Camera) == Permission.Granted;
        }

        // Metodo per richiedere nuovamente i permessi se necessario
        public void RequestCameraPermissionAgain()
        {
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Camera) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Android.Manifest.Permission.Camera }, CAMERA_PERMISSION_REQUEST_CODE);
            }
        }
    }
}