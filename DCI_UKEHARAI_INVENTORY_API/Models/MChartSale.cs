namespace DCI_UKEHARAI_INVENTORY_API.Models
{
    public class MChart
    {
        public string name { get; set; }
        public MChartSale chart { get; set; }
    }

    public class MChartSale
    {
        public List<string> label { get; set; } = new List<string>();
        public List<MChartDataSet> dataset { get; set; } = new List<MChartDataSet>();
    }
    public class MChartDataSet
    {
        public List<int?> data { get; set; } = new List<int?>();
        public string backgroundColor { get; set; } = "";
        public bool borderSkipped { get; set; } = false;
        public int borderWidth { get; set; } = 0;
        //public string borderColor { get; set; }
        public string label { get; set; }
    }
    public class MStyleChartOfCustomer
    {
        public string customer { get; set; }
        public string backgroundColor { get; set; }
        //public string borderColor { get; set; } = "";
    }
}
