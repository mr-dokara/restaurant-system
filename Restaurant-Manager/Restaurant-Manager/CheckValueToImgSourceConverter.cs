using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DatabaseConnectionLib;

namespace Restaurant_Manager
{
    public class CheckValueToImgSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                ImageSourceConverter imgsc = new ImageSourceConverter();
                ImageSource source = (ImageSource) imgsc.ConvertFrom(value as string);
                return source;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
            //throw new NotImplementedException();
        }
    }
}