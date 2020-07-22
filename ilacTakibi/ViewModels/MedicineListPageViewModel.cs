using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ilacTakibi.DataModel;
using ilacTakibi.Services;
using Refit;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ilacTakibi.ViewModels
{
    public class MedicineListPageViewModel : BaseViewModel
    {
        private IMedicineTrackingAPI _api = null;
        private CacheService _cacheService = null;
        INotificationManager notificationManager;

        private ObservableCollection<MedicineItemGroupedModel> liveMedicineList { get; set; }
        public ObservableCollection<MedicineItemGroupedModel> LiveMedicineList
        {
            get { return liveMedicineList; }
            set
            {
                liveMedicineList = value;
                OnPropertyChanged(nameof(LiveMedicineList));
            }
        }

        private ObservableCollection<MedicineItemGroupedModel> usedMedicineList { get; set; }
        public ObservableCollection<MedicineItemGroupedModel> UsedMedicineList
        {
            get { return usedMedicineList; }
            set
            {
                usedMedicineList = value;
                OnPropertyChanged(nameof(UsedMedicineList));
            }
        }

        public MedicineListPageViewModel()
        {
            _api = RestService.For<IMedicineTrackingAPI>("http://gelisimedestekol.com/index.php");
            _cacheService = App.cacheService;
            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.Initialize();
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                var evtData = (NotificationEventArgs)eventArgs;
                ShowNotification(evtData.Title, evtData.Message);
            };
            GetLiveMedicineList.Execute(null);
            GetUsedMedicineList.Execute(null);
        }

        void ShowNotification(string title, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var msg = new Label()
                {
                    Text = $"Notification Received:\nTitle: {title}\nMessage: {message}"
                };
            });
        }

        void OnScheduleClick()
        {
            string title = $"Local Notification";
            string message = $"You have now received notifications!";
            notificationManager.ScheduleNotification(title, message);
        }

        public ICommand GetLiveMedicineList => new Command(async () =>
        {
            IsBusy = true;
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var response = await _api.GetList();
                    if (!response.isError)
                    {
                        var listData = response.value;
                        LiveMedicineList = new ObservableCollection<MedicineItemGroupedModel>();
                        listData.GroupBy(x => x.IlacTarihi.date.Date).ForEach((y) =>
                        {
                            var group = new MedicineItemGroupedModel() { Date = y.Key };
                            y.ForEach((m) =>
                            {
                                group.Add(m);
                            });
                            LiveMedicineList.Add(group);
                        });
                        if (LiveMedicineList != null)
                            _cacheService.SaveDataItems(LiveMedicineList);
                    }
                    else
                    {
                        //return err msg...
                    }
                });
                IsBusy = false;
            }
            else
            {
                var list = await _cacheService.GetListOnCache();
                if (list != null)
                    LiveMedicineList = new ObservableCollection<MedicineItemGroupedModel>();
                //..
                IsBusy = false;
            }
        });

        public ICommand GetUsedMedicineList => new Command(() =>
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var list = await _cacheService.GetListOnCache();
                if (list != null)
                    UsedMedicineList = new ObservableCollection<MedicineItemGroupedModel>(list);
            });
        });

        public ICommand NotifyCommand => new Command((item) =>
        {
            var data = item as MedicineItemModel;
            OnScheduleClick();
        });

        public ICommand UsedCommand => new Command((item) =>
        {
            var data = item as MedicineItemModel;
            if (data.IsUsed == false)
            {
                data.IsUsed = true;
                liveMedicineList.ForEach(x =>
                {
                    if (x.Contains(data))
                    {
                        data.IsUsed = true;
                        _cacheService.SaveDataItem(x);
                        return;
                    }
                 });
            }
        });

        public ICommand ClearAllDataCommand => new Command(async () =>
        {
            await _cacheService.ClearAllData();
        });
    }
}
