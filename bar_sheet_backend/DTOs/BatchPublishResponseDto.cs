using System.Collections.Generic;

namespace BarSheetAPI.DTOs
{
  public class BatchPublishResponseDto
  {
    public int TotalShopsProcessed { get; set; }
    public int SuccessfulShops { get; set; }
    public List<int> SuccessfulShopIds { get; set; } = new List<int>();
    public int FailedShops { get; set; }
    public List<BatchPublishErrorDto> FailedShopsDetails { get; set; } = new List<BatchPublishErrorDto>();
    public string Summary { get; set; } = string.Empty;
  }

  public class BatchPublishErrorDto
  {
    public int ShopId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
  }
}
