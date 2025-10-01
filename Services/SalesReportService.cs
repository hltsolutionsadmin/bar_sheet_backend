using BarSheetAPI.DTOs;
using BarSheetAPI.Helper;
using BarSheetAPI.Models;
using BarSheetAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BarSheetAPI.Services
{
  public class SalesReportService : ISalesReportService
  {
    private readonly InventoryDbContext _context;

    public SalesReportService(InventoryDbContext context)
    {
      _context = context;
    }

    public async Task<SalesReportResponseDto> GetProductsSummaryAsync(int shopId, DateTime date)
    {
      var report = await _context.DailyReports
          .Include(r => r.Shop)
          .FirstOrDefaultAsync(r => r.ShopId == shopId && r.ReportDate == date.Date);

      if (report != null)
      {
        return new SalesReportResponseDto
        {
          Date = report.ReportDate,
          ShopId = report.ShopId,
          ShopName = report.Shop?.Name ?? $"Shop {report.ShopId}",
          ObProductsSummary = string.IsNullOrEmpty(report.OBJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.OBJson),
          ReceiptsProductsSummary = string.IsNullOrEmpty(report.ReceiptsJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.ReceiptsJson),
          SalesProductsSummary = string.IsNullOrEmpty(report.SalesJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.SalesJson),
          BreaksProductsSummary = string.IsNullOrEmpty(report.BreaksJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.BreaksJson),
          CbProductsSummary = string.IsNullOrEmpty(report.CBJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.CBJson),
          TotalReceiptsAmount = report.TotalReceiptsAmount,
          TotalSalesAmount = report.TotalSalesAmount,
          TotalBreaksAmount = report.TotalBreaksAmount,
          OverallTotalAmount = report.OverallTotalAmount
        };
      }

      var products = await _context.Products
          .Include(p => p.Category)
          .Where(p => p.ShopId == shopId)
          .ToListAsync();

      var shop = await _context.Shops.FirstOrDefaultAsync(s => s.Id == shopId);

      var obSummary = new List<ProductSummaryAmountDto>();

      foreach (var product in products)
      {
        var summary = new ProductSummaryAmountDto
        {
          ProductId = product.Id,
          CategoryName = product.Category?.Name ?? string.Empty,
          Sizes = new List<SizeAmountDto>()
        };

        foreach (var variant in product.Variants)
        {
          decimal amount = variant.Quantity * variant.Price;
          summary.Sizes.Add(new SizeAmountDto
          {
            ProductSizeId = variant.SizeId,
            Quantity = variant.Quantity,
            Price = variant.Price,
            Amount = amount
          });
        }

        summary.TotalAmount = summary.Sizes.Sum(s => s.Amount);
        if (summary.Sizes.Any())
        {
          obSummary.Add(summary);
        }
      }

      return new SalesReportResponseDto
      {
        Date = date.Date,
        ShopId = shopId,
        ShopName = shop?.Name ?? $"Shop {shopId}",
        ObProductsSummary = obSummary,
        ReceiptsProductsSummary = new List<ProductSummaryAmountDto>(),
        SalesProductsSummary = new List<ProductSummaryAmountDto>(),
        BreaksProductsSummary = new List<ProductSummaryAmountDto>(),
        CbProductsSummary = obSummary.Select(s => new ProductSummaryAmountDto
        {
          ProductId = s.ProductId,
          CategoryName = s.CategoryName,
          Sizes = s.Sizes.Select(size => new SizeAmountDto
          {
            ProductSizeId = size.ProductSizeId,
            Quantity = size.Quantity,
            Price = size.Price,
            Amount = size.Amount
          }).ToList(),
          TotalAmount = s.TotalAmount
        }).ToList(),
        TotalReceiptsAmount = 0,
        TotalSalesAmount = 0,
        TotalBreaksAmount = 0,
        OverallTotalAmount = 0
      };
    }

    public async Task<SalesReportResponseDto> SaveDraftAsync(SalesReportRequestDto dto)
    {
      var existingReport = await _context.DailyReports
          .FirstOrDefaultAsync(r => r.ShopId == dto.ShopId && r.ReportDate == dto.Date.Date);
      if (existingReport != null && existingReport.IsPublished)
      {
        throw new InvalidOperationException("Cannot save draft: Report is already published.");
      }

      var products = await _context.Products
          .Include(p => p.Category)
          .Where(p => p.ShopId == dto.ShopId)
          .ToListAsync();

      var shop = await _context.Shops.FirstOrDefaultAsync(s => s.Id == dto.ShopId);

      var report = existingReport ?? new DailyReport
      {
        ShopId = dto.ShopId,
        ReportDate = dto.Date.Date,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        CreatedBy = "system"
      };

      var previousOb = new List<ProductSummaryAmountDto>();
      var previousReceipts = new List<ProductSummaryAmountDto>();
      var previousSales = new List<ProductSummaryAmountDto>();
      var previousBreaks = new List<ProductSummaryAmountDto>();

      if (existingReport != null)
      {
        previousOb = string.IsNullOrEmpty(report.OBJson)
            ? new List<ProductSummaryAmountDto>()
            : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.OBJson);
        previousReceipts = string.IsNullOrEmpty(report.ReceiptsJson)
            ? new List<ProductSummaryAmountDto>()
            : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.ReceiptsJson);
        previousSales = string.IsNullOrEmpty(report.SalesJson)
            ? new List<ProductSummaryAmountDto>()
            : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.SalesJson);
        previousBreaks = string.IsNullOrEmpty(report.BreaksJson)
            ? new List<ProductSummaryAmountDto>()
            : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.BreaksJson);
      }
      else
      {
        foreach (var product in products)
        {
          var summary = new ProductSummaryAmountDto
          {
            ProductId = product.Id,
            CategoryName = product.Category?.Name ?? string.Empty,
            Sizes = new List<SizeAmountDto>()
          };
          foreach (var variant in product.Variants)
          {
            summary.Sizes.Add(new SizeAmountDto
            {
              ProductSizeId = variant.SizeId,
              Quantity = variant.Quantity,
              Price = variant.Price,
              Amount = variant.Quantity * variant.Price
            });
          }
          summary.TotalAmount = summary.Sizes.Sum(s => s.Amount);
          if (summary.Sizes.Any())
          {
            previousOb.Add(summary);
          }
        }
      }

      var obSummary = dto.ObProductsSummary?.Any() == true
          ? await BuildProductSummaryAsync(dto.ObProductsSummary, dto.ShopId, products)
          : previousOb;
      var receiptsSummary = dto.ReceiptsProductsSummary?.Any() == true
          ? await BuildProductSummaryAsync(dto.ReceiptsProductsSummary, dto.ShopId, products)
          : previousReceipts;
      var salesSummary = dto.SalesProductsSummary?.Any() == true
          ? await BuildProductSummaryAsync(dto.SalesProductsSummary, dto.ShopId, products)
          : previousSales;
      var breaksSummary = dto.BreaksProductsSummary?.Any() == true
          ? await BuildProductSummaryAsync(dto.BreaksProductsSummary, dto.ShopId, products)
          : previousBreaks;

      // Validate breaks quantities
      foreach (var breakProduct in breaksSummary)
      {
        var product = products.FirstOrDefault(p => p.Id == breakProduct.ProductId);
        if (product == null)
        {
          throw new InvalidOperationException($"Invalid product ID: {breakProduct.ProductId}");
        }
        foreach (var size in breakProduct.Sizes)
        {
          var variant = product.Variants.FirstOrDefault(v => v.SizeId == size.ProductSizeId);
          if (variant == null)
          {
            throw new InvalidOperationException($"Invalid size ID: {size.ProductSizeId} for product {breakProduct.ProductId}");
          }
          if (size.Quantity < 0)
          {
            throw new InvalidOperationException($"Break quantity for product {breakProduct.ProductId}, size {size.ProductSizeId} cannot be negative");
          }
          if (size.Quantity != (int)size.Quantity)
          {
            throw new InvalidOperationException($"Break quantity for product {breakProduct.ProductId}, size {size.ProductSizeId} must be a whole number");
          }
        }
      }

      var allProductIds = obSummary.Select(p => p.ProductId)
          .Union(receiptsSummary.Select(p => p.ProductId))
          .Union(salesSummary.Select(p => p.ProductId))
          .Union(breaksSummary.Select(p => p.ProductId))
          .Distinct()
          .ToList();

      var cbSummary = new List<ProductSummaryAmountDto>();
      foreach (var productId in allProductIds)
      {
        var product = products.FirstOrDefault(p => p.Id == productId);
        if (product == null) continue;

        var cbProduct = new ProductSummaryAmountDto
        {
          ProductId = productId,
          CategoryName = product.Category?.Name ?? string.Empty,
          Sizes = new List<SizeAmountDto>()
        };

        var obProduct = obSummary.FirstOrDefault(p => p.ProductId == productId);
        var receiptsProduct = receiptsSummary.FirstOrDefault(p => p.ProductId == productId);
        var salesProduct = salesSummary.FirstOrDefault(p => p.ProductId == productId);
        var breaksProduct = breaksSummary.FirstOrDefault(p => p.ProductId == productId);

        var allSizes = (obProduct?.Sizes?.Select(s => s.ProductSizeId) ?? new List<int>())
            .Union(receiptsProduct?.Sizes?.Select(s => s.ProductSizeId) ?? new List<int>())
            .Union(salesProduct?.Sizes?.Select(s => s.ProductSizeId) ?? new List<int>())
            .Union(breaksProduct?.Sizes?.Select(s => s.ProductSizeId) ?? new List<int>())
            .Distinct()
            .ToList();

        foreach (var sizeId in allSizes)
        {
          var variant = product.Variants.FirstOrDefault(v => v.SizeId == sizeId);
          if (variant == null) continue;

          var obQty = obProduct?.Sizes.FirstOrDefault(s => s.ProductSizeId == sizeId)?.Quantity ?? 0;
          var receiptsQty = receiptsProduct?.Sizes.FirstOrDefault(s => s.ProductSizeId == sizeId)?.Quantity ?? 0;
          var salesQty = salesProduct?.Sizes.FirstOrDefault(s => s.ProductSizeId == sizeId)?.Quantity ?? 0;
          var breaksQty = breaksProduct?.Sizes.FirstOrDefault(s => s.ProductSizeId == sizeId)?.Quantity ?? 0;

          var cbQty = obQty + receiptsQty - salesQty - breaksQty;
          if (cbQty < 0)
          {
            throw new InvalidOperationException($"Closing balance for product {productId}, size {sizeId} cannot be negative");
          }
          var cbAmount = cbQty * variant.Price;

          cbProduct.Sizes.Add(new SizeAmountDto
          {
            ProductSizeId = sizeId,
            Quantity = cbQty,
            Price = variant.Price,
            Amount = cbAmount
          });
        }

        cbProduct.TotalAmount = cbProduct.Sizes.Sum(s => s.Amount);
        if (cbProduct.Sizes.Any())
        {
          cbSummary.Add(cbProduct);
        }
      }

      var totalReceiptsAmount = receiptsSummary.Sum(p => p.TotalAmount);
      var totalSalesAmount = salesSummary.Sum(p => p.TotalAmount);
      var totalBreaksAmount = breaksSummary.Sum(p => p.TotalAmount);
      var overallTotalAmount = totalReceiptsAmount - totalSalesAmount - totalBreaksAmount;

      report.OBJson = JsonSerializer.Serialize(obSummary);
      report.ReceiptsJson = JsonSerializer.Serialize(receiptsSummary);
      report.SalesJson = JsonSerializer.Serialize(salesSummary);
      report.BreaksJson = JsonSerializer.Serialize(breaksSummary);
      report.CBJson = JsonSerializer.Serialize(cbSummary);
      report.TotalReceiptsAmount = totalReceiptsAmount;
      report.TotalSalesAmount = totalSalesAmount;
      report.TotalBreaksAmount = totalBreaksAmount;
      report.OverallTotalAmount = overallTotalAmount;

      if (existingReport == null)
      {
        _context.DailyReports.Add(report);
      }
      await _context.SaveChangesAsync();

      return new SalesReportResponseDto
      {
        Date = report.ReportDate,
        ShopId = report.ShopId,
        ShopName = shop?.Name ?? $"Shop {dto.ShopId}",
        ObProductsSummary = obSummary,
        ReceiptsProductsSummary = receiptsSummary,
        SalesProductsSummary = salesSummary,
        BreaksProductsSummary = breaksSummary,
        CbProductsSummary = cbSummary,
        TotalReceiptsAmount = totalReceiptsAmount,
        TotalSalesAmount = totalSalesAmount,
        TotalBreaksAmount = totalBreaksAmount,
        OverallTotalAmount = overallTotalAmount
      };
    }

    public async Task<SalesReportResponseDto> PublishReportAsync(int shopId, DateTime date)
    {
      var report = await _context.DailyReports
          .FirstOrDefaultAsync(r => r.ShopId == shopId && r.ReportDate == date.Date);

      if (report == null)
      {
        throw new InvalidOperationException("Cannot publish: No report exists for this date and shop.");
      }
      if (report.IsPublished)
      {
        throw new InvalidOperationException("Cannot publish: Report is already published.");
      }

      // Mark report as published
      report.IsPublished = true;
      report.PublishedAt = DateTime.UtcNow;
      report.UpdatedAt = DateTime.UtcNow;

      // Load CB from report
      var cbSummary = string.IsNullOrEmpty(report.CBJson)
          ? new List<ProductSummaryAmountDto>()
          : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.CBJson);

      // Update product quantities
      var products = await _context.Products
          .Where(p => p.ShopId == shopId)
          .ToListAsync();

      foreach (var cbProduct in cbSummary)
      {
        var product = products.FirstOrDefault(p => p.Id == cbProduct.ProductId);
        if (product == null) continue;

        var variants = product.Variants;
        foreach (var cbSize in cbProduct.Sizes)
        {
          var variant = variants.FirstOrDefault(v => v.SizeId == cbSize.ProductSizeId);
          if (variant != null)
          {
            if (cbSize.Quantity < 0)
            {
              throw new InvalidOperationException($"Cannot publish: Closing balance for product {cbProduct.ProductId}, size {cbSize.ProductSizeId} is negative.");
            }
            variant.Quantity = cbSize.Quantity;
          }
        }
        product.Variants = variants; // Update VariantsJson via setter
      }

      await _context.SaveChangesAsync();

      // Return the final report
      return await GetProductsSummaryAsync(shopId, date);
    }

    private async Task<List<ProductSummaryAmountDto>> BuildProductSummaryAsync(
        List<ProductSizesDto> input,
        int shopId,
        List<Product> products)
    {
      var result = new List<ProductSummaryAmountDto>();

      foreach (var psDto in input)
      {
        var product = products.FirstOrDefault(p => p.Id == psDto.ProductId);
        if (product == null) continue;

        var summary = new ProductSummaryAmountDto
        {
          ProductId = product.Id,
          CategoryName = product.Category?.Name ?? string.Empty,
          Sizes = new List<SizeAmountDto>()
        };

        foreach (var sizeDto in psDto.Sizes)
        {
          var variant = product.Variants.FirstOrDefault(v => v.SizeId == sizeDto.ProductSizeId);
          if (variant == null) continue;

          decimal price = variant.Price;
          decimal amount = sizeDto.Quantity * price;

          summary.Sizes.Add(new SizeAmountDto
          {
            ProductSizeId = sizeDto.ProductSizeId,
            Quantity = sizeDto.Quantity,
            Price = price,
            Amount = amount
          });
        }

        summary.TotalAmount = summary.Sizes.Sum(s => s.Amount);
        if (summary.Sizes.Any())
        {
          result.Add(summary);
        }
      }

      return result;
    }

    public async Task<object> GetAllReportsAsync(int shopId, DateTime? date, int pageNumber, int pageSize)
    {
      if (date.HasValue)
      {
        var reports = await _context.DailyReports
            .Include(r => r.Shop)
            .Where(r => r.ShopId == shopId && r.ReportDate == date.Value.Date)
            .OrderBy(r => r.ReportDate)
            .ToListAsync();

        var fullReports = reports.Select(r => new SalesReportResponseDto
        {
          Date = r.ReportDate,
          ShopId = r.ShopId,
          ShopName = r.Shop?.Name ?? $"Shop {r.ShopId}",
          ObProductsSummary = string.IsNullOrEmpty(r.OBJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(r.OBJson),
          ReceiptsProductsSummary = string.IsNullOrEmpty(r.ReceiptsJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(r.ReceiptsJson),
          SalesProductsSummary = string.IsNullOrEmpty(r.SalesJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(r.SalesJson),
          BreaksProductsSummary = string.IsNullOrEmpty(r.BreaksJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(r.BreaksJson),
          CbProductsSummary = string.IsNullOrEmpty(r.CBJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(r.CBJson),
          TotalReceiptsAmount = r.TotalReceiptsAmount,
          TotalSalesAmount = r.TotalSalesAmount,
          TotalBreaksAmount = r.TotalBreaksAmount,
          OverallTotalAmount = r.OverallTotalAmount
        }).ToList();

        return fullReports;
      }

      var query = _context.DailyReports
          .Include(r => r.Shop)
          .Where(r => r.ShopId == shopId)
          .AsQueryable();

      var totalCount = await query.CountAsync();

      var reportsSummary = await query
          .OrderByDescending(r => r.ReportDate)
          .Skip((pageNumber - 1) * pageSize)
          .Take(pageSize)
          .Select(r => new SalesReportSummaryDto
          {
            Id = r.Id,
            ShopId = r.ShopId,
            //ShopName = r.Shop?.Name ?? $"Shop {r.ShopId}",
            Date = r.ReportDate,
            IsPublished = r.IsPublished,
            TotalReceiptsAmount = r.TotalReceiptsAmount,
            TotalSalesAmount = r.TotalSalesAmount,
            TotalBreaksAmount = r.TotalBreaksAmount,
            OverallTotalAmount = r.OverallTotalAmount
          })
          .ToListAsync();

      return new SalesReportListResponseDto
      {
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize,
        Reports = reportsSummary
      };
    }

    public async Task<byte[]> GenerateReportsPdfAsync(int shopId, DateTime fromDate, DateTime toDate)
    {
      if (fromDate.Date > toDate.Date)
        throw new InvalidOperationException("From date must be less than or equal to To date.");

      // Fetch all existing reports in the date range
      var reports = await _context.DailyReports
          .Include(r => r.Shop)
          .Where(r => r.ShopId == shopId && r.ReportDate >= fromDate.Date && r.ReportDate <= toDate.Date)
          .OrderBy(r => r.ReportDate)
          .ToListAsync();

      // If no reports exist in the range, throw exception
      if (!reports.Any())
      {
        throw new InvalidOperationException(
            $"No reports found for shop {shopId} between {fromDate:yyyy-MM-dd} and {toDate:yyyy-MM-dd}.");
      }

      // Ensure we only use actual reports from DB, not "fill" missing dates
      var minAvailableDate = reports.Min(r => r.ReportDate.Date);
      var maxAvailableDate = reports.Max(r => r.ReportDate.Date);

      // If the requested range is fully outside the available DB range, throw
      if (toDate.Date < minAvailableDate || fromDate.Date > maxAvailableDate)
      {
        throw new InvalidOperationException(
            $"No reports available between {fromDate:yyyy-MM-dd} and {toDate:yyyy-MM-dd} for shop {shopId}.");
      }

      // Convert DailyReports to SalesReportResponseDto
      var reportDtos = reports.Select(report => new SalesReportResponseDto
      {
        Date = report.ReportDate,
        ShopId = report.ShopId,
        ShopName = report.Shop?.Name ?? $"Shop {report.ShopId}",
        ObProductsSummary = string.IsNullOrEmpty(report.OBJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.OBJson),
        ReceiptsProductsSummary = string.IsNullOrEmpty(report.ReceiptsJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.ReceiptsJson),
        SalesProductsSummary = string.IsNullOrEmpty(report.SalesJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.SalesJson),
        BreaksProductsSummary = string.IsNullOrEmpty(report.BreaksJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.BreaksJson),
        CbProductsSummary = string.IsNullOrEmpty(report.CBJson)
              ? new List<ProductSummaryAmountDto>()
              : JsonSerializer.Deserialize<List<ProductSummaryAmountDto>>(report.CBJson),
        TotalReceiptsAmount = report.TotalReceiptsAmount,
        TotalSalesAmount = report.TotalSalesAmount,
        TotalBreaksAmount = report.TotalBreaksAmount,
        OverallTotalAmount = report.OverallTotalAmount
      }).ToList();

      // Fetch additional data for PDF
      var products = await _context.Products
          .Where(p => p.ShopId == shopId)
          .ToListAsync();

      var productNames = products.ToDictionary(p => p.Id, p => p.Name ?? p.Id.ToString());

      var allSizeIds = reportDtos
          .SelectMany(r => (r.ObProductsSummary ?? new List<ProductSummaryAmountDto>())
                          .Concat(r.ReceiptsProductsSummary ?? new List<ProductSummaryAmountDto>())
                          .Concat(r.SalesProductsSummary ?? new List<ProductSummaryAmountDto>())
                          .Concat(r.BreaksProductsSummary ?? new List<ProductSummaryAmountDto>())
                          .Concat(r.CbProductsSummary ?? new List<ProductSummaryAmountDto>()))
          .SelectMany(p => p.Sizes.Select(s => s.ProductSizeId))
          .Distinct()
          .ToList();

      Dictionary<int, string> sizeNames = allSizeIds.ToDictionary(id => id, id => $"Size-{id}");

      try
      {
        var dbSizeNames = await _context.ProductSizes
            .Where(s => allSizeIds.Contains(s.ProductSizeId))
            .ToDictionaryAsync(s => s.ProductSizeId, s => s.Name ?? $"Size-{s.ProductSizeId}");

        foreach (var kv in dbSizeNames)
          sizeNames[kv.Key] = kv.Value;
      }
      catch (Exception)
      {
        // If ProductSizes doesn't exist or query fails, keep fallback names
      }

      string shopName = $"Shop {shopId}";
      try
      {
        var shop = await _context.Shops.FirstOrDefaultAsync(s => s.Id == shopId);
        if (shop != null && !string.IsNullOrEmpty(shop.Name))
          shopName = shop.Name;
      }
      catch (Exception)
      {
        // Keep fallback shop name if Shops set doesn't exist
      }

      // Generate PDF with only the available reports
      return PdfHelper.GenerateReportsPdf(reportDtos, productNames, shopName, sizeNames);
    }
  }
}
