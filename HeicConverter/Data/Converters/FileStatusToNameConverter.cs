using EnumsNET;
using System;

using Windows.UI.Xaml.Data;

namespace HeicConverter.Data.Converters
{
    public class FileStatusToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((FileStatus)value).AsString(EnumFormat.Description);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
