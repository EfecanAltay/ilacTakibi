using System;
using System.Globalization;
using Xamarin.Forms;

namespace ilacTakibi.Converters
{
    public class DateGroupHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                if(value is DateTime)
                {
                    var binding_date = (DateTime)value;
                    if (binding_date.Date.Equals(DateTime.Today.Date))
                    {
                        return "Bugün";
                    }
                    else
                    {
                        String.Format("0:d/M/yyyy", binding_date);
                    }
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
