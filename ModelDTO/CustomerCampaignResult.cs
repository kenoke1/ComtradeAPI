namespace ComtradeAPI.ModelDTO
{
    public class CustomerCampaignResult
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int RewardsReceived { get; set; }
        public int PurchasesMade { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalSaved { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
    }
}
