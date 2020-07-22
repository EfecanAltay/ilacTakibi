using System;
using System.Globalization;
using Xamarin.Forms;

namespace ilacTakibi.Converters
{
    public class MedicineListItemUsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is bool)
                {
                    bool key = (bool)value;
                    var param = parameter as string;
                    if (key)
                    {
                        switch (param)
                        {
                            case "Background":
                                return Color.FromHex("#87DE84");
                            case "Border":
                                return Color.FromHex("#87DE84");
                            case "Bool":
                                return true;
                            case "BoolReverse":
                                return false;
                        }
                    }
                    else
                    {
                        switch (param)
                        {
                            case "Background":
                                return Color.FromHex("#E9B3B3");
                            case "Border":
                                return Color.FromHex("#E9B3B3");
                            case "Bool":
                                return false;
                            case "BoolReverse":
                                return true;
                        }
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
