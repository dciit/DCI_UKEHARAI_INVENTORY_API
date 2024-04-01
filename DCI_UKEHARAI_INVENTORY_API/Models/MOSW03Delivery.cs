namespace DCI_UKEHARAI_INVENTORY_API.Models
{
    public class MOSW03Delivery
    {
        public string model {  get; set; }
        public string cfdate { get; set; }
        public string ifdate { get; set; }
        public string pltype { get; set; }
        public int qty { get; set; }    
        public int alqty { get; set; }
        public int picqty { get; set; }
    }
}
