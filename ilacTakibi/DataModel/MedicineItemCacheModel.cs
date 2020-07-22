using System;

namespace ilacTakibi.DataModel
{
    public class MedicineItemCacheModel
    {
        public MedicineItemGroupedModel content { get; set; }
        public DateTime date { get; set; }

        public MedicineItemCacheModel(MedicineItemGroupedModel content)
        {
            this.date = content.Date;
            this.content = content;
        }
    }
}
