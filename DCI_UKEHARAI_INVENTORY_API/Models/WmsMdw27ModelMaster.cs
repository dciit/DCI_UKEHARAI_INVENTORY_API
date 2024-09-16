using System;
using System.Collections.Generic;

namespace DCI_UKEHARAI_INVENTORY_API.Models;

public partial class WmsMdw27ModelMaster
{
    public string Model { get; set; } = null!;

    public string Modelgroup { get; set; } = null!;

    public string? Area { get; set; }

    public string Pltype { get; set; } = null!;

    public string Strloc { get; set; } = null!;

    public int Rev { get; set; }

    public int Lrev { get; set; }

    public string Strdate { get; set; } = null!;

    public string Enddate { get; set; } = null!;

    public string Remark { get; set; } = null!;

    public string Sebango { get; set; } = null!;

    public string Diameter { get; set; } = null!;

    public string? Customer { get; set; }

    public string Active { get; set; } = null!;

    public string? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }
}
