using ProjectPulse.Core.DTOs.Auth;

namespace ProjectPulse.Core.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, AuthResponseDto? Data, string Message)> LoginAsync(LoginDto dto);
    Task<(bool Success, string Message)> VerifyEmailAsync(string userId, string token);
    Task<(bool Success, AuthResponseDto? Data, string Message)> RefreshTokenAsync(string refreshToken);
}