using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Enums;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Application.Services;

public class JobService : IJobService
{
    private readonly IRepository<Job> _jobRepo;
    private readonly IRepository<JobAssignment> _assignRepo;
    private readonly IRepository<JobCompletion> _completionRepo;
    private readonly IRepository<TeamMember> _teamMemberRepo;
    private readonly IRepository<Invoice> _invoiceRepo;
    private readonly IUnitOfWork _uow;
    private readonly ISequenceGenerator _sequences;
    private readonly ICurrentUserContext _userContext;

    public JobService(
        IRepository<Job> jobRepo,
        IRepository<JobAssignment> assignRepo,
        IRepository<JobCompletion> completionRepo,
        IRepository<TeamMember> teamMemberRepo,
        IRepository<Invoice> invoiceRepo,
        IUnitOfWork uow,
        ISequenceGenerator sequences,
        ICurrentUserContext userContext)
    {
        _jobRepo = jobRepo;
        _assignRepo = assignRepo;
        _completionRepo = completionRepo;
        _teamMemberRepo = teamMemberRepo;
        _invoiceRepo = invoiceRepo;
        _uow = uow;
        _sequences = sequences;
        _userContext = userContext;
    }

    public async Task<PagedResult<JobDto>> GetJobsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _jobRepo.GetPagedAsync(query,
            j => (string.IsNullOrEmpty(query.Search) || j.Number.Contains(query.Search) || (j.Customer != null && j.Customer.Name.ToLower().Contains(query.Search.ToLower()))),
            cancellationToken,
            j => j.Customer!,
            j => j.Assignments);

        return new PagedResult<JobDto>
        {
            Items = paged.Items.Select(MapToDto).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<JobDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var j = await _jobRepo.GetByIdAsync(id, cancellationToken, j => j.Customer!, j => j.Assignments)
            ?? throw new DomainException("Job not found", 404);
        return MapToDto(j);
    }

    public async Task<JobDto> UpdateAsync(int id, UpdateJobRequest request, CancellationToken cancellationToken = default)
    {
        var j = await _jobRepo.GetByIdAsync(id, cancellationToken, j => j.Customer!, j => j.Assignments)
            ?? throw new DomainException("Job not found", 404);

        if (request.ScheduledFor.HasValue) j.ScheduledFor = request.ScheduledFor.Value;
        if (request.StartTime != null) j.StartTime = request.StartTime;
        if (request.DurationHours.HasValue) j.DurationHours = request.DurationHours.Value;
        if (request.Address != null) j.Address = request.Address;
        if (request.Notes != null) j.Notes = request.Notes;

        await _uow.SaveChangesAsync(cancellationToken);
        return MapToDto(j);
    }

    public async Task AssignStaffAsync(int id, AssignStaffRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Job not found", 404);

        var existing = await _assignRepo.ListAsync(a => a.JobId == id && a.StaffId == request.StaffId, cancellationToken);
        var active = existing.FirstOrDefault(a => a.Active);
        if (active != null) return;

        var assign = new JobAssignment
        {
            JobId = id,
            StaffId = request.StaffId,
            Role = request.Role,
            Active = true
        };
        await _assignRepo.AddAsync(assign, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveStaffAsync(int id, int staffId, CancellationToken cancellationToken = default)
    {
        var existing = await _assignRepo.ListAsync(a => a.JobId == id && a.StaffId == staffId && a.Active, cancellationToken);
        var active = existing.FirstOrDefault();
        if (active != null)
        {
            active.Active = false;
            active.RemovedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateStatusAsync(int id, UpdateJobStatusRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Job not found", 404);

        if (request.Status == JobStatus.COMPLETED)
        {
            var completion = await _completionRepo.ListAsync(c => c.JobId == id, cancellationToken);
            if (completion.Count == 0)
                throw new DomainException("Cannot complete job without a completion report");
        }

        job.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            job.ProblemNotes = string.IsNullOrWhiteSpace(job.ProblemNotes)
                ? request.Note
                : $"{job.ProblemNotes}\n{request.Note}";
        }
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteJobAsync(int id, CompleteJobRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Job not found", 404);

        var completion = new JobCompletion
        {
            JobId = id,
            SubmittedBy = _userContext.Email ?? "field",
            WorkDone = request.WorkDone,
            CompletionStatus = request.CompletionStatus,
            Issues = request.Issues,
            Complaint = request.Complaint,
            MissingDamaged = request.MissingDamaged,
            Notes = request.Notes
        };
        await _completionRepo.AddAsync(completion, cancellationToken);

        job.Status = JobStatus.COMPLETED;
        await _uow.SaveChangesAsync(cancellationToken);

        // Auto-generate invoice if not exists
        var existingInvoice = await _invoiceRepo.AnyAsync(i => i.JobId == id, cancellationToken);
        if (!existingInvoice)
        {
            var invNumber = await _sequences.NextAsync("INV-", cancellationToken);
            var inv = new Invoice
            {
                Number = invNumber,
                CustomerId = job.CustomerId,
                JobId = job.Id,
                Total = job.Amount,
                Status = InvoiceStatus.UNPAID
            };
            await _invoiceRepo.AddAsync(inv, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AssignTeamAsync(int id, int teamId, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Job not found", 404);

        job.TeamId = teamId;
        var members = await _teamMemberRepo.ListAsync(m => m.TeamId == teamId, cancellationToken);
        foreach (var m in members)
        {
            await AssignStaffAsync(id, new AssignStaffRequest(m.StaffId, "CREW"), cancellationToken);
        }
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task RequestCancelAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Job not found", 404);

        job.CancelRequested = true;
        job.CancelReason = reason;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<MyJobDto>> GetMyJobsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var jobs = await _jobRepo.ListAsync(
            j => j.Status == JobStatus.PLANNED || j.Status == JobStatus.IN_PROGRESS,
            cancellationToken,
            j => j.Customer!,
            j => j.Assignments);

        return jobs.Select(j => new MyJobDto(
            j.Id, j.Number, j.CustomerName, j.ServiceType, j.Address, j.ScheduledFor,
            j.StartTime, j.Status, j.StaffCount
        )).ToList();
    }

    private static JobDto MapToDto(Job j)
    {
        return new JobDto(
            j.Id,
            j.Number,
            j.CustomerId,
            j.CustomerName,
            j.QuotationId,
            j.ServiceType,
            j.Address,
            j.ScheduledFor,
            j.StartTime,
            j.DurationHours,
            j.Notes,
            j.ProblemNotes,
            j.EndDate,
            j.TeamId,
            j.CancelRequested,
            j.CancelReason,
            j.Amount,
            j.Status,
            j.StaffCount,
            j.Delayed,
            j.CreatedAt
        );
    }
}
