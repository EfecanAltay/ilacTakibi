using Xamarin.Forms;
using ilacTakibi.Services;
using ilacTakibi.ViewModels;

namespace ilacTakibi
{
    public partial class App : Application
    {
        public static INavigation CurrentNavigation = null;
        public static CacheService cacheService = null;
        public static MedicineListPageViewModel referenceViewModel = null;
        public static bool isFocusToApp = true;
        public App()
        {
            InitializeComponent();
            cacheService = new CacheService();
            cacheService.Init();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            isFocusToApp = false;
        }

        protected override void OnResume()
        {
            isFocusToApp = true;
        }
    }
}
