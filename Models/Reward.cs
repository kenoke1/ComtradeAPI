namespace ComtradeAPI.Models
{
    public class Reward
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public int AgentId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public DateTime RewardDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Campaign Campaign { get; set; } = null!;
        public Agent Agent { get; set; } = null!;
        public Purchase? Purchase { get; set; }
    }
}
