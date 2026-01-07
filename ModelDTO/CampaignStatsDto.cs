namespace ComtradeAPI.ModelDTO
{
    public class CampaignStatsDto
    {
        public int TotalRewardsGiven { get; set; }
        public int UniqueCustomersRewarded { get; set; }
        public int TotalPurchases { get; set; }
        public int PurchasesWithDiscount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDiscountApplied { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
