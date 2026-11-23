using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroomErp.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet("jobs")]
    public async Task<ActionResult<List<JobDto>>> GetJobs([FromQuery] QueryParams query)
    {
        var result = await _jobService.GetJobsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpGet("jobs/{id:int}")]
    public async Task<ActionResult<JobDto>> GetJobById(int id)
    {
        var job = await _jobService.GetByIdAsync(id);
        return Ok(job);
    }

    [HttpPatch("jobs/{id:int}")]
    public async Task<ActionResult<JobDto>> UpdateJob(int id, [FromBody] UpdateJobRequest request)
    {
        var job = await _jobService.UpdateAsync(id, request);
        return Ok(job);
    }

    [HttpPost("jobs/{id:int}/assign")]
    public async Task<IActionResult> AssignStaff(int id, [FromBody] AssignStaffRequest request)
    {
        await _jobService.AssignStaffAsync(id, request);
        return NoContent();
    }

    [HttpDelete("jobs/{id:int}/assign/{staffId:int}")]
    public async Task<IActionResult> RemoveStaff(int id, int staffId)
    {
        await _jobService.RemoveStaffAsync(id, staffId);
        return NoContent();
    }

    [HttpPost("jobs/{id:int}/team")]
    public async Task<IActionResult> AssignTeam(int id, [FromBody] Dictionary<string, int> body)
    {
        if (body.TryGetValue("team_id", out var teamId))
        {
            await _jobService.AssignTeamAsync(id, teamId);
            return NoContent();
        }
        return BadRequest(new { detail = "team_id required" });
    }

    [HttpPost("jobs/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateJobStatusRequest request)
    {
        await _jobService.UpdateStatusAsync(id, request);
        return NoContent();
    }

    [HttpPost("jobs/{id:int}/complete")]
    public async Task<IActionResult> CompleteJob(int id, [FromBody] CompleteJobRequest request)
    {
        await _jobService.CompleteJobAsync(id, request);
        return NoContent();
    }

    [HttpPost("jobs/{id:int}/cancel")]
    public async Task<IActionResult> CancelJob(int id, [FromBody] Dictionary<string, string> body)
    {
        var reason = body.GetValueOrDefault("reason", "Cancelled");
        await _jobService.RequestCancelAsync(id, reason);
        return NoContent();
    }

    [HttpGet("my-jobs")]
    public async Task<ActionResult<List<MyJobDto>>> GetMyJobs()
    {
        var jobs = await _jobService.GetMyJobsAsync();
        return Ok(jobs);
    }
}
