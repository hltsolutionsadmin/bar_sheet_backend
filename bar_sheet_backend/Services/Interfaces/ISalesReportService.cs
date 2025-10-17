using BarSheetAPI.DTOs;
using System;
using System.Threading.Tasks;

namespace BarSheetAPI.Services.Interfaces
{
  public interface ISalesReportService
  {
    Task<SalesReportResponseDto> GetProductsSummaryAsync(int shopId, DateTime date);

    Task<SalesReportResponseDto> SaveDraftAsync(SalesReportRequestDto request);
    Task<SalesReportResponseDto> PublishReportAsync(int shopId, DateTime date);

    Task<object> GetAllReportsAsync(int shopId, DateTime? date, int pageNumber, int pageSize);

    Task<byte[]> GenerateReportsPdfAsync(int shopId, DateTime fromDate, DateTime toDate);

    Task<BatchPublishResponseDto> BatchPublishReportsAsync(DateTime date);

    Task<ProductVariantSalesListResponseDto> GetProductSalesAsync(int shopId, DateTime date, int pageNumber, int pageSize);

  }
}
