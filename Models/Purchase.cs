namespace ComtradeAPI.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? CampaignId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal DiscountApplied { get; set; }
        public DateTime PurchaseDate { get; set; }
        public bool IsSuccessful { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public Campaign? CampaignReward { get; set; }
    }
}
