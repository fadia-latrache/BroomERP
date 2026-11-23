using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroomErp.Api.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserContext _userContext;

    public AuthController(IAuthService authService, ICurrentUserContext userContext)
    {
        _authService = authService;
        _userContext = userContext;
    }

    [HttpPost("auth/login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpGet("users")]
    [Authorize]
    public async Task<ActionResult<List<UserDto>>> GetUsers([FromQuery] QueryParams query)
    {
        var result = await _authService.GetUsersAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("users")]
    [Authorize(Roles = "GM")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _authService.CreateUserAsync(request);
        return Created($"/api/users/{user.Id}", user);
    }

    [HttpPost("users/change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!_userContext.UserId.HasValue)
            return Unauthorized();

        await _authService.ChangePasswordAsync(_userContext.UserId.Value, request);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = _userContext.UserId,
            email = _userContext.Email,
            role = _userContext.Role?.ToString() ?? "FIELD",
            access = new
            {
                customers = new { read = true, write = _userContext.Role != Role.FIELD },
                quotations = new { read = true, write = _userContext.Role != Role.FIELD },
                jobs = new { read = true, write = true },
                invoices = new { read = _userContext.Role != Role.FIELD, write = _userContext.Role == Role.GM || _userContext.Role == Role.FINANCE },
                store = new { read = _userContext.Role != Role.FIELD, write = _userContext.Role == Role.GM || _userContext.Role == Role.STORE },
                purchasing = new { read = _userContext.Role != Role.FIELD, write = _userContext.Role == Role.GM || _userContext.Role == Role.STORE },
                reports = new { read = _userContext.Role == Role.GM || _userContext.Role == Role.FINANCE, write = false },
                admin = new { read = _userContext.Role == Role.GM, write = _userContext.Role == Role.GM }
            }
        });
    }
}
