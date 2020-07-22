using System;
namespace ilacTakibi.DataModel
{
    public class BaseResponse<T>
    {
        public T value { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }

        public BaseResponse()
        {

        }
    }
}
