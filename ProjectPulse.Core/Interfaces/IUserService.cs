using ProjectPulse.Core.DTOs.User;

namespace ProjectPulse.Core.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDetailsDto>> GetAllUsersAsync();
    Task<UserDetailsDto?> GetUserByIdAsync(string userId);
    Task<(bool Success, string Message)> UpdateUserAsync(string userId, UpdateUserDto dto);
    Task<(bool Success, string Message)> AssignRoleAsync(AssignRoleDto dto);
    Task<(bool Success, string Message)> RemoveRoleAsync(AssignRoleDto dto);
    Task<(bool Success, string Message)> DeactivateUserAsync(string userId);
    Task<(bool Success, string Message)> ActivateUserAsync(string userId);
    Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task<UserDetailsDto?> GetCurrentUserAsync(string userId);
}