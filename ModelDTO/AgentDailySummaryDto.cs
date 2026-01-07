namespace ComtradeAPI.ModelDTO
{
    public class AgentDailySummaryDto
    {
        public int AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string AgentCode { get; set; } = string.Empty;
        public int RewardsGiven { get; set; }
        public int RemainingQuota { get; set; }
    }
}
