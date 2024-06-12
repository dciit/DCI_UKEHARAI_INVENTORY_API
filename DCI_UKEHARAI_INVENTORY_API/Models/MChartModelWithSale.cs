namespace DCI_UKEHARAI_INVENTORY_API.Models
{
    public class MChartModelWithSale
    {
        public string modelName { get; set; }
        public string sku { get; set; }
        public string? customer {  get; set; }
        public string modelGroup { get; set; }  
        public int? sum { get; set; } = 0;
    }
    public class MChartModelWithPlan
    {
        //public string modelName { get; set; }
        public string sku { get; set; }
        public string modelGroup { get; set; }
        public int sum { get; set; } = 0;
        public string? modelName { get; set; }
    }

    public class MChartModelWithStock
    {
        //public string modelName { get; set; }
        public string sku { get; set; }
        public string modelGroup { get; set; }
        public int sum { get; set; } = 0;
        public string? modelName { get; set; }
    }


}
