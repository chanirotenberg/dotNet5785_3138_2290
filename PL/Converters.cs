using BO;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PL
{
    public class ConvertUpdateToTrue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value == "Update";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CallInProgressToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CallInProgress call)
                return value.ToString(); // או לבנות תיאור מותאם אישית כאן

            return "NONE";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
