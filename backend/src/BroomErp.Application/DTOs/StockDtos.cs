using BroomErp.Domain.Enums;

namespace BroomErp.Application.DTOs;

public record StockItemDto(
    int Id,
    string Name,
    string Category,
    string Unit,
    decimal Qty,
    decimal ReorderLevel,
    bool Low,
    DateTime CreatedAt
);

public record CreateStockItemRequest(
    string Name,
    string Category = "Chemicals",
    string Unit = "pcs",
    decimal ReorderLevel = 5
);

public record UpdateStockItemRequest(
    string? Name = null,
    string? Category = null,
    string? Unit = null,
    decimal? ReorderLevel = null
);

public record StockMovementDto(
    int Id,
    string Number,
    int ItemId,
    string? ItemName,
    MovementType Type,
    decimal Qty,
    int? JobId,
    string Recipient,
    string CreatedBy,
    string Reason,
    string Condition,
    DateTime CreatedAt
);

public record CreateStockMovementRequest(
    MovementType Type,
    decimal Qty,
    int? JobId = null,
    string Reason = "",
    string Recipient = "",
    string Condition = "GOOD"
);

public record StockCountDto(
    int Id,
    string Number,
    int ItemId,
    string? ItemName,
    decimal SystemQty,
    decimal CountedQty,
    decimal Difference,
    string Reason,
    string CountedBy,
    CountStatus Status,
    DateTime CreatedAt
);

public record CreateStockCountRequest(int ItemId, decimal CountedQty, string Reason = "");
