using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;

namespace BroomErp.Application.Interfaces.Services;

public interface IJobService
{
    Task<PagedResult<JobDto>> GetJobsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<JobDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JobDto> UpdateAsync(int id, UpdateJobRequest request, CancellationToken cancellationToken = default);
    Task AssignStaffAsync(int id, AssignStaffRequest request, CancellationToken cancellationToken = default);
    Task RemoveStaffAsync(int id, int staffId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int id, UpdateJobStatusRequest request, CancellationToken cancellationToken = default);
    Task CompleteJobAsync(int id, CompleteJobRequest request, CancellationToken cancellationToken = default);
    Task AssignTeamAsync(int id, int teamId, CancellationToken cancellationToken = default);
    Task RequestCancelAsync(int id, string reason, CancellationToken cancellationToken = default);
    Task<List<MyJobDto>> GetMyJobsAsync(CancellationToken cancellationToken = default);
}
