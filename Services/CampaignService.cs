using ComtradeAPI.Data;
using ComtradeAPI.ModelDTO;

namespace ComtradeAPI.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly CampaignDbContext _context;
        private readonly ILogger<CampaignService> _logger;
        private const int MaxDailyRewardsPerAgent = 5;

        public CampaignService(CampaignDbContext context, ILogger<CampaignService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<ServiceResult<CampaignRewardDto>> RewardCustomerAsync(CreateRewardRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<List<AgentDailySummaryDto>>> GetAgentDailySummaryAsync(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<CampaignStatsDto>> GetCampaignStatsAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }
    }
}
