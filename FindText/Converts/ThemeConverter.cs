using System;
using System.Globalization;
using System.Windows.Data;

namespace FindText.Converts
{
    internal class InvertBoolConverter : IValueConverter
    {
        public static readonly InvertBoolConverter Instance = new InvertBoolConverter();
        InvertBoolConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
