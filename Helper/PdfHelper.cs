using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BarSheetAPI.DTOs;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;

namespace BarSheetAPI.Helper
{
  public static partial class PdfHelper
  {
    /// <summary>
    /// Generate a PDF containing one full table per SalesReportResponseDto (usually one per date).
    /// </summary>
    public static byte[] GenerateReportsPdf(
        List<SalesReportResponseDto> reports,
        IDictionary<int, string>? productNames = null,
        string? shopName = null,
        IDictionary<int, string>? sizeNames = null)
    {
      if (reports == null || !reports.Any())
        throw new ArgumentException("No reports to generate.");

      // Ensure only distinct valid dates (avoid duplicates if caller accidentally padded range)
      reports = reports
          .Where(r => r.Date != default)
          .GroupBy(r => r.Date.Date)
          .Select(g => g.First())
          .OrderBy(r => r.Date)
          .ToList();

      using (var stream = new MemoryStream())
      {
        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf);
        document.SetMargins(18, 18, 18, 18);

        // Header (Shop name centered)
        var header = new Paragraph(!string.IsNullOrEmpty(shopName) ? shopName : $"Shop {reports.First().ShopId}")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(16)
            .SetBold();
        document.Add(header);
        document.Add(new Paragraph(" ")); // spacer

        // Global ordered size ids (consistent columns for every date/table)
        var sizeIdOrder = reports
            .SelectMany(r => (r.ObProductsSummary ?? new List<ProductSummaryAmountDto>())
                .Concat(r.ReceiptsProductsSummary ?? new List<ProductSummaryAmountDto>())
                .Concat(r.SalesProductsSummary ?? new List<ProductSummaryAmountDto>())
                .Concat(r.BreaksProductsSummary ?? new List<ProductSummaryAmountDto>())
                .Concat(r.CbProductsSummary ?? new List<ProductSummaryAmountDto>()))
            .SelectMany(p => p.Sizes.Select(s => s.ProductSizeId))
            .Distinct()
            .ToList();

        // Fallback size name generator
        Func<int, string> getSizeLabel = id =>
            sizeNames != null && sizeNames.TryGetValue(id, out var n) ? n : $"Size-{id}";

        var sizeLabels = sizeIdOrder.Select(getSizeLabel).ToList();
        bool hasSizes = sizeIdOrder.Any();

        foreach (var report in reports)
        {
          // Date heading
          document.Add(new Paragraph($"Report Date: {report.Date:yyyy-MM-dd}")
              .SetTextAlignment(TextAlignment.RIGHT)
              .SetFontSize(10));

          // Build aggregated product rows for this report
          var aggregated = new Dictionary<int, AggregatedRow>();

          void EnsureAgg(int productId, string categoryName, string productName)
          {
            if (!aggregated.TryGetValue(productId, out var agg))
            {
              agg = new AggregatedRow
              {
                ProductId = productId,
                CategoryName = categoryName ?? string.Empty,
                ProductName = productName ?? productId.ToString(),
                GroupSizes = new Dictionary<string, Dictionary<int, decimal>>
                {
                  ["OB"] = new Dictionary<int, decimal>(),
                  ["Receipts"] = new Dictionary<int, decimal>(),
                  ["Sales"] = new Dictionary<int, decimal>(),
                  ["Breaks"] = new Dictionary<int, decimal>(),
                  ["CB"] = new Dictionary<int, decimal>()
                },
                GroupTotals = new Dictionary<string, decimal>
                {
                  ["OB"] = 0m,
                  ["Receipts"] = 0m,
                  ["Sales"] = 0m,
                  ["Breaks"] = 0m,
                  ["CB"] = 0m
                }
              };
              aggregated[productId] = agg;
            }
          }

          void PopulateGroup(List<ProductSummaryAmountDto> list, string groupKey)
          {
            if (list == null) return;
            foreach (var p in list)
            {
              var pname = productNames != null && productNames.TryGetValue(p.ProductId, out var pn) ? pn : p.ProductId.ToString();
              EnsureAgg(p.ProductId, p.CategoryName ?? string.Empty, pname);

              var agg = aggregated[p.ProductId];

              foreach (var s in p.Sizes ?? new List<SizeAmountDto>())
              {
                agg.GroupSizes[groupKey][s.ProductSizeId] = s.Quantity;
              }

              if (p.TotalAmount > 0) agg.GroupTotals[groupKey] = p.TotalAmount;
            }
          }

          PopulateGroup(report.ObProductsSummary, "OB");
          PopulateGroup(report.ReceiptsProductsSummary, "Receipts");
          PopulateGroup(report.SalesProductsSummary, "Sales");
          PopulateGroup(report.BreaksProductsSummary, "Breaks");
          PopulateGroup(report.CbProductsSummary, "CB");

          // Prepare table
          int sizeCount = sizeIdOrder.Count;
          int colCount = hasSizes ? (2 + 5 * sizeCount + 1) : 4; // Updated to include Breaks
          var table = new Table(colCount, false).UseAllAvailableWidth();

          if (hasSizes)
          {
            // Header rows
            table.AddHeaderCell(new Cell(2, 1).Add(new Paragraph("No")).SetBold().SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell(2, 1).Add(new Paragraph("Brand Name")).SetBold().SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell(1, sizeCount).Add(new Paragraph("O.B.")).SetBold().SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell(1, sizeCount).Add(new Paragraph("Receipts")).SetBold().SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell(1, sizeCount).Add(new Paragraph("Sale")).SetBold().SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell(1, sizeCount).Add(new Paragraph("Breaks")).SetBold().SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell(1, sizeCount).Add(new Paragraph("C.B.")).SetBold().SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell(2, 1).Add(new Paragraph("Amount")).SetBold().SetTextAlignment(TextAlignment.CENTER));

            for (int g = 0; g < 5; g++) // Updated to include Breaks
              foreach (var lbl in sizeLabels)
                table.AddHeaderCell(new Cell().Add(new Paragraph(lbl)).SetTextAlignment(TextAlignment.CENTER));
          }
          else
          {
            table.AddHeaderCell(new Cell().Add(new Paragraph("No")).SetBold().SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Brand Name")).SetBold().SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Amount")).SetBold().SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Breaks Amount")).SetBold().SetTextAlignment(TextAlignment.CENTER));
          }

          var byCategory = aggregated.Values
              .OrderBy(a => a.CategoryName)
              .ThenBy(a => a.ProductName)
              .GroupBy(a => a.CategoryName);

          int rowNo = 1;
          foreach (var catGroup in byCategory)
          {
            var catCell = new Cell(1, colCount).Add(new Paragraph(catGroup.Key ?? string.Empty))
                .SetBold()
                .SetBackgroundColor(new iText.Kernel.Colors.DeviceRgb(240, 244, 248))
                .SetBorder(new SolidBorder(1));
            table.AddCell(catCell);

            foreach (var prod in catGroup)
            {
              if (hasSizes)
              {
                table.AddCell(new Cell().Add(new Paragraph(rowNo.ToString())));
                table.AddCell(new Cell().Add(new Paragraph(prod.ProductName ?? string.Empty)));

                var groups = new[] { "OB", "Receipts", "Sales", "Breaks", "CB" };
                foreach (var g in groups)
                {
                  foreach (var sizeId in sizeIdOrder)
                  {
                    prod.GroupSizes.TryGetValue(g, out var dict);
                    decimal qty = 0;
                    if (dict != null && dict.TryGetValue(sizeId, out var q)) qty = q;
                    table.AddCell(new Cell().Add(new Paragraph(qty == 0 ? string.Empty : qty.ToString("0.##"))));
                  }
                }

                decimal amount = prod.GroupTotals.ContainsKey("CB") && prod.GroupTotals["CB"] > 0
                    ? prod.GroupTotals["CB"]
                    : prod.GroupTotals.ContainsKey("Receipts") && prod.GroupTotals["Receipts"] > 0
                        ? prod.GroupTotals["Receipts"]
                        : prod.GroupTotals.ContainsKey("OB") ? prod.GroupTotals["OB"] : 0m;

                table.AddCell(new Cell().Add(new Paragraph(amount.ToString("0.00"))).SetTextAlignment(TextAlignment.RIGHT));
              }
              else
              {
                table.AddCell(new Cell().Add(new Paragraph(rowNo.ToString())));
                table.AddCell(new Cell().Add(new Paragraph(prod.ProductName ?? string.Empty)));
                var amount = prod.GroupTotals.ContainsKey("CB") && prod.GroupTotals["CB"] > 0 ? prod.GroupTotals["CB"] : 0m;
                var breaksAmount = prod.GroupTotals.ContainsKey("Breaks") && prod.GroupTotals["Breaks"] > 0 ? prod.GroupTotals["Breaks"] : 0m;
                table.AddCell(new Cell().Add(new Paragraph(amount.ToString("0.00"))).SetTextAlignment(TextAlignment.RIGHT));
                table.AddCell(new Cell().Add(new Paragraph(breaksAmount.ToString("0.00"))).SetTextAlignment(TextAlignment.RIGHT));
              }

              rowNo++;
            }
          }

          document.Add(table);

          // Totals
          var totalsTable = new Table(2, false).SetWidth(UnitValue.CreatePercentValue(30)).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
          totalsTable.AddCell(new Cell().Add(new Paragraph("Total Receipts Amount")).SetBold());
          totalsTable.AddCell(new Cell().Add(new Paragraph(report.TotalReceiptsAmount.ToString("0.00"))));
          totalsTable.AddCell(new Cell().Add(new Paragraph("Total Sales Amount")).SetBold());
          totalsTable.AddCell(new Cell().Add(new Paragraph(report.TotalSalesAmount.ToString("0.00"))));
          totalsTable.AddCell(new Cell().Add(new Paragraph("Total Breaks Amount")).SetBold());
          totalsTable.AddCell(new Cell().Add(new Paragraph(report.TotalBreaksAmount.ToString("0.00"))));
          totalsTable.AddCell(new Cell().Add(new Paragraph("Overall Total Amount")).SetBold());
          totalsTable.AddCell(new Cell().Add(new Paragraph(report.OverallTotalAmount.ToString("0.00"))));
          document.Add(totalsTable);

          if (report != reports.Last())
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        }

        document.Close();
        return stream.ToArray();
      }
    }

    private class AggregatedRow
    {
      public int ProductId { get; set; }
      public string ProductName { get; set; } = string.Empty;
      public string CategoryName { get; set; } = string.Empty;
      public Dictionary<string, Dictionary<int, decimal>> GroupSizes { get; set; } = new();
      public Dictionary<string, decimal> GroupTotals { get; set; } = new();
    }
  }
}
