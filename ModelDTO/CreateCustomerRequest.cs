namespace ComtradeAPI.ModelDTO
{
    public record CreateCustomerRequest(
        string CustomerId,
        string Name,
        string Email,
        string PhoneNumber,
        bool IsLoyalCustomer);
    
}
