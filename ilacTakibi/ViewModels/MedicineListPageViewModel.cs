using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Input;
using ilacTakibi.DataModel;
using ilacTakibi.Services;
using Refit;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ilacTakibi.ViewModels
{
    public class MedicineListPageViewModel : BaseViewModel, IDisposable
    {
        private IMedicineTrackingAPI _api = null;
        private CacheService _cacheService = null;
        INotificationManager notificationManager;

        public Action<MedicineItemModel> CurrentMedicineAction;
        public Timer timer;

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

        public ObservableCollection<MedicineItemGroupedModel> MedicineList { get; set; }

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
            GetLivedMedicineList.Execute(null);
            GetUsedMedicineList.Execute(null);
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private bool haveScrollAction = false;

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(DateTime.Now.Second == 0)
                GetLivedMedicineList.Execute(null);
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

        public async Task FetchMedicineList()
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
                        MedicineList = new ObservableCollection<MedicineItemGroupedModel>();
                        listData.GroupBy(x => x.IlacTarihi.date.Date).ForEach((y) =>
                        {
                            var group = new MedicineItemGroupedModel() { Date = y.Key };
                            y.ForEach((m) =>
                            {
                                group.Add(m);
                            });
                            MedicineList.Add(group);
                        });
                        if (MedicineList != null)
                            _cacheService.SaveDataItems(MedicineList.OrderBy(x => x.Date));
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
        }

        public ICommand GetUsedMedicineList => new Command(() =>
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var list = await _cacheService.GetListOnCache();
                List<MedicineItemGroupedModel> orderedList = new List<MedicineItemGroupedModel>();
                if (list != null)
                {
                    list.ForEach(y =>
                    {
                        var newGroup = new MedicineItemGroupedModel();
                        y.Where(x => x.IlacTarihi.date <= DateTime.Now).OrderByDescending(z => z.IlacTarihi.date).ForEach(item =>
                        {
                            newGroup.Add(item);
                            newGroup.Date = item.IlacTarihi.date;
                        });
                        orderedList.Add(newGroup);
                    });
                    UsedMedicineList = new ObservableCollection<MedicineItemGroupedModel>(orderedList.OrderByDescending(x => x.Date));
                }
            });
        });

        public ICommand GetLivedMedicineList => new Command(() =>
       {
           haveScrollAction = true;
           Task.Run(async () =>
           {
               await FetchMedicineList();
               var list = await _cacheService.GetListOnCache();
               List<MedicineItemGroupedModel> orderedList = new List<MedicineItemGroupedModel>();
               if (list != null)
               {
                   list.ForEach(y =>
                    {
                        var newGroup = new MedicineItemGroupedModel();
                        y.OrderBy(z => z.IlacTarihi.date).ForEach(item =>
                        {
                            newGroup.Add(item);
                            newGroup.Date = item.IlacTarihi.date;
                            DateTime date = newGroup.Date;
                            var nowDate = DateTime.Now;
                            var vDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                            var nDate = new DateTime(nowDate.Year, nowDate.Month, nowDate.Day, nowDate.Hour, nowDate.Minute, 0);
                            if (vDate.Equals(nDate) && haveScrollAction)
                            {
                                CurrentMedicineAction?.Invoke(item);
                                haveScrollAction = false;
                            }
                        });
                        orderedList.Add(newGroup);
                    });
                   Device.BeginInvokeOnMainThread(async () =>
                   {
                       LiveMedicineList = new ObservableCollection<MedicineItemGroupedModel>(orderedList.OrderBy(x => x.Date));
                   });
               }
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
                LiveMedicineList.ForEach(x =>
                {
                    if (x.Contains(data))
                    {
                        var index = x.IndexOf(data);
                        x.Remove(data);
                        data.IsUsed = true;
                        x.Insert(index, data);
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

        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
