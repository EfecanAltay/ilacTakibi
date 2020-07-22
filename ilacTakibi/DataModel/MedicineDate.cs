using System;
namespace ilacTakibi.DataModel
{
    public class MedicineDate
    {
        public DateTime date { get; set; }
        public int timezone_type { get; set; }
        public string timezone { get; set; }

        public override string ToString()
        {
            return date.Ticks.ToString("X2");
        }
    }
}
