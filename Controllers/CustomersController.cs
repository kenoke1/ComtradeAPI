using ComtradeAPI.ModelDTO;
using ComtradeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComtradeAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCustomer(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return BadRequest(new { error = "Customer ID is required" });

            var result = await _customerService.GetCustomerAsync(customerId);
            return result.IsSuccess ? Ok(result.Data) : NotFound(new { error = result.ErrorMessage });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateCustomer([FromBody] CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerService.CreateOrUpdateCustomerAsync(request);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.ErrorMessage });
        }

        [HttpGet("loyal")]
        public async Task<IActionResult> GetLoyalCustomers()
        {
            var result = await _customerService.GetLoyalCustomersAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.ErrorMessage });
        }
    }
}
