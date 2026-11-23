using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Enums;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwt;
    private const int LockAfter = 5;
    private const int LockMinutes = 15;

    public AuthService(IRepository<User> userRepo, IUnitOfWork uow, IPasswordHasher hasher, IJwtProvider jwt)
    {
        _userRepo = userRepo;
        _uow = uow;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var users = await _userRepo.ListAsync(u => u.Email == email, cancellationToken);
        var user = users.FirstOrDefault();

        var now = DateTime.UtcNow;
        if (user != null && user.LockedUntil.HasValue && user.LockedUntil.Value > now)
        {
            throw new DomainException($"Account locked after repeated failures. Try again in {LockMinutes} minutes.", 403);
        }

        if (user == null || !user.Active || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            if (user != null)
            {
                user.FailedLogins++;
                if (user.FailedLogins >= LockAfter)
                {
                    user.FailedLogins = 0;
                    user.LockedUntil = now.AddMinutes(LockMinutes);
                }
                await _uow.SaveChangesAsync(cancellationToken);
            }
            throw new DomainException("Wrong email or password", 401);
        }

        user.FailedLogins = 0;
        user.LockedUntil = null;
        await _uow.SaveChangesAsync(cancellationToken);

        var token = _jwt.GenerateToken(user.Id, user.Role, user.Email);
        return new LoginResponse(token, user.Role.ToString(), user.Email, user.Id);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Password.Length < 8)
            throw new DomainException("Password needs at least 8 characters");

        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await _userRepo.AnyAsync(u => u.Email == email, cancellationToken);
        if (exists)
            throw new DomainException("Email already in use");

        var user = new User
        {
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            Role = request.Role,
            StaffId = request.StaffId,
            Active = true
        };

        await _userRepo.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.Email, user.Role, user.Active, user.StaffId, user.CreatedAt);
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _userRepo.GetPagedAsync(query, null, cancellationToken);
        return new PagedResult<UserDto>
        {
            Items = paged.Items.Select(u => new UserDto(u.Id, u.Email, u.Role, u.Active, u.StaffId, u.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, cancellationToken)
            ?? throw new DomainException("User not found", 404);

        if (!_hasher.Verify(request.OldPassword, user.PasswordHash))
            throw new DomainException("Current password is wrong");

        if (request.NewPassword.Length < 8)
            throw new DomainException("Password needs at least 8 characters");

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        user.FailedLogins = 0;
        user.LockedUntil = null;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(int userId, ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, cancellationToken)
            ?? throw new DomainException("User not found", 404);

        if (request.NewPassword.Length < 8)
            throw new DomainException("Password needs at least 8 characters");

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        user.FailedLogins = 0;
        user.LockedUntil = null;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureAdminUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var hasUsers = await _userRepo.AnyAsync(u => true, cancellationToken);
        if (!hasUsers && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            var admin = new User
            {
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = _hasher.Hash(password),
                Role = Role.GM,
                Active = true
            };
            await _userRepo.AddAsync(admin, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
