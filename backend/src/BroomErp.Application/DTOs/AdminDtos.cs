using BroomErp.Domain.Enums;

namespace BroomErp.Application.DTOs;

public record EnquiryDto(
    int Id,
    string Number,
    int CustomerId,
    string? CustomerName,
    string ServiceType,
    string Address,
    DateOnly? RequestedDate,
    string Notes,
    EnquiryStatus Status,
    string CreatedBy,
    DateTime CreatedAt
);

public record CreateEnquiryRequest(
    int CustomerId,
    string ServiceType,
    string Address = "",
    DateOnly? RequestedDate = null,
    string Notes = ""
);

public record UpdateEnquiryStatusRequest(EnquiryStatus Status);

public record ApprovalDto(
    int Id,
    string Kind,
    string DocType,
    int DocId,
    string DocNumber,
    ApprovalStatus Status,
    string Note,
    string RequestedBy,
    string? DecidedBy,
    DateTime? DecidedAt,
    string? Reason,
    DateTime CreatedAt
);

public record DecideApprovalRequest(bool Approved, string? Reason = null);

public record AuditLogDto(
    int Id,
    string Actor,
    string Role,
    string Method,
    string Path,
    int StatusCode,
    DateTime CreatedAt
);

public record AuditChangeDto(
    int Id,
    string Entity,
    int EntityId,
    string Field,
    string? Old,
    string? New,
    string Actor,
    DateTime CreatedAt
);

public record ServiceTypeDto(int Id, string Code, string Name, bool Active);
public record CreateServiceTypeRequest(string Code, string Name);

public record CompanyProfileDto(
    int Id,
    string Name,
    string Address,
    string Phone,
    string CrNumber,
    string BankDetails,
    int? LogoAttachmentId
);

public record UpdateCompanyProfileRequest(
    string? Name = null,
    string? Address = null,
    string? Phone = null,
    string? CrNumber = null,
    string? BankDetails = null
);

public record AttachmentDto(
    int Id,
    string EntityType,
    int EntityId,
    string Filename,
    string ContentType,
    long Size,
    string UploadedBy,
    DateTime CreatedAt
);
