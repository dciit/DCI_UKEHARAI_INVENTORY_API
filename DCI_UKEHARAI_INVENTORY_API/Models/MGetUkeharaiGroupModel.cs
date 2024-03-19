namespace DCI_UKEHARAI_INVENTORY_API.Models
{
    public class MGetUkeharaiGroupModel
    {
        public string groupModel { get; set; }
        public List<MLineItem> line { get; set; } = new List<MLineItem>();
    }

    public class MLineItem
    {
        public string line { get; set; }
        public List<MGroupModelItem> sale { get; set; } = new List<MGroupModelItem>();
        public List<MGroupModelItem> plan { get; set; } = new List<MGroupModelItem>();
        public List<MGroupModelItem> inventory { get; set; } = new List<MGroupModelItem>();
    }

    public class MGroupModelItem
    {
        public string date { get; set; }
        public string qty { get; set; }
    }

}
