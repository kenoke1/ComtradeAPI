using ComtradeAPI.ModelDTO;

namespace ComtradeAPI.Services
{
    public interface IPurchaseService
    {
        Task<ServiceResult<PurchaseImportResultDto>> ImportPurchasesFromCsvAsync(Stream csvStream);
        Task<ServiceResult<List<PurchaseDto>>> GetPurchasesByCustomerAsync(string customerId);
        Task<ServiceResult<CampaignResultsDto>> GetCampaignResultsAsync(DateTime? startDate, DateTime? endDate);
    }
}
