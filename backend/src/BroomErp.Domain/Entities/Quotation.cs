using BroomErp.Domain.Common;
using BroomErp.Domain.Enums;

namespace BroomErp.Domain.Entities;

public class Quotation : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? EnquiryId { get; set; }
    public Enquiry? Enquiry { get; set; }

    public decimal DiscountPct { get; set; } = 0;
    public decimal Total { get; set; } = 0;
    public string PaymentTerms { get; set; } = string.Empty;
    public QuotationStatus Status { get; set; } = QuotationStatus.DRAFT;
    public string ServiceAddress { get; set; } = string.Empty;
    public string Basis { get; set; } = "DAILY"; // DAILY | HOURLY
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public bool CancelRequested { get; set; } = false;
    public string? CancelReason { get; set; }

    public List<QuotationLine> Lines { get; set; } = [];

    public decimal Subtotal => Lines.Sum(l => l.Qty * l.UnitPrice * (l.Days > 0 ? l.Days : 1));
    public string? CustomerName => Customer?.Name;

    public void RecalculateTotal()
    {
        var sub = Subtotal;
        var discount = sub * (DiscountPct / 100m);
        Total = Math.Round(sub - discount, 2);
    }
}

public class QuotationLine : BaseEntity
{
    public int QuotationId { get; set; }
    public Quotation? Quotation { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public int Days { get; set; } = 1;
    public string Location { get; set; } = string.Empty;
}
