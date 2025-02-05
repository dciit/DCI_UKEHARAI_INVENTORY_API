﻿namespace DCI_UKEHARAI_INVENTORY_API.Models
{
    public class MWarning
    {
        public string model { get; set; }
        public string sbu { get; set; }
        public string sebango { get; set; }
        public List<string> customer { get; set; } = new List<string>();
        public List<MPltypeOfCustomer> pltype { get; set; } = new List<MPltypeOfCustomer>();
        public decimal total { get; set; }
        public double inventory { get; set; } = 0;
        public List<MData> listSale { get; set; } = new List<MData>();
        public List<MData> listInventory { get; set; } = new List<MData>();
        public List<MWarningExcel>? listSaleExcel { get; set; } = new List<MWarningExcel>();
        public decimal inbound { get; set; } = 0;
        public string? inboundType { get; set; } = "";
        public double? hold { get; set; } = 0;
        public decimal saleToday {  get; set; } = 0;
        public bool? inventoryUnEqual { get; set; } = false;
    }

    public class MWarningExcel
    {
        public string model { get; set; }
        public string customer { get; set; }
        public string pltype { get; set; }
        public List<MData> data { get; set; } = new List<MData>();
    }

    public class MPltypeOfCustomer
    {
        public string customer { get; set; }
        public List<string> pltype { get; set; } = new List<string>();
    }
}
