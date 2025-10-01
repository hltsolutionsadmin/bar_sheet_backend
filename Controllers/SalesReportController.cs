using BarSheetAPI.DTOs;
using BarSheetAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarSheetAPI.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class SalesReportController : ControllerBase
  {
    private readonly ISalesReportService _salesReportService;

    public SalesReportController(ISalesReportService salesReportService)
    {
      _salesReportService = salesReportService;
    }

    /// <summary>
    /// Get current sales report (OB, CB, receipts, sales)
    /// </summary>
    [HttpGet("{shopId}/{date}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReport(int shopId, string date)
    {
      if (!DateTime.TryParse(date, out var parsedDate))
      {
        return BadRequest("Invalid date format. Please use yyyy-MM-dd.");
      }

      try
      {
        var report = await _salesReportService.GetProductsSummaryAsync(shopId, parsedDate.Date);
        return Ok(report);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred: {ex.Message}");
      }
    }

    /// <summary>
    /// Save draft receipts/sales (does not change OB)
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SaveDraft([FromBody] SalesReportRequestDto request)
    {
      if (request == null)
        return BadRequest("Request body cannot be null.");

      try
      {
        // Always take only the Date part
        request.Date = request.Date.Date;

        var result = await _salesReportService.SaveDraftAsync(request);
        return Ok(result);
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(ex.Message); // e.g., "Cannot save draft: Report is already published."
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred: {ex.Message}");
      }
    }

    /// <summary>
    /// Publish final report: update product stock, OB = CB for next day
    /// </summary>
    [HttpPost("publish/{shopId}/{date}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishReport(int shopId, string date)
    {
      if (!DateTime.TryParse(date, out var parsedDate))
      {
        return BadRequest("Invalid date format. Please use yyyy-MM-dd.");
      }

      try
      {
        var result = await _salesReportService.PublishReportAsync(shopId, parsedDate.Date);
        return Ok(result);
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(ex.Message); // e.g., "Cannot publish: No report exists..."
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred: {ex.Message}");
      }
    }

    [HttpGet("all/{shopId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllReports(
     int shopId,
     [FromQuery] string? date,
     [FromQuery] int pageNumber = 1,
     [FromQuery] int pageSize = 10)
    {
      DateTime? parsedDate = null;
      if (!string.IsNullOrEmpty(date))
      {
        if (DateTime.TryParse(date, out var d))
          parsedDate = d.Date;
        else
          return BadRequest("Invalid date format. Please use yyyy-MM-dd.");
      }

      try
      {
        var result = await _salesReportService.GetAllReportsAsync(shopId, parsedDate, pageNumber, pageSize);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred: {ex.Message}");
      }
    }

    [HttpGet("pdf-range/{shopId}/{from}/{to}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DownloadReportPdfRange(int shopId, string from, string to)
    {
      if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
        return BadRequest("Invalid date format. Use yyyy-MM-dd.");

      try
      {
        var pdfBytes = await _salesReportService.GenerateReportsPdfAsync(shopId, fromDate.Date, toDate.Date);
        var fileName = $"SalesReport_{shopId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
      }
      catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
      catch (Exception ex) { return StatusCode(500, $"An error occurred: {ex.Message}"); }
    }

  }


}
