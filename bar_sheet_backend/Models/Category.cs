namespace BarSheetAPI.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; internal set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
