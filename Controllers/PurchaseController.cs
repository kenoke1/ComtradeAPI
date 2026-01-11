using ComtradeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComtradeAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILogger<PurchaseController> _logger;

        public PurchaseController(IPurchaseService purchaseService, ILogger<PurchaseController> logger)
        {
            _purchaseService = purchaseService;
            _logger = logger;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportPurchases(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Only CSV files are accepted" });

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _purchaseService.ImportPurchasesFromCsvAsync(stream);
                return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.ErrorMessage, details = result.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading CSV file");
                return StatusCode(500, new { error = "Error processing file upload" });
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetPurchasesByCustomer(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return BadRequest(new { error = "Customer ID is required" });

            var result = await _purchaseService.GetPurchasesByCustomerAsync(customerId);
            return result.IsSuccess ? Ok(result.Data) : NotFound(new { error = result.ErrorMessage });
        }

        [HttpGet("campaign-results")]
        public async Task<IActionResult> GetCampaignResults([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var result = await _purchaseService.GetCampaignResultsAsync(startDate, endDate);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.ErrorMessage});
        }
    }
}
