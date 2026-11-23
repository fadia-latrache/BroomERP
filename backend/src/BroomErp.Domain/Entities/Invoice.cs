using BroomErp.Domain.Common;
using BroomErp.Domain.Enums;

namespace BroomErp.Domain.Entities;

public class Invoice : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int JobId { get; set; }
    public Job? Job { get; set; }

    public decimal Total { get; set; }
    public decimal Paid { get; set; } = 0;
    public DateOnly? DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.DRAFT;
    public string? CancelReason { get; set; }
    public bool CancelRequested { get; set; } = false;

    public List<Payment> Payments { get; set; } = [];

    public decimal Balance => Total - Paid;
    public string? CustomerName => Customer?.Name;
    public bool Overdue => DueDate.HasValue &&
                           DueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow) &&
                           (Status == InvoiceStatus.UNPAID || Status == InvoiceStatus.PARTIAL);
}

public class Payment : BaseEntity, IAggregateRoot
{
    public string ReceiptNo { get; set; } = string.Empty;
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
}
