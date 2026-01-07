namespace ComtradeAPI.ModelDTO
{
    public class PurchaseCsvRecord
    {
        public string CustomerId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal? DiscountApplied { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public bool? IsSuccessful { get; set; }
    }
}
