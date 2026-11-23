using BroomErp.Domain.Enums;

namespace BroomErp.Application.DTOs;

public record SupplierDto(int Id, string Name, string Phone, DateTime CreatedAt);
public record CreateSupplierRequest(string Name, string Phone = "");
public record UpdateSupplierRequest(string? Name = null, string? Phone = null);

public record RequisitionDto(
    int Id,
    string Number,
    string Description,
    decimal Qty,
    decimal EstUnitPrice,
    int? ItemId,
    string Department,
    string Reason,
    string RequestedBy,
    RequisitionStatus Status,
    DateTime CreatedAt
);

public record CreateRequisitionRequest(
    string Description,
    decimal Qty,
    decimal EstUnitPrice = 0,
    int? ItemId = null,
    string Department = "",
    string Reason = ""
);

public record SupplierQuotationDto(
    int Id,
    string Number,
    int RequisitionId,
    int SupplierId,
    string? SupplierName,
    decimal UnitPrice,
    string Notes,
    bool Selected,
    DateTime CreatedAt
);

public record CreateSupplierQuotationRequest(int SupplierId, decimal UnitPrice, string Notes = "");

public record LpoDto(
    int Id,
    string Number,
    int RequisitionId,
    int SupplierId,
    string? SupplierName,
    decimal Qty,
    decimal UnitPrice,
    decimal Total,
    LpoStatus Status,
    string? GrnNo,
    DateTime CreatedAt
);

public record CreateLpoRequest(int? SupplierId = null, decimal? UnitPrice = null);

public record SupplierInvoiceDto(
    int Id,
    string Number,
    string SupplierRef,
    int LpoId,
    string? LpoNumber,
    string? SupplierName,
    decimal Amount,
    decimal Paid,
    decimal Balance,
    DateOnly? DueDate,
    bool MatchesLpo,
    SupplierInvoiceStatus Status,
    DateTime CreatedAt
);

public record CreateSupplierInvoiceRequest(
    int LpoId,
    string SupplierRef,
    decimal Amount,
    DateOnly? DueDate = null
);

public record VoucherDto(
    int Id,
    string Number,
    VoucherKind Kind,
    string Payee,
    decimal Amount,
    PaymentMethod Method,
    VoucherStatus Status,
    int? LpoId,
    int? SupplierInvoiceId,
    int? InvoiceId,
    string Note,
    DateOnly? PaidOn,
    DateTime CreatedAt
);

public record CreateVoucherRequest(
    VoucherKind Kind,
    string Payee,
    decimal Amount,
    PaymentMethod Method = PaymentMethod.BANK_TRANSFER,
    int? LpoId = null,
    int? SupplierInvoiceId = null,
    int? InvoiceId = null,
    string Note = ""
);
