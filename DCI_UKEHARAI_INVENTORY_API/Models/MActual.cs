namespace DCI_UKEHARAI_INVENTORY_API.Models
{
    public class MActual
    {
        public string? ym { get; set; }
        public string? model { get; set; }
        public string? modelCode { get; set; }
        public string? line { get; set; }
        public string? wcno { get; set; }
        public string? sebango { get; set; }
        public List<AlGsdActpln> listActPln { get; set; }
        public AlGsdCurpln listCurpln { get; set; }
        public List<AlSaleForecaseMonth> listSaleForecast { get; set; }
    }
}
