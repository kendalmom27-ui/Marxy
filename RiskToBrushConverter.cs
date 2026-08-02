using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RasTweaksCS
{
    public class RiskToBrushConverter : IValueConverter
    {
        public static readonly SolidColorBrush Safe = new((Color)ColorConverter.ConvertFromString("#4CAF50"));
        public static readonly SolidColorBrush Caution = new((Color)ColorConverter.ConvertFromString("#FFB74D"));
        public static readonly SolidColorBrush Security = new((Color)ColorConverter.ConvertFromString("#FF5C5C"));

        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string) switch
            {
                "Caution" => Caution,
                "Security" => Security,
                _ => Safe,
            };
        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class CategoryToIconConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string) switch
            {
                "Network" => "🌐",
                "Power" => "⚡",
                "Boot" => "🖥️",
                "System" => "🔧",
                "Kernel" => "💻",
                "GPU" => "🎮",
                "Aim" => "🎯",
                _ => "✨",
            };
        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
