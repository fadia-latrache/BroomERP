using BroomErp.Domain.Enums;

namespace BroomErp.Application.DTOs;

public record JobDto(
    int Id,
    string Number,
    int CustomerId,
    string? CustomerName,
    int? QuotationId,
    string ServiceType,
    string Address,
    DateOnly ScheduledFor,
    string? StartTime,
    decimal? DurationHours,
    string Notes,
    string ProblemNotes,
    DateOnly? EndDate,
    int? TeamId,
    bool CancelRequested,
    string? CancelReason,
    decimal Amount,
    JobStatus Status,
    int StaffCount,
    bool Delayed,
    DateTime CreatedAt
);

public record UpdateJobRequest(
    DateOnly? ScheduledFor = null,
    string? StartTime = null,
    decimal? DurationHours = null,
    string? Address = null,
    string? Notes = null
);

public record AssignStaffRequest(int StaffId, string Role = "CREW");
public record UpdateJobStatusRequest(JobStatus Status, string Note = "");
public record CompleteJobRequest(
    string WorkDone,
    string CompletionStatus = "FULL",
    string Issues = "",
    string Complaint = "",
    string MissingDamaged = "",
    string Notes = ""
);

public record MyJobDto(
    int Id,
    string Number,
    string? CustomerName,
    string ServiceType,
    string Address,
    DateOnly ScheduledFor,
    string? StartTime,
    JobStatus Status,
    int StaffCount
);
