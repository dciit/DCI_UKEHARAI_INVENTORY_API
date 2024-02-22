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
        public string? type { get; set; }   
        public string? customer { get; set; }   
        public string? pltype {  get; set; }
        public string? pltypeText { get; set; }
        public string? menuAuto { get; set; }
        public string? detail { get; set; } 

        public string? begin {  get; set; }

        public double d01 { get; set; }
        public double d02 { get; set; }
        public double d03 { get; set; }
        public double d04 { get; set; }
        public double d05 { get; set; }
        public double d06 { get; set; }
        public double d07 { get; set; }
        public double d08 { get; set; }
        public double d09 { get; set; }
        public double d10 { get; set; }
        public double d11 { get; set; }
        public double d12 { get; set; }
        public double d13 { get; set; }
        public double d14 { get; set; }
        public double d15 { get; set; }
        public double d16 { get; set; }
        public double d17 { get; set; }
        public double d18 { get; set; }
        public double d19 { get; set; }
        public double d20 { get; set; }
        public double d21 { get; set; }
        public double d22 { get; set; }
        public double d23 { get; set; }
        public double d24 { get; set; }
        public double d25 { get; set; }
        public double d26 { get; set; }
        public double d27 { get; set; }
        public double d28 { get; set; }
        public double d29 { get; set; }
        public double d30 { get; set; }
        public double d31 { get; set; }



        public List<AlGsdActpln> listActPln { get; set; }
        public AlGsdCurpln listCurpln { get; set; }
        public List<AlSaleForecaseMonth> listSaleForecast { get; set; }
        public List<MInbound> listInbound { get; set; }
        public List<string> listPltype { get; set; }
        public List<MInventory> listInventory { get; set; }
        public List<MMainResult> listActMain { get; set; }
    }
}
