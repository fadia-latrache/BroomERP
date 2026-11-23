using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroomErp.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet("staff")]
    public async Task<ActionResult<List<StaffDto>>> GetStaff([FromQuery] QueryParams query)
    {
        var result = await _staffService.GetStaffAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpGet("staff/{id:int}")]
    public async Task<ActionResult<StaffDto>> GetStaffById(int id)
    {
        var staff = await _staffService.GetByIdAsync(id);
        return Ok(staff);
    }

    [HttpPost("staff")]
    public async Task<ActionResult<StaffDto>> CreateStaff([FromBody] CreateStaffRequest request)
    {
        var staff = await _staffService.CreateAsync(request);
        return Created($"/api/staff/{staff.Id}", staff);
    }

    [HttpPatch("staff/{id:int}")]
    public async Task<ActionResult<StaffDto>> UpdateStaff(int id, [FromBody] UpdateStaffRequest request)
    {
        var staff = await _staffService.UpdateAsync(id, request);
        return Ok(staff);
    }

    [HttpGet("teams")]
    public async Task<ActionResult<List<TeamDto>>> GetTeams()
    {
        var teams = await _staffService.GetTeamsAsync();
        return Ok(teams);
    }

    [HttpPost("teams")]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var team = await _staffService.CreateTeamAsync(request);
        return Created($"/api/teams/{team.Id}", team);
    }

    [HttpPost("teams/{id:int}/members")]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberRequest request)
    {
        await _staffService.AddTeamMemberAsync(id, request.StaffId);
        return NoContent();
    }

    [HttpDelete("teams/{id:int}/members/{staffId:int}")]
    public async Task<IActionResult> RemoveMember(int id, int staffId)
    {
        await _staffService.RemoveTeamMemberAsync(id, staffId);
        return NoContent();
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<List<AttendanceDto>>> GetAttendance([FromQuery] QueryParams query)
    {
        var result = await _staffService.GetAttendanceAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("attendance")]
    public async Task<ActionResult<AttendanceDto>> RecordAttendance([FromBody] CreateAttendanceRequest request)
    {
        var attendance = await _staffService.RecordAttendanceAsync(request);
        return Ok(attendance);
    }

    [HttpGet("roster")]
    public async Task<ActionResult<List<RosterEntryDto>>> GetRoster([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var startDate = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var endDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
        var entries = await _staffService.GetRosterAsync(startDate, endDate);
        return Ok(entries);
    }

    [HttpGet("jobs/{jobId:int}/roster")]
    public async Task<ActionResult<List<RosterEntryDto>>> GetJobRoster(int jobId)
    {
        var entries = await _staffService.GetJobRosterAsync(jobId);
        return Ok(entries);
    }

    [HttpPost("jobs/{jobId:int}/roster")]
    public async Task<ActionResult<RosterEntryDto>> AddJobRoster(int jobId, [FromBody] CreateRosterRequest request)
    {
        var entry = await _staffService.AddRosterEntryAsync(jobId, request);
        return Ok(entry);
    }

    [HttpDelete("roster/{entryId:int}")]
    public async Task<IActionResult> RemoveRosterEntry(int entryId)
    {
        await _staffService.RemoveRosterEntryAsync(entryId);
        return NoContent();
    }
}
