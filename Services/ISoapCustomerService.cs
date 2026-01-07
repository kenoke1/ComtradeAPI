using ComtradeAPI.ModelDTO;

namespace ComtradeAPI.Services
{
    public interface ISoapCustomerService
    {
        Task<CustomerDto?> FindPersonAsync(string customerId);
        Task<bool> SyncCustomerDataAsync(string customerId);
    }
}
