using BroomErp.Domain.Common;
using BroomErp.Domain.Enums;

namespace BroomErp.Domain.Entities;

public class User : BaseEntity, IAggregateRoot
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool Active { get; set; } = true;
    public int FailedLogins { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }
    public int? StaffId { get; set; }
    public Staff? Staff { get; set; }
}

public class Enquiry : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateOnly? RequestedDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public EnquiryStatus Status { get; set; } = EnquiryStatus.NEW;
    public string CreatedBy { get; set; } = string.Empty;

    public string? CustomerName => Customer?.Name;
}

public class AuditLog : BaseEntity, IAggregateRoot
{
    public string Actor { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}

public class AuditChange : BaseEntity, IAggregateRoot
{
    public string Entity { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Field { get; set; } = string.Empty;
    public string? Old { get; set; }
    public string? New { get; set; }
    public string Actor { get; set; } = string.Empty;
}

public class Approval : BaseEntity, IAggregateRoot
{
    public string Kind { get; set; } = string.Empty;
    public string DocType { get; set; } = string.Empty;
    public int DocId { get; set; }
    public string DocNumber { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.PENDING;
    public string Note { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string? DecidedBy { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? Reason { get; set; }
}

public class Attachment : BaseEntity, IAggregateRoot
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string StoredName { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
}

public class ServiceTypeRef : BaseEntity, IAggregateRoot
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}

public class CompanyProfile : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = "Broom Services Co.";
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CrNumber { get; set; } = string.Empty;
    public string BankDetails { get; set; } = string.Empty;
    public int? LogoAttachmentId { get; set; }
}
