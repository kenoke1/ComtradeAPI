using ComtradeAPI.Data;
using ComtradeAPI.ModelDTO;
using ComtradeAPI.Models;
using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ComtradeAPI.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly CampaignDbContext _context;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(CampaignDbContext context, ILogger<PurchaseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<PurchaseImportResultDto>> ImportPurchasesFromCsvAsync(Stream csvStream)
        {
            var result = new PurchaseImportResultDto();

            try
            {
                using var reader = new StreamReader(csvStream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    TrimOptions = TrimOptions.Trim
                });

                var records = csv.GetRecords<PurchaseCsvRecord>().ToList();
                result.TotalRecords = records.Count;

                _logger.LogInformation("Starting import of {Count} purchase records", records.Count);

                foreach (var record in records)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(record.CustomerId) || string.IsNullOrWhiteSpace(record.OrderNumber))
                        {
                            result.FailedRecords++;
                            result.Errors.Add($"Missing required fields for order {record.OrderNumber}");
                            continue;
                        }

                        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == record.CustomerId);

                        if (customer == null)
                        {
                            result.FailedRecords++;
                            result.Errors.Add($"Customer {record.CustomerId} not found for order {record.OrderNumber}");
                            continue;
                        }

                        var existingPurchase = await _context.Purchases.AnyAsync(p => p.OrderNumber == record.OrderNumber);

                        if (existingPurchase)
                        {
                            result.FailedRecords++;
                            result.Errors.Add($"Duplicate order number: {record.OrderNumber}");
                            continue;
                        }

                        var purchaseDate = record.PurchaseDate ?? DateTime.UtcNow;
                        var campaignReward = await _context.Campaigns
                            .Where(r => r.CustomerId == customer.Id && r.RewardDate <= purchaseDate)
                            .OrderByDescending(r => r.RewardDate)
                            .FirstOrDefaultAsync();

                        var purchase = new Purchase
                        {
                            CustomerId = customer.Id,
                            CampaignId = campaignReward?.Id,
                            OrderNumber = record.OrderNumber,
                            Amount = record.Amount,
                            DiscountApplied = record.DiscountApplied ?? 0,
                            PurchaseDate = purchaseDate,
                            IsSuccessful = record.IsSuccessful ?? true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Purchases.Add(purchase);
                        result.SuccessfulRecords++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedRecords++;
                        result.Errors.Add($"Error processing order {record.OrderNumber}: {ex.Message}");
                        _logger.LogError(ex, "Error processing purchase record {OrderNumber}", record.OrderNumber);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase import completed: {Success} successful, {Failed} failed out of {Total}",
                    result.SuccessfulRecords, result.FailedRecords, result.TotalRecords);

                return ServiceResult<PurchaseImportResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing purchases from CSV");
                return ServiceResult<PurchaseImportResultDto>.Failure($"Error processing CSV file: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<PurchaseDto>>> GetPurchasesByCustomerAsync(string customerId)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (customer == null)
                    return ServiceResult<List<PurchaseDto>>.Failure($"Customer {customerId} not found");

                var purchases = await _context.Purchases
                    .Include(p => p.CampaignReward)
                    .Where(p => p.CustomerId == customer.Id)
                    .Select(p => new PurchaseDto
                    {
                        OrderNumber = p.OrderNumber,
                        Amount = p.Amount,
                        DiscountApplied = p.DiscountApplied,
                        PurchaseDate = p.PurchaseDate,
                        IsSuccessful = p.IsSuccessful,
                        HasCampaignDiscount = p.CampaignId.HasValue,
                        DiscountPercentage = p.CampaignReward != null ? p.CampaignReward.DiscountPercentage : 0
                    })
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToListAsync();

                return ServiceResult<List<PurchaseDto>>.Success(purchases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchases for customer {CustomerId}", customerId);
                return ServiceResult<List<PurchaseDto>>.Failure("Error retrieving purchase history");
            }
        }

        public async Task<ServiceResult<CampaignResultsDto>> GetCampaignResultsAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                _logger.LogInformation("Generating campaign results from {StartDate} to {EndDate}", start, end);

                var campaignResults = await _context.Campaigns
                    .Include(r => r.Customer)
                    .Include(r => r.Agent)
                    .Where(r => r.RewardDate >= start && r.RewardDate <= end)
                    .Select(r => new
                    {
                        Reward = r,
                        Purchases = _context.Purchases
                            .Where(p => p.CampaignId == r.Id && p.IsSuccessful)
                            .ToList()
                    })
                    .ToListAsync();

                var results = new CampaignResultsDto
                {
                    StartDate = start,
                    EndDate = end,
                    TotalRewardsIssued = campaignResults.Count,
                    TotalCustomersRewarded = campaignResults.Select(r => r.Reward.CustomerId).Distinct().Count(),
                    CustomerResults = campaignResults
                        .GroupBy(r => r.Reward.CustomerId)
                        .Select(g =>
                        {
                            var firstReward = g.First().Reward;
                            var allPurchases = g.SelectMany(r => r.Purchases).ToList();

                            return new CustomerCampaignResult
                            {
                                CustomerId = firstReward.Customer.CustomerId,
                                CustomerName = firstReward.Customer.Name,
                                RewardsReceived = g.Count(),
                                PurchasesMade = allPurchases.Count,
                                TotalSpent = allPurchases.Sum(p => p.Amount),
                                TotalSaved = allPurchases.Sum(p => p.DiscountApplied),
                                LastPurchaseDate = allPurchases.Any() ? allPurchases.Max(p => p.PurchaseDate) : null
                            };
                        })
                        .OrderByDescending(c => c.TotalSpent)
                        .ToList()
                };

                results.TotalRevenue = results.CustomerResults.Sum(c => c.TotalSpent);
                results.TotalDiscountsGiven = results.CustomerResults.Sum(c => c.TotalSaved);
                results.ConversionRate = results.TotalRewardsIssued > 0
                    ? (decimal)results.CustomerResults.Count(c => c.PurchasesMade > 0) / results.TotalRewardsIssued * 100
                    : 0;

                _logger.LogInformation("Campaign results generated: {Rewards} rewards, {Revenue} revenue, {Conversion}% conversion",
                    results.TotalRewardsIssued, results.TotalRevenue, results.ConversionRate);

                return ServiceResult<CampaignResultsDto>.Success(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaign results");
                return ServiceResult<CampaignResultsDto>.Failure("Error retrieving campaign results");
            }
        }
    }
}


        
    

