using ilacTakibi.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ilacTakibi.DataModel
{
    [Serializable]
    public class MedicineItemGroupedModel : ObservableCollection<MedicineItemModel>, INotifyCollectionChanged
    {
        private DateTime date;
        public DateTime Date {
            get => date.ToLocalTime();
            set
            {
                date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
