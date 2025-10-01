namespace BarSheetAPI.DTOs
{
    public class ShopRegisterDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Identity { get; set; }
        public string ContactNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
