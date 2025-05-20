using System.Globalization;
using static MQTT.Models.MqttNotificationModel;

namespace UOMacroMobile.Converters
{
    public class SeverityToTitleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NotificationSeverity severity)
            {
                return severity switch
                {
                    NotificationSeverity.Warning => Color.FromArgb("#FFC107"), // Giallo
                    NotificationSeverity.Error => Color.FromArgb("#FF5252"),   // Rosso
                    _ => Colors.White                                          // Info - bianco
                };
            }
            return Colors.White; // Default
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConnectionStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected ? Colors.Green : Colors.Red;
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SeverityToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NotificationSeverity severity)
            {
                return severity switch
                {
                    NotificationSeverity.Info => Color.FromArgb("#4CAF50"),    // Verde
                    NotificationSeverity.Warning => Color.FromArgb("#FFC107"), // Giallo
                    NotificationSeverity.Error => Color.FromArgb("#FF5252"),   // Rosso
                    _ => Color.FromArgb("#4CAF50")                             // Default - verde
                };
            }
            return Color.FromArgb("#4CAF50"); // Default - verde
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullFilterSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedSeverity = value as NotificationSeverity?;
            return !selectedSeverity.HasValue ? Color.FromArgb("#FFFFFF") : Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InfoFilterSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedSeverity = value as NotificationSeverity?;
            return selectedSeverity.HasValue && selectedSeverity.Value == NotificationSeverity.Info
                ? Color.FromArgb("#FFFFFF")
                : Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WarningFilterSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedSeverity = value as NotificationSeverity?;
            return selectedSeverity.HasValue && selectedSeverity.Value == NotificationSeverity.Warning
                ? Color.FromArgb("#FFFFFF")
                : Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ErrorFilterSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedSeverity = value as NotificationSeverity?;
            return selectedSeverity.HasValue && selectedSeverity.Value == NotificationSeverity.Error
                ? Color.FromArgb("#FFFFFF")
                : Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}