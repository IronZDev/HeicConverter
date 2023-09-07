using System;
using Windows.UI.Xaml.Data;

namespace HeicConverter.Data.Converters
{
    public class FileStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((FileStatus)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
