namespace ComtradeAPI.ModelDTO
{
    public record RegisterRequest(
        string Username,
        string Password,
        string Email,
        string Role,
        int? AgentId

        );
    
}
