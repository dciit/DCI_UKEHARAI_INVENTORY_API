using System;
using System.Collections.Generic;

namespace DCI_UKEHARAI_INVENTORY_API.Models;

public partial class AlCustomer
{
    public string CustomerCode { get; set; } = null!;

    public string? CustomerName { get; set; }

    public string? CustomerNameShort { get; set; }

    public string? ShipCode { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? Address3 { get; set; }

    public string? Country { get; set; }
}
