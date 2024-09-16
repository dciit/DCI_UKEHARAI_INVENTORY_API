using System.Diagnostics;

namespace DCI_UKEHARAI_INVENTORY_API.Models
{
    public class MActual
    {
        public string? modelGroup { get; set; }
        public string? sbu { get; set; }
        public string? ym { get; set; }
        public string? model { get; set; }
        public string? modelCode { get; set; }
        public string? line { get; set; }
        public string? wcno { get; set; }
        public string? sebango { get; set; }
        public string? type { get; set; }
        public string? customer { get; set; }
        public string? pltype { get; set; }
        public string? pltypeText { get; set; }
        public string? menuAuto { get; set; }
        public string? detail { get; set; }

        public string? begin { get; set; }

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
        public List<Total> listTotal { get; set; } = new List<Total>();
        public bool warning { get; set; } = false; // สำหรับหน้า Warning 1 = Inventory balance ติดลบ 
        public List<MData> listInventoryPlanning { get; set; }
        public double totalInventoryPlanning { get; set; } = 0;

        public List<AlGsdActpln> listActFinal { get; set; } = new List<AlGsdActpln>();
        public List<AlGsdCurpln> listCurpln { get; set; } = new List<AlGsdCurpln>();
        public List<AlSaleForecaseMonth> listSaleForecast { get; set; } = new List<AlSaleForecaseMonth>();
        public List<MInbound> listInbound { get; set; } = new List<MInbound>();
        public List<MInventory> Inventory { get; set; } = new List<MInventory>();
        public List<MMainResult> listActMain { get; set; } = new List<MMainResult>();
        public EkbWipPartStock LastInventoryMain { get; set; } = new EkbWipPartStock();
        public int totalInventoryPlanningMain { get; set; } = 0;
        public List<MHoldInventory> listHoldInventory { get; set; } = new List<MHoldInventory>();
        public List<MHoldInventory> listPDTInventory { get; set; } = new List<MHoldInventory>();
        public double LastInventory { get; set; } = 0;
        public List<InventoryBalance> InventoryBalance { get; set; } = new List<InventoryBalance>();
        public List<InventoryBalancePltype> InventoryBalancePltype { get; set; } = new List<InventoryBalancePltype>();

        public List<MData> listSaleForeCaseAllCustomer { get; set; }
        public List<MDelivery> listDelivery { get; set; } = new List<MDelivery>();
        public List<MData> listInventoryPlanningMain { get; set; } = new List<MData>();
        public List<MInventory> listInventoryPlanningMainOrFinal { get; set; } = new List<MInventory>();
        public decimal? inventoryPlanningMainOrFinalEnd { get; set; } = 0;
    }

    public class MDelivery
    {
        public string pltype { get; set; } = "";
        public string customer { get; set; } = "";
        public List<MData> data { get; set; } = new List<MData>();
    }
    public class InventoryBalancePltype
    {
        public string modelName { get; set; }
        public string pltype { get; set; }
        public List<InventoryBalancePltypeData> data { get; set; }
    }
    public class MData
    {
        public string date { get; set; }
        public double value { get; set; } = 0;
        public string? customer { get; set; }
        public string? pltype { get; set; }
    }
    public class InventoryBalance
    {
        public double value { get; set; }
        public string date { get; set; }
    }

    public class InventoryBalancePltypeData
    {
        public string date { set; get; }
        public double value { get; set; }
    }
    public class Total
    {
        public string key { get; set; }
        public double total { get; set; } = 0;
    }
}
