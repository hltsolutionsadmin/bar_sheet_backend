using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace BarSheetAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; } // Ex: RUM, BEER etc.
        public Category Category { get; internal set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; internal set; }
         public string VariantsJson { get; set; }

    [NotMapped]
    public List<ProductVariant> Variants
    {
      get => string.IsNullOrWhiteSpace(VariantsJson)
          ? new List<ProductVariant>()
          : JsonSerializer.Deserialize<List<ProductVariant>>(VariantsJson) ?? new List<ProductVariant>();

      set => VariantsJson = JsonSerializer.Serialize(value ?? new List<ProductVariant>());
    }
  }

  public class ProductVariant
  {
    public int SizeId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
  }
}
