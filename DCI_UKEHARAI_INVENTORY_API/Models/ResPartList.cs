using System;
using System.Collections.Generic;

namespace DCI_UKEHARAI_INVENTORY_API.Models;

public partial class ResPartList
{
    public int Pid { get; set; }

    public string Ym { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string Page { get; set; } = null!;

    public string Sno { get; set; } = null!;

    public string Lvl { get; set; } = null!;

    public string Partno { get; set; } = null!;

    public string Cm { get; set; } = null!;

    public string? Route { get; set; }

    public string? Catmat { get; set; }

    public string? Exp { get; set; }

    public string? Cnvcode { get; set; }

    public decimal? Cnvwt { get; set; }

    public string? Vender { get; set; }

    public decimal? Reqqty { get; set; }

    public string? Ivunit { get; set; }

    public string? Whunit { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? CreateBy { get; set; }
}
