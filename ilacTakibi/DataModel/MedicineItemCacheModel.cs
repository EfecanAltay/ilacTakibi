using System;
using System.Linq;

namespace ilacTakibi.DataModel
{
    public class MedicineItemCacheModel
    {
        public MedicineItemGroupedModel content { get; set; }
        public DateTime date { get; set; }

        public MedicineItemCacheModel(MedicineItemGroupedModel content)
        {
            if (content.Any())
                this.date = content[0].IlacTarihi.date.Date;
            this.content = content;
        }
    }
}
