using ComtradeAPI.Data;
using ComtradeAPI.ModelDTO;
using ComtradeAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ComtradeAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly CampaignDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public AuthService(CampaignDbContext context, IConfiguration configuration, ILogger logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return ServiceResult<bool>.Failure("User not found");

                if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
                    return ServiceResult<bool>.Failure("Current password is incorrect");

                user.PasswordHash = HashPassword(request.NewPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password changed for user {UserId}", userId);
                return ServiceResult<bool>.Success(true);


            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return ServiceResult<bool>.Failure("Password change failed");
            }
        }

        public Task<ServiceResult<UserInfo>> GetCurrentUserAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<LoginResponseNew>> LoginAsync(LoginRequestNew request)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Agent)
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null || !user.IsActive)
                {
                    return ServiceResult<LoginResponseNew>.Failure("Invalid username or password");
                }

                if (!VerifyPassword(request.Password, user.PasswordHash))
                    return ServiceResult<LoginResponseNew>.Failure("Invalid username or password");

                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var accessToken = GenerateAccessToken(user);
                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                var response = new LoginResponseNew
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = 3600,
                    User = MapToUserInfo(user)
                };

                _logger.LogInformation("User {Username} logged in successfully", user.Username);
                return ServiceResult<LoginResponseNew>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return ServiceResult<LoginResponseNew>.Failure("Login failed");
            }


        }

        public async Task<ServiceResult<LoginResponseNew>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var token = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .ThenInclude(u => u.Agent)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
                    return ServiceResult<LoginResponseNew>.Failure("Invalid or expired refresh token");

                if (!token.User.IsActive)
                    return ServiceResult<LoginResponseNew>.Failure("User is inactive");

                // Revoke old token
                token.IsRevoked = true;

                var accessToken = GenerateAccessToken(token.User);
                var newRefreshToken = await GenerateRefreshTokenAsync(token.User.Id);

                await _context.SaveChangesAsync();

                var response = new LoginResponseNew
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = 3600,
                    User = MapToUserInfo(token.User)
                };

                return ServiceResult<LoginResponseNew>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return ServiceResult<LoginResponseNew>.Failure("Token refresh failed");
            }
        }

        public async Task<ServiceResult<LoginResponseNew>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                    return ServiceResult<LoginResponseNew>.Failure("Username already exists");

                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                    return ServiceResult<LoginResponseNew>.Failure("Email already exists");

                var validRoles = new[] { "Admin", "Agent", "Manager" };
                if (!validRoles.Contains(request.Role))
                    return ServiceResult<LoginResponseNew>.Failure("Invalid role");

                if (request.Role == "Agent")
                {
                    if (!request.AgentId.HasValue)
                        return ServiceResult<LoginResponseNew>.Failure("AgentId is required for Agent role");

                    var agentExists = await _context.Agents.AnyAsync(a => a.Id == request.AgentId.Value);
                    if (!agentExists)
                        return ServiceResult<LoginResponseNew>.Failure("Agent not found");
                }

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    Role = request.Role,
                    AgentId = request.AgentId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (user.AgentId.HasValue)
                {
                    user.Agent = await _context.Agents.FindAsync(user.AgentId.Value);
                }

                var accessToken = GenerateAccessToken(user);
                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                var response = new LoginResponseNew
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = 3600,
                    User = MapToUserInfo(user)
                };

                _logger.LogInformation("User {Username} registered successfully", user.Username);
                return ServiceResult<LoginResponseNew>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
                return ServiceResult<LoginResponseNew>.Failure("Registration failed");
            }

        }
        

        public async Task<ServiceResult<bool>> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (token != null)
                {
                    token.IsRevoked = true;
                    await _context.SaveChangesAsync();
                }

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return ServiceResult<bool>.Failure("Token revocation failed");
            }
        }

        private string GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            if (user.AgentId.HasValue)
            {
                claims.Add(new Claim("AgentId", user.AgentId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private async Task<string> GenerateRefreshTokenAsync(int userId)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var token = Convert.ToBase64String(randomBytes);

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return token;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private UserInfo MapToUserInfo(User user)
        {
            return new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                AgentId = user.AgentId,
                AgentName = user.Agent?.Name,
                AgentCode = user.Agent?.AgentCode
            };
        }


    }
}
