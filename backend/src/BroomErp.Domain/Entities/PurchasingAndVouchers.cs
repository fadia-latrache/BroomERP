using BroomErp.Domain.Common;
using BroomErp.Domain.Enums;

namespace BroomErp.Domain.Entities;

public class Supplier : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class Requisition : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public StockItem? Item { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal EstUnitPrice { get; set; } = 0;
    public string Department { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public RequisitionStatus Status { get; set; } = RequisitionStatus.DRAFT;
}

public class SupplierQuotation : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int RequisitionId { get; set; }
    public Requisition? Requisition { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal UnitPrice { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool Selected { get; set; } = false;

    public string? SupplierName => Supplier?.Name;
}

public class Lpo : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int RequisitionId { get; set; }
    public Requisition? Requisition { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public LpoStatus Status { get; set; } = LpoStatus.PENDING_APPROVAL;
    public string? GrnNo { get; set; }

    public string? SupplierName => Supplier?.Name;
}

public class SupplierInvoice : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public string SupplierRef { get; set; } = string.Empty;
    public int LpoId { get; set; }
    public Lpo? Lpo { get; set; }
    public decimal Amount { get; set; }
    public decimal Paid { get; set; } = 0;
    public DateOnly? DueDate { get; set; }
    public bool MatchesLpo { get; set; } = true;
    public SupplierInvoiceStatus Status { get; set; } = SupplierInvoiceStatus.RECORDED;

    public string? SupplierName => Lpo?.SupplierName;
    public string? LpoNumber => Lpo?.Number;
    public decimal Balance => Amount - Paid;
}

public class Voucher : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public VoucherKind Kind { get; set; }
    public string Payee { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public int? LpoId { get; set; }
    public int? SupplierInvoiceId { get; set; }
    public int? InvoiceId { get; set; }
    public string Note { get; set; } = string.Empty;
    public VoucherStatus Status { get; set; } = VoucherStatus.PENDING_APPROVAL;
    public DateOnly? PaidOn { get; set; }
}
