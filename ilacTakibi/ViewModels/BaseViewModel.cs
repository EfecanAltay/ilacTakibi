using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ilacTakibi.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { OnPropertyChanged(nameof(IsBusy)); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { OnPropertyChanged(nameof(Title)); }
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
