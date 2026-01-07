namespace ComtradeAPI.ModelDTO
{
    public class PurchaseDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal DiscountApplied { get; set; }
        public DateTime PurchaseDate { get; set; }
        public bool IsSuccessful { get; set; }
        public bool HasCampaignDiscount { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}
