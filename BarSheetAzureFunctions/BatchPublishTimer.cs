using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace BarsheetAutoPublishFunction
{
  public class BatchPublishTimer
  {
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BatchPublishTimer> _logger;

    public BatchPublishTimer(HttpClient httpClient, IConfiguration configuration, ILogger<BatchPublishTimer> logger)
    {
      _httpClient = httpClient;
      _configuration = configuration;
      _logger = logger;
    }

    /// <summary>
    /// Timer-triggered function to batch publish daily reports for all shops.
    /// Runs automatically at 11:40 PM IST (18:10 UTC) every day.
    /// </summary>
    [Function("BatchPublishTimer")]
    public async Task Run([TimerTrigger("0 10 18 * * *")] TimerInfo myTimer)  // CRON: sec min hour day month day-of-week (UTC)
    {
      _logger.LogInformation($"Batch Publish Timer triggered at: {DateTime.UtcNow}, Local: {DateTime.Now}");


      try
      {
        // Get config
        var apiBaseUrl = _configuration["ApiBaseUrl"];
        var apiKey = _configuration["BatchApiKey"];
        var offsetDaysStr = _configuration["TargetDateOffsetDays"];
        if (string.IsNullOrEmpty(apiBaseUrl) || string.IsNullOrEmpty(apiKey))
        {
          _logger.LogError("❌ Missing ApiBaseUrl or BatchApiKey in configuration.");
          return;
        }

        if (!int.TryParse(offsetDaysStr, out var offsetDays))
          offsetDays = 0;

        var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);
        var targetDate = DateTime.UtcNow.AddDays(offsetDays).Date;  // Use UTC for consistency; adjust to local if needed
        var dateStr = targetDate.ToString("yyyy-MM-dd");

        _logger.LogInformation($"Publishing reports for date: {dateStr} (offset: {offsetDays} days)");

        // Build request
        var url = $"{apiBaseUrl}/api/SalesReport/batch-publish/{dateStr}";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("X-Batch-Key", apiKey);
        // No body needed for our endpoint

        // Send request
        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
          // Parse and log response
          var batchResponse = JsonSerializer.Deserialize<BatchPublishResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
          _logger.LogInformation($"Batch publish succeeded: {batchResponse.Summary}");
          _logger.LogInformation($"Details: {batchResponse.SuccessfulShops} successful shops: [{string.Join(", ", batchResponse.SuccessfulShopIds)}]");
          if (batchResponse.FailedShops > 0)
          {
            _logger.LogWarning($"Failed shops: {batchResponse.FailedShops}. Details: {string.Join("; ", batchResponse.FailedShopsDetails.Select(f => $"{f.ShopId}: {f.ErrorMessage}"))}");
          }
        }
        else
        {
          _logger.LogError($"Batch publish failed. Status: {response.StatusCode}. Response: {responseContent}");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Exception in Batch Publish Timer: {Message}", ex.Message);
        throw;  // Re-throw for Azure retry (default: 3 retries on failure)
      }
    }
  }

  // Simple DTO to deserialize the API response (matches your BatchPublishResponseDto)
  public class BatchPublishResponseDto
  {
    public int TotalShopsProcessed { get; set; }
    public int SuccessfulShops { get; set; }
    public System.Collections.Generic.List<int> SuccessfulShopIds { get; set; } = new();
    public int FailedShops { get; set; }
    public System.Collections.Generic.List<BatchPublishErrorDto> FailedShopsDetails { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
  }

  public class BatchPublishErrorDto
  {
    public int ShopId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
  }
}
