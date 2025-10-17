namespace BarSheetAPI.Models
{
    public class SaleProduct
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; internal set; }
        public int ProductId { get; set; }
        public Product Product { get; internal set; }
        public int DailyReportId { get; set; }
        public DailyReport DailyReport { get; internal set; }
        public int QuantitySold { get; set; }
        public int OB_Q { get; set; }
        public int OB_P { get; set; }
        public int OB_N { get; set; }
        public int CB_Q { get; set; }
        public int CB_P { get; set; }
        public int CB_N { get; set; }
        public int RC_Q { get; set; }
        public int RC_P { get; set; }
        public int RC_N { get; set; }
        public int SP_Q { get; set; }
        public int SP_P { get; set; }
        public int SP_N { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
