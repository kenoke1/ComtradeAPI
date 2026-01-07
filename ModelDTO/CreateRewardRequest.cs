namespace ComtradeAPI.ModelDTO
{
    public record CreateRewardRequest(
        int AgentId,
        int CustomerId,
        DateTime RewardDate,
        decimal DiscountPercentage,
        string? Notes
        );
    
}
