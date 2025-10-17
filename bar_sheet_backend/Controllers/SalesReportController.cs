using BarSheetAPI.DTOs;
using BarSheetAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace BarSheetAPI.Controllers
{
  // Custom attribute for API key validation (loads key from config dynamically)
  public class ApiKeyAttribute : Attribute, IAuthorizationFilter
  {
    public void OnAuthorization(AuthorizationFilterContext context)
    {
      var config = context.HttpContext.RequestServices.GetService<IConfiguration>();
      if (config == null)
      {
        context.Result = new UnauthorizedResult();
        return;
      }

      var expectedKey = config["BatchPublishKey"] ?? throw new InvalidOperationException("BatchPublishKey not configured.");
      if (!context.HttpContext.Request.Headers.TryGetValue("X-Batch-Key", out var extractedApiKey))
      {
        context.Result = new UnauthorizedResult();
        return;
      }

      if (!extractedApiKey.Equals(expectedKey))
      {
        context.Result = new UnauthorizedResult();
        return;
      }
    }
  }

  [Route("api/[controller]")]
  [ApiController]
  public class SalesReportController : ControllerBase
  {
    private readonly ISalesReportService _salesReportService;
    private readonly IConfiguration _configuration;

    public SalesReportController(ISalesReportService salesReportService, IConfiguration configuration)
    {
      _salesReportService = salesReportService;
      _configuration = configuration;
    }

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

    [HttpPost("save")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SaveDraft([FromBody] SalesReportRequestDto request)
    {
      if (request == null)
        return BadRequest("Request body cannot be null.");

      try
      {
        request.Date = request.Date.Date;

        var result = await _salesReportService.SaveDraftAsync(request);
        return Ok(result);
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(ex.Message); 
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred: {ex.Message}");
      }
    }

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
        return BadRequest(ex.Message);
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

    [HttpPost("batch-publish/{date}")]
    [ApiKey] 
    public async Task<IActionResult> BatchPublish(string date)
    {
      if (!DateTime.TryParse(date, out var parsedDate))
      {
        return BadRequest("Invalid date format. Please use yyyy-MM-dd.");
      }

      try
      {
        var result = await _salesReportService.BatchPublishReportsAsync(parsedDate.Date);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Batch publish failed: {ex.Message}");
      }
    }

    [HttpGet("product-sales/{shopId}/{date}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetProductSales(
            int shopId,
            string date,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
    {
      if (!DateTime.TryParse(date, out var parsedDate))
      {
        return BadRequest("Invalid date format. Please use yyyy-MM-dd.");
      }

      try
      {
        var result = await _salesReportService.GetProductSalesAsync(shopId, parsedDate.Date, pageNumber, pageSize);
        return Ok(result);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
      catch (InvalidOperationException ex)
      {
        return StatusCode(500, $"An error occurred while processing the report: {ex.Message}");
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred: {ex.Message}");
      }
    }
  }
}
