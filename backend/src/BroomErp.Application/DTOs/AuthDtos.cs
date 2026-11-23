using BroomErp.Domain.Enums;

namespace BroomErp.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Role, string Email, int UserId);
public record UserDto(int Id, string Email, Role Role, bool Active, int? StaffId, DateTime CreatedAt);
public record CreateUserRequest(string Email, string Password, Role Role, int? StaffId = null);
public record ChangePasswordRequest(string OldPassword, string NewPassword);
public record ResetPasswordRequest(string NewPassword);
