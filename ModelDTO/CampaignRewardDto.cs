namespace ComtradeAPI.ModelDTO
{
    public class CampaignRewardDto
    {
        public int Id { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string AgentCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public DateTime RewardDate { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
