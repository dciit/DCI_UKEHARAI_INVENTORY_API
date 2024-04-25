namespace DCI_UKEHARAI_INVENTORY_API.Models
{
    public class MUpdateInventoryMain
    {
        public string ym {  get; set; }
        public string empcode { get; set; } 
        public List<InventoryMain> data { get; set; } = new List<InventoryMain>();
    }

    public class InventoryMain
    {
        public string model { get; set; }
        public int value {  get; set; }
    }
}
