namespace BarSheetAPI.DTOs
{
    public class AddProductDto
    {
        public string Name { get; set; }
        public int CategoryId { get; set; } // Ex: RUM, BEER etc.

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public int ShopId { get; set; }

       public List<ProductVariantDto> Variants { get; set; }
  }
  public class ProductVariantDto
  {
    public int SizeId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
  }
}
