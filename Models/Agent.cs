namespace ComtradeAPI.Models
{
    public class Agent
    {
        public int Id { get; set; }
        public string AgentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
