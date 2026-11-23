using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;

namespace BroomErp.Application.Interfaces.Services;

public interface IStaffService
{
    Task<PagedResult<StaffDto>> GetStaffAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<StaffDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<StaffDto> CreateAsync(CreateStaffRequest request, CancellationToken cancellationToken = default);
    Task<StaffDto> UpdateAsync(int id, UpdateStaffRequest request, CancellationToken cancellationToken = default);
    Task ToggleActiveAsync(int id, bool active, CancellationToken cancellationToken = default);

    Task<List<TeamDto>> GetTeamsAsync(CancellationToken cancellationToken = default);
    Task<TeamDto> CreateTeamAsync(CreateTeamRequest request, CancellationToken cancellationToken = default);
    Task AddTeamMemberAsync(int teamId, int staffId, CancellationToken cancellationToken = default);
    Task RemoveTeamMemberAsync(int teamId, int staffId, CancellationToken cancellationToken = default);

    Task<PagedResult<AttendanceDto>> GetAttendanceAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<AttendanceDto> RecordAttendanceAsync(CreateAttendanceRequest request, CancellationToken cancellationToken = default);

    Task<List<RosterEntryDto>> GetRosterAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<List<RosterEntryDto>> GetJobRosterAsync(int jobId, CancellationToken cancellationToken = default);
    Task<RosterEntryDto> AddRosterEntryAsync(int jobId, CreateRosterRequest request, CancellationToken cancellationToken = default);
    Task RemoveRosterEntryAsync(int entryId, CancellationToken cancellationToken = default);
}
