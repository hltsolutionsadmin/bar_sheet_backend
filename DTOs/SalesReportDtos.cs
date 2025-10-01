using System;
using System.Collections.Generic;

namespace BarSheetAPI.DTOs
{
  // Request-side DTOs (frontend -> backend)
  public class SizeQtyDto
  {
    public int ProductSizeId { get; set; }
    public int Quantity { get; set; }
  }

  public class ProductSizesDto
  {
    public int ProductId { get; set; }
    public List<SizeQtyDto> Sizes { get; set; } = new();
  }

  // The frontend may send only the arrays it changed (receipts or sales etc.)
  public class SalesReportRequestDto
  {
    public DateTime Date { get; set; }
    public int ShopId { get; set; }

    public List<ProductSizesDto> ObProductsSummary { get; set; } = new();
    public List<ProductSizesDto> ReceiptsProductsSummary { get; set; } = new();
    public List<ProductSizesDto> SalesProductsSummary { get; set; } = new();

    public List<ProductSizesDto> BreaksProductsSummary { get; set; } = new();
    public List<ProductSizesDto> CbProductsSummary { get; set; } = new();
  }

  // Response-side DTOs (backend -> frontend) — enriched with price & amount
  public class SizeAmountDto
  {
    public int ProductSizeId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
  }

  public class ProductSummaryAmountDto
  {
    public int ProductId { get; set; }

    // NEW: category name for grouping on frontend
    public string CategoryName { get; set; } = string.Empty;

    public List<SizeAmountDto> Sizes { get; set; } = new();
    public decimal TotalAmount { get; set; }
  }

  public class SalesReportResponseDto
  {
    public DateTime Date { get; set; }
    public int ShopId { get; set; }

    public string ShopName { get; set; } = string.Empty;

    public List<ProductSummaryAmountDto> ObProductsSummary { get; set; } = new();
    public List<ProductSummaryAmountDto> ReceiptsProductsSummary { get; set; } = new();
    public List<ProductSummaryAmountDto> SalesProductsSummary { get; set; } = new();

    public List<ProductSummaryAmountDto> BreaksProductsSummary { get; set; } = new();
    public List<ProductSummaryAmountDto> CbProductsSummary { get; set; } = new();


    public decimal TotalReceiptsAmount { get; set; }
    public decimal TotalSalesAmount { get; set; }

    public decimal TotalBreaksAmount { get; set; }
    public decimal OverallTotalAmount { get; set; }
  }

  public class SalesReportSummaryDto
  {
    public int Id { get; set; }
    public int ShopId { get; set; }

    public string ShopName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsPublished { get; set; }
    public decimal TotalReceiptsAmount { get; set; }
    public decimal TotalSalesAmount { get; set; }

    public decimal TotalBreaksAmount { get; set; }
    public decimal OverallTotalAmount { get; set; }
  }

  // 🔹 NEW: Paginated response DTO
  public class SalesReportListResponseDto
  {
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<SalesReportSummaryDto> Reports { get; set; } = new();
  }
}
