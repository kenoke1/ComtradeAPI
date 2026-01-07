using ComtradeAPI.Data;
using ComtradeAPI.ModelDTO;
using ComtradeAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace ComtradeAPI.Services
{
    public class SoapCustomerService : ISoapCustomerService
    {
        private readonly HttpClient _httpClient;
        private readonly CampaignDbContext _context;
        private readonly ILogger<SoapCustomerService> _logger;
        private const string SoapEndpoint = "https://www.crcind.com/csp/samples/SOAP.Demo.cls";

        public SoapCustomerService(HttpClient httpClient, CampaignDbContext context, ILogger<SoapCustomerService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }
        public async Task<CustomerDto?> FindPersonAsync(string customerId)
        {
            try
            {
                _logger.LogInformation("Calling SOAP service for customer {CustomerId}", customerId);

                var soapRequest = BuildFindPersonSoapRequest(customerId);
                var content = new StringContent(soapRequest, System.Text.Encoding.UTF8, "text/xml");
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "text/xml; charset=utf-8");
                content.Headers.Add("SOAPAction", "http://tempuri.org/SOAP.Demo.FindPerson");

                var response = await _httpClient.PostAsync(SoapEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("SOAP request failed with status {StatusCode} for customer {CustomerId}", response.StatusCode, customerId);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("SOAP Response: {Response}", responseContent);

                return ParseFindPersonResponse(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling FindPerson SOAP service for customer {CustomerId}", customerId);
                return null;
            }
        }
        

        public async Task<bool> SyncCustomerDataAsync(string customerId)
        {
            try
            {
                var customerData = await FindPersonAsync(customerId);

                if (customerData == null)
                {
                    _logger.LogWarning("Customer {CustomerId} not found in SOAP service", customerId);
                    return false;
                }

                var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (existingCustomer != null)
                {
                    existingCustomer.Name = customerData.Name;
                    existingCustomer.Email = customerData.Email;
                    existingCustomer.PhoneNumber = customerData.PhoneNumber;
                    _context.Customers.Update(existingCustomer);
                    _logger.LogInformation("Updated existing customer {CustomerId}", customerId);
                }
                else
                {
                    var newCustomer = new Customer
                    {
                        CustomerId = customerId,
                        Name = customerData.Name,
                        Email = customerData.Email,
                        PhoneNumber = customerData.PhoneNumber,
                        IsLoyalCustomer = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Customers.Add(newCustomer);
                    _logger.LogInformation("Created new customer {CustomerId}", customerId);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing customer {CustomerId}", customerId);
                return false;
            }


        }

        private string BuildFindPersonSoapRequest(string customerId)
        {
                        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
                           xmlns:tem=""http://tempuri.org"">
              <soap:Header/>
              <soap:Body>
                <tem:FindPerson>
                  <tem:id>{customerId}</tem:id>
                </tem:FindPerson>
              </soap:Body>
            </soap:Envelope>";
        }

        private CustomerDto? ParseFindPersonResponse(string soapResponse)
        {
            try
            {
                var doc = XDocument.Parse(soapResponse);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace tempNs = "http://tempuri.org";

                var body = doc.Root?.Element(soapNs + "Body");
                if (body == null) return null;

                var findPersonResponse = body.Element(tempNs + "FindPersonResponse");
                if (findPersonResponse == null) return null;

                var findPersonResult = findPersonResponse.Element(tempNs + "FindPersonResult");
                if (findPersonResult == null) return null;

                var personElement = findPersonResult.Element(tempNs + "Person") ?? findPersonResult;

                var customerId = personElement.Element(tempNs + "ID")?.Value ?? personElement.Element("ID")?.Value ?? string.Empty;
                var name = personElement.Element(tempNs + "Name")?.Value ?? personElement.Element("Name")?.Value ?? string.Empty;
                var email = personElement.Element(tempNs + "Email")?.Value ?? personElement.Element("Email")?.Value ?? string.Empty;
                var phone = personElement.Element(tempNs + "Phone")?.Value ?? personElement.Element(tempNs + "PhoneNumber")?.Value
                         ?? personElement.Element("Phone")?.Value ?? personElement.Element("PhoneNumber")?.Value ?? string.Empty;

                if (string.IsNullOrEmpty(customerId)) return null;

                return new CustomerDto
                {
                    CustomerId = customerId,
                    Name = name,
                    Email = email,
                    PhoneNumber = phone,
                    IsLoyalCustomer = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing SOAP response");
                return null;
            }
        }
    }
}
