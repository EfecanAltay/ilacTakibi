using System;
using System.Collections.Generic;
using ilacTakibi.DataModel;
using ilacTakibi.Pages;
using ilacTakibi.Services;
using Xamarin.Forms;

namespace ilacTakibi
{
    public partial class MainPage : MasterDetailPage
    {
        public MainPage()
        {
            InitializeComponent();
            // Define a selected handler for the ListView.

            masterPage.ListItemTapped = (tappedItem) =>
            {
                Type type = null;
                switch (tappedItem.Id)
                {
                    case 0:
                        type = typeof(LiveMedicineListPage);
                        break;
                    case 1:
                        type = typeof(UsedMedicineListPage);
                        break;
                    case 2:
                        //...
                        break;
                    default:
                        break;
                }

                if (type != null)
                    this.Detail = new NavigationPage((Page)Activator.CreateInstance(type));
                this.IsPresented = false;
            };
        }
    }
}
