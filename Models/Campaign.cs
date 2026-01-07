namespace ComtradeAPI.Models
{
    public class Campaign
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public int CustomerId { get; set; }
        public DateTime RewardDate { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public Agent Agent { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
    }
}
