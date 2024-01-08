using System;
using System.Collections.Generic;

namespace DCI_UKEHARAI_INVENTORY_API.Models;

public partial class PnCompressor
{
    public string ModelCode { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string? ModelType { get; set; }

    public string? ModelGroup { get; set; }

    public int Line { get; set; }

    public string? Status { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? Capacity { get; set; }

    /// <summary>
    /// Model customer
    /// </summary>
    public string? Rmk1 { get; set; }

    /// <summary>
    /// Pallate
    /// </summary>
    public string? Rmk2 { get; set; }

    /// <summary>
    /// Magnet
    /// </summary>
    public string? Rmk3 { get; set; }

    public string? Rmk4 { get; set; }

    /// <summary>
    /// bit for check terminal cover
    /// </summary>
    public string? Rmk5 { get; set; }

    public string? Rmk6 { get; set; }

    public string? Rmk7 { get; set; }

    public string? Rmk8 { get; set; }

    public string? Rmk9 { get; set; }

    /// <summary>
    /// data for check original line (backflush)
    /// </summary>
    public string? Rmk10 { get; set; }
}
