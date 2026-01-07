using ComtradeAPI.Data;
using ComtradeAPI.ModelDTO;
using ComtradeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ComtradeAPI.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly CampaignDbContext _context;
        private readonly ISoapCustomerService _soapService;
        private readonly ILogger<CustomerService> _logger;

        public async Task<ServiceResult<CustomerDto>> GetCustomerAsync(string customerId)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (customer == null)
                {
                    _logger.LogInformation("Customer {CustomerId} not found locally, trying SOAP service", customerId);
                    var synced = await _soapService.SyncCustomerDataAsync(customerId);

                    if (synced)
                        customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);

                    if (customer == null)
                        return ServiceResult<CustomerDto>.Failure($"Customer {customerId} not found");
                }

                var dto = new CustomerDto
                {
                    CustomerId = customer.CustomerId,
                    Name = customer.Name,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    IsLoyalCustomer = customer.IsLoyalCustomer
                };

                return ServiceResult<CustomerDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer {CustomerId}", customerId);
                return ServiceResult<CustomerDto>.Failure("Error retrieving customer data");
            }
        }

        public async Task<ServiceResult<CustomerDto>> CreateOrUpdateCustomerAsync(CreateCustomerRequest request)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);

                if (customer != null)
                {
                    customer.Name = request.Name;
                    customer.Email = request.Email;
                    customer.PhoneNumber = request.PhoneNumber;
                    customer.IsLoyalCustomer = request.IsLoyalCustomer;
                    _context.Customers.Update(customer);
                }
                else
                {
                    customer = new Customer
                    {
                        CustomerId = request.CustomerId,
                        Name = request.Name,
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        IsLoyalCustomer = request.IsLoyalCustomer,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Customers.Add(customer);
                }

                await _context.SaveChangesAsync();

                var dto = new CustomerDto
                {
                    CustomerId = customer.CustomerId,
                    Name = customer.Name,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    IsLoyalCustomer = customer.IsLoyalCustomer
                };

                return ServiceResult<CustomerDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating customer");
                return ServiceResult<CustomerDto>.Failure("Error processing customer data");
            }
        }

        public async Task<ServiceResult<List<CustomerDto>>> GetLoyalCustomersAsync()
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => c.IsLoyalCustomer)
                    .Select(c => new CustomerDto
                    {
                        CustomerId = c.CustomerId,
                        Name = c.Name,
                        Email = c.Email,
                        PhoneNumber = c.PhoneNumber,
                        IsLoyalCustomer = c.IsLoyalCustomer
                    })
                    .ToListAsync();

                return ServiceResult<List<CustomerDto>>.Success(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loyal customers");
                return ServiceResult<List<CustomerDto>>.Failure("Error retrieving customer list");
            }
        }
    }
}
