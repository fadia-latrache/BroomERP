using BroomErp.Domain.Common;
using BroomErp.Domain.Enums;

namespace BroomErp.Domain.Entities;

public class Job : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }

    public string ServiceType { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateOnly ScheduledFor { get; set; }
    public string? StartTime { get; set; }
    public decimal? DurationHours { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string ProblemNotes { get; set; } = string.Empty;
    public DateOnly? EndDate { get; set; }
    public int? TeamId { get; set; }
    public Team? Team { get; set; }
    public bool CancelRequested { get; set; } = false;
    public string? CancelReason { get; set; }
    public decimal Amount { get; set; } = 0;
    public JobStatus Status { get; set; } = JobStatus.PLANNED;

    public List<JobAssignment> Assignments { get; set; } = [];
    public JobCompletion? Completion { get; set; }

    public string? CustomerName => Customer?.Name;
    public int StaffCount => Assignments.Count(a => a.Active);
    public bool Delayed => (Status == JobStatus.PLANNED || Status == JobStatus.IN_PROGRESS) &&
                           ScheduledFor < DateOnly.FromDateTime(DateTime.UtcNow);
}

public class JobAssignment : BaseEntity
{
    public int JobId { get; set; }
    public Job? Job { get; set; }
    public int StaffId { get; set; }
    public Staff? Staff { get; set; }
    public string Role { get; set; } = "CREW"; // CREW | DRIVER | SUPERVISOR
    public bool Active { get; set; } = true;
    public DateTime? RemovedAt { get; set; }

    public string? StaffName => Staff?.Name;
    public string? JobNumber => Job?.Number;
}

public class JobCompletion : BaseEntity
{
    public int JobId { get; set; }
    public Job? Job { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
    public string WorkDone { get; set; } = string.Empty;
    public string CompletionStatus { get; set; } = "FULL"; // FULL | PARTIAL
    public string Issues { get; set; } = string.Empty;
    public string Complaint { get; set; } = string.Empty;
    public string MissingDamaged { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
