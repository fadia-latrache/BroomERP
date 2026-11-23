using BroomErp.Domain.Enums;

namespace BroomErp.Application.DTOs;

public record QuotationLineDto(int Id, string Description, decimal Qty, decimal UnitPrice, int Days, string Location);
public record QuotationLineRequest(string Description, decimal Qty, decimal UnitPrice, int Days = 1, string Location = "");

public record QuotationDto(
    int Id,
    string Number,
    int CustomerId,
    string? CustomerName,
    decimal Subtotal,
    decimal DiscountPct,
    decimal Total,
    QuotationStatus Status,
    string PaymentTerms,
    string ServiceAddress,
    string Basis,
    string Notes,
    string CreatedBy,
    string? ApprovedBy,
    bool CancelRequested,
    string? CancelReason,
    List<QuotationLineDto> Lines,
    DateTime CreatedAt
);

public record CreateQuotationRequest(
    int CustomerId,
    List<QuotationLineRequest> Lines,
    int? EnquiryId = null,
    decimal DiscountPct = 0,
    string PaymentTerms = "",
    string ServiceAddress = "",
    string Basis = "DAILY",
    string Notes = ""
);

public record UpdateQuotationRequest(
    List<QuotationLineRequest>? Lines = null,
    decimal? DiscountPct = null,
    string? PaymentTerms = null,
    string? ServiceAddress = null,
    string? Basis = null,
    string? Notes = null
);

public record AcceptQuotationRequest(
    string ServiceType,
    DateOnly ScheduledFor,
    string Address = "",
    string? StartTime = null
);

public record VoiceQuotationRequest(string Prompt, bool AutoCreate = false, int? CustomerId = null);
public record VoiceQuotationResponse(bool Success, object Parsed, object? Created, string VoiceResponse, string Prompt);
