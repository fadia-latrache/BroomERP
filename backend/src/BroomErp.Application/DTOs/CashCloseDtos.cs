using BroomErp.Domain.Enums;

namespace BroomErp.Application.DTOs;

public record CashCloseDto(
    int Id,
    DateOnly CloseDate,
    decimal OpeningFloat,
    decimal CashReceived,
    decimal CashPaidOut,
    decimal HandedOver,
    decimal Expected,
    decimal Counted,
    decimal Difference,
    CloseStatus Status,
    DateTime CreatedAt
);

public record CreateCashCloseRequest(
    DateOnly CloseDate,
    decimal OpeningFloat = 0,
    decimal Counted = 0,
    decimal HandedOver = 0
);
