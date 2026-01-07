namespace ComtradeAPI.ModelDTO
{
    public class CustomerDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsLoyalCustomer { get; set; }
    }
}
