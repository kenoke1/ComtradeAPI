namespace ComtradeAPI.ModelDTO
{
    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword);
    
}
