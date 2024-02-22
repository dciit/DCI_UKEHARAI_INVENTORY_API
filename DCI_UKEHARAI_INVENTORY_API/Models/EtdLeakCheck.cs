using System;
using System.Collections.Generic;

namespace DCI_UKEHARAI_INVENTORY_API.Models;

public partial class EtdLeakCheck
{
    public string? SerialNo { get; set; }

    public string? EmpCode { get; set; }

    public string? Brazing { get; set; }

    public string? LineName { get; set; }

    public DateTime? StampTime { get; set; }
}
