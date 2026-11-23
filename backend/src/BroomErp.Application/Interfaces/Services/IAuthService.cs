using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Domain.Enums;

namespace BroomErp.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<UserDto>> GetUsersAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(int userId, ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task EnsureAdminUserAsync(string email, string password, CancellationToken cancellationToken = default);
}
