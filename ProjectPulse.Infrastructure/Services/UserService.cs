using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectPulse.Core.DTOs.User;
using ProjectPulse.Core.Entities;
using ProjectPulse.Core.Interfaces;

namespace ProjectPulse.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;

    public UserService(
        UserManager<AppUser> userManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    // ─── Helpers ──────────────────────────────────────────────

    private async Task<UserDetailsDto> MapToDto(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserDetailsDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles
        };
    }

    // ─── Queries ──────────────────────────────────────────────

    public async Task<IEnumerable<UserDetailsDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var result = new List<UserDetailsDto>();
        foreach (var user in users)
            result.Add(await MapToDto(user));

        return result;
    }

    public async Task<UserDetailsDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : await MapToDto(user);
    }

    public async Task<UserDetailsDto?> GetCurrentUserAsync(string userId)
    {
        return await GetUserByIdAsync(userId);
    }

    // ─── Commands ─────────────────────────────────────────────

    public async Task<(bool Success, string Message)> UpdateUserAsync(
        string userId, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return (false, "User not found.");

        user.FullName = dto.FullName;
        user.AvatarUrl = dto.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? (true, "Profile updated successfully.")
            : (false, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Success, string Message)> AssignRoleAsync(AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null) return (false, "User not found.");

        var validRoles = new[] { "Admin", "ProjectManager", "Member" };
        if (!validRoles.Contains(dto.Role))
            return (false, $"Invalid role. Valid roles: {string.Join(", ", validRoles)}");

        if (await _userManager.IsInRoleAsync(user, dto.Role))
            return (false, $"User already has the role '{dto.Role}'.");

        var result = await _userManager.AddToRoleAsync(user, dto.Role);
        return result.Succeeded
            ? (true, $"Role '{dto.Role}' assigned successfully.")
            : (false, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Success, string Message)> RemoveRoleAsync(AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null) return (false, "User not found.");

        if (!await _userManager.IsInRoleAsync(user, dto.Role))
            return (false, $"User does not have the role '{dto.Role}'.");

        var result = await _userManager.RemoveFromRoleAsync(user, dto.Role);
        return result.Succeeded
            ? (true, $"Role '{dto.Role}' removed successfully.")
            : (false, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Success, string Message)> DeactivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return (false, "User not found.");
        if (!user.IsActive) return (false, "User is already deactivated.");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        return (true, "User deactivated successfully.");
    }

    public async Task<(bool Success, string Message)> ActivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return (false, "User not found.");
        if (user.IsActive) return (false, "User is already active.");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        return (true, "User activated successfully.");
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(
        string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return (false, "User not found.");

        var result = await _userManager.ChangePasswordAsync(
            user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        // 📧 Notify user of password change
        if (user.Email is not null)
        {
            try
            {
                await _emailService.SendPasswordChangedEmailAsync(user.Email, user.FullName);
            }
            catch { }
        }

        return (true, "Password changed successfully.");
    }
}