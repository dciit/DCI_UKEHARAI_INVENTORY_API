using System;
using System.Collections.Generic;

namespace DCI_UKEHARAI_INVENTORY_API.Models;

public partial class DictMstr
{
    public int DictId { get; set; }

    public string? DictSystem { get; set; }

    public string? DictType { get; set; }

    public string? Code { get; set; }

    public string? Description { get; set; }

    public string? RefCode { get; set; }

    public string? Ref1 { get; set; }

    public string? Ref2 { get; set; }

    public string? Ref3 { get; set; }

    public string? Ref4 { get; set; }

    public string? Note { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? DictStatus { get; set; }
}
