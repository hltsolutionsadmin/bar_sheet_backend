using System;
using System.Collections.Generic;

namespace BarSheetAPI.Models
{
  public class DailyReport
  {
    public int Id { get; set; }
    public int ShopId { get; set; }
    public Shop Shop { get; internal set; }

    public DateTime ReportDate { get; set; }


    // 🔹 New JSON columns for structured snapshots
    public string? OBJson { get; set; }          // Opening Balance
    public string? ReceiptsJson { get; set; }    // Receipts
    public string? SalesJson { get; set; }       // Sales
    public string? CBJson { get; set; }

    public string? BreaksJson { get; set; }
    // Publish / lifecycle flags
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }

    // 🔹 Aggregates
    public decimal OverallTotalAmount { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public decimal TotalReceiptsAmount { get; set; }

    public decimal TotalBreaksAmount { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public ICollection<SaleProduct> SaleProducts { get; set; }
  }
}
