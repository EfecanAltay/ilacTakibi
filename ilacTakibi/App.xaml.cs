using Xamarin.Forms;
using ilacTakibi.Services;

namespace ilacTakibi
{
    public partial class App : Application
    {
        public static INavigation CurrentNavigation = null;

        public static CacheService cacheService = null;
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
        }

        protected override void OnResume()
        {
        }
    }
}
