namespace DCI_UKEHARAI_INVENTORY_API.Models
{
    public class MWarning
    {
        public string model { get; set; }
        public string sbu { get; set; }
        public string sebango { get; set; }
        public List<string> customer { get; set; } = new List<string>();
        public List<string> pltype { get; set; } = new List<string>();
        public double total { get; set; }
        public List<MData> listSale { get; set; } = new List<MData>();
        public List<MData> listInventory { get; set; } = new List<MData>();
    }
}
