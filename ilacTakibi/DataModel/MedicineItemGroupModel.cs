using ilacTakibi.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace ilacTakibi.DataModel
{
    [Serializable]
    public class MedicineItemGroupedModel : ObservableCollection<MedicineItemModel>
    {
        private DateTime date;
        public DateTime Date {
            get => date.ToLocalTime();
            set
            {
                date = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Date)));
            }
        }
    }
}
