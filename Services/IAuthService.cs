using ComtradeAPI.ModelDTO;

namespace ComtradeAPI.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<LoginResponseNew>> LoginAsync(LoginRequestNew request);
        Task<ServiceResult<LoginResponseNew>> RegisterAsync(RegisterRequest request);
        Task<ServiceResult<LoginResponseNew>> RefreshTokenAsync(string refreshToken);
        Task<ServiceResult<bool>> RevokeTokenAsync(string refreshToken);
        Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<ServiceResult<UserInfo>> GetCurrentUserAsync(int userId);
    }
}
