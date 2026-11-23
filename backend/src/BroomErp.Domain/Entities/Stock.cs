using BroomErp.Domain.Common;
using BroomErp.Domain.Enums;

namespace BroomErp.Domain.Entities;

public class StockItem : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Chemicals";
    public string Unit { get; set; } = "pcs";
    public decimal Qty { get; set; } = 0;
    public decimal ReorderLevel { get; set; } = 5;

    public List<StockMovement> Movements { get; set; } = [];

    public bool Low => Qty <= ReorderLevel;
}

public class StockMovement : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public StockItem? Item { get; set; }
    public MovementType Type { get; set; }
    public decimal Qty { get; set; }
    public int? JobId { get; set; }
    public Job? Job { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string Condition { get; set; } = "GOOD";

    public string? ItemName => Item?.Name;
}

public class StockCount : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public StockItem? Item { get; set; }
    public decimal SystemQty { get; set; }
    public decimal CountedQty { get; set; }
    public decimal Difference { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string CountedBy { get; set; } = string.Empty;
    public CountStatus Status { get; set; }

    public string? ItemName => Item?.Name;
}
