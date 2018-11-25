using System;
using System.Globalization;
using Xamarin.Forms;

namespace WheelSpinner
{
    public class ObjToStr : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"You've selected: {value?.ToString() ?? "-"}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}