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
            listData.ForEach(item => {
                item.content.Date = item.date;
                returnedList.Add(item.content);
            });
            return returnedList.OrderByDescending(x => x.Date);
        }

        private string ToMedicineItemKey(MedicineItemGroupedModel model)
        {
            return model.Date.Ticks.ToString("X2");
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

            medicineItems.ForEach(item =>
            {
                if (allkeys.Contains(ToMedicineItemKey(item)) == false)
                    SaveDataItem(item);
            });
        }

        public async Task ClearAllData()
        {
            await LocalMachineCache.InvalidateAll();
        }
    }
}
