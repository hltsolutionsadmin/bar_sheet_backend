namespace BarSheetAPI.DTOs
{
    public class AddCategoryDTO
    {
        public string Name { get; set; }
        public int ShopId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

  public class UpdateCategoryDTO
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public int ShopId { get; set; }
  }
}
