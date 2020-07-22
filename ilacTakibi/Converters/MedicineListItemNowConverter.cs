using ilacTakibi.Services;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace ilacTakibi.Converters
{
    public class MedicineListItemNowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                if(value is MedicineItemModel)
                {
                    MedicineItemModel item = (MedicineItemModel)value;
                    var date = item.IlacTarihi.date;
                    var nowDate = DateTime.Now;
                    var vDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                    var nDate = new DateTime(nowDate.Year, nowDate.Month, nowDate.Day, nowDate.Hour, nowDate.Minute, 0);
                    var param = parameter as string;
                    if (vDate.Equals(nDate))
                    {
                        switch (param)
                        {
                            case "Background":
                                return Color.FromHex("#0EA8A8");
                            case "Done":
                                return !item.IsUsed;
                            case "SetAlert":
                                return false;
                        }
                    }
                    else
                    {
                        switch (param)
                        {
                            case "Background":
                                return Color.Transparent;
                            case "Done":
                                return date < nowDate && !item.IsUsed;
                            case "SetAlert":
                                return date > nowDate;
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
