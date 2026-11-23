namespace BroomErp.Domain.Enums;

public enum Role
{
    GM,
    FIELD,
    SALES,
    OPS,
    STORE,
    FINANCE,
    SUPERVISOR
}

public enum ServiceType
{
    CLEANING,
    DEEP_CLEANING,
    MAIDS_HOURLY,
    MAIDS_MONTHLY,
    AC,
    MOVING,
    EVENT,
    FACADE,
    SOFT_SKILL,
    FURNITURE,
    CARPET,
    CURTAINS,
    HOTEL_KEEPING,
    MARBLE
}

public enum QuotationStatus
{
    DRAFT,
    PENDING_APPROVAL,
    APPROVED,
    ACCEPTED,
    REJECTED,
    CANCELLED
}

public enum JobStatus
{
    PLANNED,
    IN_PROGRESS,
    COMPLETED,
    PROBLEM,
    CANCELLED
}

public enum InvoiceStatus
{
    DRAFT,
    UNPAID,
    PARTIAL,
    PAID,
    CANCELLED
}

public enum PaymentMethod
{
    CASH,
    BANK_TRANSFER
}

public enum MovementType
{
    RECEIVE,
    ISSUE,
    RETURN,
    ADJUST
}

public enum AssetStatus
{
    AVAILABLE,
    ISSUED,
    UNDER_MAINTENANCE,
    DAMAGED,
    MISSING,
    RETIRED
}

public enum Condition
{
    GOOD,
    DEFECTIVE
}

public enum MaintenanceStatus
{
    REPORTED,
    UNDER_REVIEW,
    AWAITING_COST_APPROVAL,
    UNDER_MAINTENANCE,
    REPAIRED,
    CLOSED
}

public enum RequisitionStatus
{
    DRAFT,
    PENDING_APPROVAL,
    APPROVED,
    REJECTED,
    ORDERED
}

public enum LpoStatus
{
    PENDING_APPROVAL,
    ORDERED,
    RECEIVED,
    REJECTED
}

public enum VoucherKind
{
    SUPPLIER,
    EXPENSE,
    REFUND
}

public enum VoucherStatus
{
    PENDING_APPROVAL,
    APPROVED,
    PAID,
    REJECTED
}

public enum CloseStatus
{
    CLOSED,
    NEEDS_APPROVAL,
    APPROVED
}

public enum EnquiryStatus
{
    NEW,
    CONTACTED,
    QUOTATION_REQUIRED,
    QUOTATION_SENT,
    ACCEPTED,
    REJECTED,
    CONVERTED,
    CANCELLED
}

public enum ApprovalStatus
{
    PENDING,
    APPROVED,
    REJECTED
}

public enum CountStatus
{
    MATCHED,
    PENDING_APPROVAL,
    APPLIED,
    REJECTED
}

public enum SupplierInvoiceStatus
{
    RECORDED,
    PARTIAL,
    PAID
}

public enum RentalBasis
{
    DAILY,
    MONTHLY
}

public enum RentalStatus
{
    ACTIVE,
    RETURNED
}
