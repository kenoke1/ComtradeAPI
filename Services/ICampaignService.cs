using ComtradeAPI.ModelDTO;
using ComtradeAPI.Models;

namespace ComtradeAPI.Services
{
    public interface ICampaignService
    {
        Task<ServiceResult<CampaignRewardDto>> RewardCustomerAsync(CreateRewardRequest request);
        Task<ServiceResult<List<AgentDailySummaryDto>>> GetAgentDailySummaryAsync(DateTime date);
        Task<ServiceResult<CampaignStatsDto>> GetCampaignStatsAsync(DateTime startDate, DateTime endDate);
    }
}
