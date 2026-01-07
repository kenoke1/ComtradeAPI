namespace ComtradeAPI.ModelDTO
{
    public class CampaignResultsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalRewardsIssued { get; set; }
        public int TotalCustomersRewarded { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDiscountsGiven { get; set; }
        public decimal ConversionRate { get; set; }
        public List<CustomerCampaignResult> CustomerResults { get; set; } = new();
    }
}
