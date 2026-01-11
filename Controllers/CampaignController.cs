using ComtradeAPI.ModelDTO;
using ComtradeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ComtradeAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;

        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }


        [HttpPost("rewards")]
        public async Task<IActionResult> CreateReward([FromBody] CreateRewardRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _campaignService.RewardCustomerAsync(request);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.ErrorMessage });
        }

        [HttpGet("daily-summary")]
        public async Task<IActionResult> GetDailySummary([FromQuery] DateTime? date)
        {
            var targetDate = date ?? DateTime.UtcNow;
            var result = await _campaignService.GetAgentDailySummaryAsync(targetDate);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.ErrorMessage });

        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest(new { error = "Start date must be before end date" });

            var result = await _campaignService.GetCampaignStatsAsync(startDate, endDate);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.ErrorMessage });
        }
    }
}
