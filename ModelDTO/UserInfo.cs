namespace ComtradeAPI.ModelDTO
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? AgentCode { get; set; }
    }
}
