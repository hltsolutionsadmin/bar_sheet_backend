namespace BarSheetAPI.Models
{
    public class ProductSize
    {
        public int ProductSizeId { get; set; }
        public string Name { get; set; }
        public int ShopId { get; set; }
        public bool IsActive { get; set; }
        public Shop Shop { get; internal set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
