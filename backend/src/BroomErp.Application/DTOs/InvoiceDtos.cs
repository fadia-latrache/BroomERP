using BroomErp.Domain.Enums;

namespace BroomErp.Application.DTOs;

public record InvoiceDto(
    int Id,
    string Number,
    int CustomerId,
    string? CustomerName,
    int JobId,
    decimal Total,
    decimal Paid,
    decimal Balance,
    DateOnly? DueDate,
    InvoiceStatus Status,
    bool Overdue,
    bool CancelRequested,
    string? CancelReason,
    DateTime CreatedAt
);

public record CreateInvoiceRequest(DateOnly? DueDate = null);

public record PaymentDto(
    int Id,
    string ReceiptNo,
    int InvoiceId,
    decimal Amount,
    PaymentMethod Method,
    string Reference,
    string ReceivedBy,
    DateTime CreatedAt
);

public record CreatePaymentRequest(decimal Amount, PaymentMethod Method, string Reference = "");
