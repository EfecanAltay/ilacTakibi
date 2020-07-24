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
    public class MedicineListPageViewModel : BaseViewModel
    {
        private IMedicineTrackingAPI _api = null;
        private CacheService _cacheService = null;
        INotificationManager notificationManager;

        public Action<MedicineItemModel> CurrentMedicineAction;

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

        private List<MedicineItemModel> currentlyItems = new List<MedicineItemModel>();

        public MedicineListPageViewModel()
        {
            App.referenceViewModel = this;
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
        }

        private bool haveScrollAction = false;

        private void ShowNotification(string title, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var msg = new Label()
                {
                    Text = $"Notification Received:\nTitle: {title}\nMessage: {message}"
                };
            });
        }

        public void NotifyAction()
        {
            if (currentlyItems.Any())
            {
                string title = $"İlaç Kullanma Zamanı !";
                string message = "Kullanman gereken ilacın var.";
                notificationManager.ScheduleNotification(title, message);
            }
        }

        public async Task FetchMedicineList()
        {
            IsBusy = true;
            haveScrollAction = true;
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
                    IsBusy = false;
                });
            }
            else
                IsBusy = false;
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
            currentlyItems.Clear();
            Task.WhenAll
            (
                FetchMedicineList(),
                UpdateLiveMedicineList()
            );
        });

        private bool EquelsNowMinute(DateTime date)
        {
            var nowDate = DateTime.Now;
            var vDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            var nDate = new DateTime(nowDate.Year, nowDate.Month, nowDate.Day, nowDate.Hour, nowDate.Minute, 0);
            return vDate.Equals(nDate);
        }

        public async Task UpdateLiveMedicineList()
        {
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
                        if (EquelsNowMinute(newGroup.Date) && haveScrollAction)
                        {
                            currentlyItems.Add(item);
                            CurrentMedicineAction?.Invoke(item);
                            haveScrollAction = false;
                        }
                    });
                    orderedList.Add(newGroup);
                });
            }
            if (orderedList != null && orderedList.Any())
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    LiveMedicineList = new ObservableCollection<MedicineItemGroupedModel>(orderedList.OrderBy(x => x.Date));
                });
            }
        }

        public ICommand AddForAlertCommand => new Command((item) =>
        {
            var data = item as MedicineItemModel;
            //NotifyAction();
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

        //Email Section is Test Mode 
        public ICommand ShareCommand => new Command(async (toEmail) =>
        {
            string body = "";
            UsedMedicineList.ForEach(group =>
            {
                body += group.ToString() + "\n";
            });
            //if (toEmail != null) for test
            if(true)
            {
                //var s_email = (List<string>)toEmail; for test
                var s_email = new List<string>();
                var result = await Application.Current.MainPage.DisplayPromptAsync("Mail Gönder", "Lütfen Mail adresi veya adresleri giriniz.\nörnek : example1@example.com;example2@example.com");
                var mails = result.Trim().Split(';');
                mails.ForEach(mail => { 
                    s_email.Add(mail);
                });
                if (s_email.Any())
                {
                    try
                    {
                        var message = new EmailMessage
                        {
                            Subject = "İlaç Kullanım Raporu",
                            Body = body,
                            To = s_email,
                            //Cc = ccRecipients,
                            //Bcc = bccRecipients
                        };
                        await Email.ComposeAsync(message);
                    }
                    catch (FeatureNotSupportedException fbsEx)
                    {
                        await Application.Current.MainPage.DisplayAlert("Hata", fbsEx.Message, "Tamam");
                    }
                    catch (Exception ex)
                    {
                        await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
                    }
                }
            }
        });

        public ICommand ClearAllDataCommand => new Command(async () =>
        {
            await _cacheService.ClearAllData();
        });
    }
}
