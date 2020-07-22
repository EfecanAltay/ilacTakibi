using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using ilacTakibi.DataModel;
using Newtonsoft.Json;
using Splat;
using Xamarin.Forms.Internals;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace ilacTakibi.Services
{
    public class CacheService
    {
        public IBlobCache LocalMachineCache { get; set; }
        private IFilesystemProvider _filesystemProvider;

        public void Init()
        {
            _filesystemProvider = Locator.Current.GetService<IFilesystemProvider>();
            GetLocalMachineCache();
        }

        private void GetLocalMachineCache()
        {
            /*
            var localCache = new Lazy<IBlobCache>(() =>
            {
                _filesystemProvider.CreateRecursive(_filesystemProvider.GetDefaultLocalMachineCacheDirectory()).SubscribeOn(BlobCache.TaskpoolScheduler).Wait();
                return new SQLitePersistentBlobCache(Path.Combine(_filesystemProvider.GetDefaultLocalMachineCacheDirectory(), "blobs.db"), BlobCache.TaskpoolScheduler);
            });
            */
            var settings = new JsonSerializerSettings();
            settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            Locator.CurrentMutable.RegisterConstant(settings, typeof(JsonSerializerSettings));

            BlobCache.ForcedDateTimeKind = DateTimeKind.Local;
            this.LocalMachineCache = BlobCache.LocalMachine;
        }

        public async Task<IEnumerable<MedicineItemGroupedModel>> GetListOnCache()
        {
            var listData = await LocalMachineCache.GetAllObjects<MedicineItemCacheModel>();
            List<MedicineItemGroupedModel> returnedList = new List<MedicineItemGroupedModel>();
            listData.ForEach(item =>
            {
                item.content.Date = item.date;
                returnedList.Add(item.content);
            });

            return returnedList;
        }

        private string ToMedicineItemKey(MedicineItemGroupedModel model)
        {
            return model.Date.Date.Ticks.ToString("X2");
        }

        public async Task<MedicineItemGroupedModel> GetMedicineItemOnCache(string key)
        {
            var cacheModel = await LocalMachineCache.GetObject<MedicineItemCacheModel>(key);
            return cacheModel.content;
        }

        public async void SaveDataItem(MedicineItemGroupedModel medicineItemModel, string key = null)
        {
            if (string.IsNullOrEmpty(key))
                key = ToMedicineItemKey(medicineItemModel);
            var cacheModel = new MedicineItemCacheModel(medicineItemModel);
            await LocalMachineCache.InsertObject(key, cacheModel);
        }

        public async void SaveDataItems(IEnumerable<MedicineItemGroupedModel> medicineItems)
        {
            var allkeys = (await LocalMachineCache.GetAllKeys()).ToList();

            medicineItems.ForEach(async (item) =>
            {
                var itemKey = ToMedicineItemKey(item);
                if (allkeys.Contains(itemKey))
                {
                    var cachedModel = await LocalMachineCache.GetObject<MedicineItemCacheModel>(itemKey);
                    if (cachedModel.content != null && cachedModel.content.Any())
                    {
                        item.ForEach(i =>
                        {
                            var result = cachedModel.content.Where(r => 
                            {
                                var rDate = new DateTime(r.IlacTarihi.date.Year, r.IlacTarihi.date.Month, r.IlacTarihi.date.Day, r.IlacTarihi.date.Hour, r.IlacTarihi.date.Minute,0);
                                var iDate = new DateTime(i.IlacTarihi.date.Year, i.IlacTarihi.date.Month, i.IlacTarihi.date.Day, i.IlacTarihi.date.Hour, i.IlacTarihi.date.Minute, 0);
                                return rDate.Equals(iDate) && r.ilacIsmi.Equals(i.ilacIsmi);
                            });
                            if (result.Any() == false)
                            {
                                cachedModel.date = i.IlacTarihi.date.Date;
                                cachedModel.content.Add(i);
                            }
                        });
                        SaveDataItem(cachedModel.content, key: itemKey);
                    }
                    else
                    {
                        SaveDataItem(item, key: itemKey);
                    }
                }
                else
                {
                    SaveDataItem(item, key: itemKey);
                }
            });
        }

        public async Task ClearAllData()
        {
            await LocalMachineCache.InvalidateAll();
        }
    }
}
