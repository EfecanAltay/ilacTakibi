using ilacTakibi.DataModel;
using ilacTakibi.Services;
using ilacTakibi.ViewModels;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ilacTakibi.Pages
{
    public partial class LiveMedicineListPage : ContentPage
    {
        public LiveMedicineListPage()
        {
            InitializeComponent();
            App.CurrentNavigation = Navigation;
            var bindingContext = BindingContext as MedicineListPageViewModel;
            bindingContext.CurrentMedicineAction = (itemModel) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MedicineItemModel medicineItemModel = null;
                    MedicineItemGroupedModel medicineItemGroupModel = null;
                    bindingContext.LiveMedicineList?.ForEach(i =>
                    {
                        var ds = i.Where(j => j.IlacTarihi.date.Equals(itemModel.IlacTarihi.date));
                        if (ds.Any())
                        {
                            medicineItemModel = ds.Last();
                            medicineItemGroupModel = i;
                            return;
                        }
                    });
                    if (medicineItemModel != null && medicineItemGroupModel != null)
                        medicineList.ScrollTo(medicineItemModel, medicineItemGroupModel, ScrollToPosition.Start, true);
                });
            };
        }
    }
}
