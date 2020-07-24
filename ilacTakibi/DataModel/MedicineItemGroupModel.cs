using ilacTakibi.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace ilacTakibi.DataModel
{
    [Serializable]
    public class MedicineItemGroupedModel : ObservableCollection<MedicineItemModel>
    {
        private DateTime date;
        public DateTime Date
        {
            get => date.ToLocalTime();
            set
            {
                date = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Date)));
            }
        }

        public override string ToString()
        {
            string sObject = date.ToString("dd/MM/yyyy") + " Tarihli Kullanım Raporu \n";

            var notusedItems = Items.Where(x => x.IsUsed == false);
            var usedItems = Items.Where(x => x.IsUsed);

            sObject += "Kullanmadığınız İlaçlar ---------\n";
            if (notusedItems.Any() == false)
                sObject += "- Yok\n";
            notusedItems.ForEach(item =>
            {
                sObject += item.IlacTarihi.date.ToString("HH:mm") + " " + item.ilacIsmi + "\n";
            });

            sObject += "Kullandığınız İlaçlar ---------\n";
            if (usedItems.Any() == false)
                sObject += "- Yok\n";
            usedItems.ForEach(item =>
            {
                sObject += item.IlacTarihi.date.ToString("HH:mm") + " " + item.ilacIsmi + "\n";
            });

            return sObject;
        }
    }
}
