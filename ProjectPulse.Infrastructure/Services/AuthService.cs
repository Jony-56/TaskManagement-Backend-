using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectPulse.Core.DTOs.Auth;
using ProjectPulse.Core.Entities;
using ProjectPulse.Core.Interfaces;
using ProjectPulse.Infrastructure.Data;

namespace ProjectPulse.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public AuthService(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        TokenService tokenService,
        IConfiguration config,
        AppDbContext db)
    {
        _userManager = userManager;
        _emailService = emailService;
        _tokenService = tokenService;
        _config = config;
        _db = db;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            return (false, "Email already registered.");

        var user = new AppUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        // Assign default role
        await _userManager.AddToRoleAsync(user, "Member");

        // Send verification email
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var verificationLink = $"{_config["AppUrl"]}/api/auth/verify-email?userId={user.Id}&token={encodedToken}";

        await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, verificationLink);

        return (true, "Registration successful. Please check your email to verify your account.");
    }

    public async Task<(bool Success, AuthResponseDto? Data, string Message)> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return (false, null, "Invalid email or password.");

        if (!user.EmailConfirmed)
            return (false, null, "Please verify your email before logging in.");

        if (!user.IsActive)
            return (false, null, "Your account has been deactivated.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiry = Convert.ToDouble(_config["JwtSettings:ExpiryInMinutes"]);
        var refreshExpiry = Convert.ToDouble(_config["JwtSettings:RefreshTokenExpiryInDays"]);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshExpiry);
        await _userManager.UpdateAsync(user);

        return (true, new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiry),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = roles
            }
        }, "Login successful.");
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return (false, "User not found.");

        var result = await _userManager.ConfirmEmailAsync(user, Uri.UnescapeDataString(token));
        if (!result.Succeeded)
            return (false, "Invalid or expired verification link.");

        await _emailService.SendWelcomeEmailAsync(user.Email!, user.FullName);
        return (true, "Email verified successfully. You can now log in.");
    }

    public async Task<(bool Success, AuthResponseDto? Data, string Message)> RefreshTokenAsync(string refreshToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow);

        if (user is null) return (false, null, "Invalid or expired refresh token.");

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiry = Convert.ToDouble(_config["JwtSettings:ExpiryInMinutes"]);
        var refreshExpiry = Convert.ToDouble(_config["JwtSettings:RefreshTokenExpiryInDays"]);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshExpiry);
        await _userManager.UpdateAsync(user);

        return (true, new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiry),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = roles
            }
        }, "Token refreshed.");
    }
}