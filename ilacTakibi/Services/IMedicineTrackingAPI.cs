using System;
using System.Threading.Tasks;
using ilacTakibi.DataModel;
using Refit;

namespace ilacTakibi.Services
{
    public interface IMedicineTrackingAPI
    {
        [Get("/")]
        Task<BaseResponse<MedicineItemModel[]>> GetList();
    }
}
