using ComtradeAPI.ModelDTO;

namespace ComtradeAPI.Services
{
    public interface ICustomerService
    {
        Task<ServiceResult<CustomerDto>> GetCustomerAsync(string customerId);
        Task<ServiceResult<CustomerDto>> CreateOrUpdateCustomerAsync(CreateCustomerRequest request);
        Task<ServiceResult<List<CustomerDto>>> GetLoyalCustomersAsync();
    }
}
