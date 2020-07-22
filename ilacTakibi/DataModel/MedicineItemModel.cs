using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ilacTakibi.DataModel;

namespace ilacTakibi.Services
{
    public class MedicineItemModel : INotifyPropertyChanged
    {
        public string isim { get; set; }
        public string soyisim { get; set; }
        public string ilacIsmi { get; set; }
        public MedicineDate IlacTarihi { get; set; }

        private bool isUsed = false;
        public bool IsUsed
        {
            get { return isUsed; }
            set {
                isUsed = value;
                OnPropertyChanged(nameof(IsUsed));
            }
        }

        public bool IsNotUsed => !isUsed;

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
