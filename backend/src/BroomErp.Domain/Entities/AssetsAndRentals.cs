using BroomErp.Domain.Common;
using BroomErp.Domain.Enums;

namespace BroomErp.Domain.Entities;

public class Asset : BaseEntity, IAggregateRoot
{
    public string Tag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Machine";
    public AssetStatus Status { get; set; } = AssetStatus.AVAILABLE;
    public string? Custodian { get; set; }
    public Condition Condition { get; set; } = Condition.GOOD;
    public string SerialNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PurchaseInfo { get; set; } = string.Empty;
    public int? CurrentJobId { get; set; }
    public Job? CurrentJob { get; set; }
}

public class AssetAssignment : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }
    public int? JobId { get; set; }
    public Job? Job { get; set; }
    public string Custodian { get; set; } = string.Empty;
    public string IssuedBy { get; set; } = string.Empty;
    public DateTime? ReturnedAt { get; set; }
    public string? ReturnCondition { get; set; }
    public string? ReturnNote { get; set; }
    public string? ReturnedTo { get; set; }
}

public class MaintenanceRequest : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }
    public string Problem { get; set; } = string.Empty;
    public decimal Cost { get; set; } = 0;
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.REPORTED;
    public string Priority { get; set; } = "NORMAL";
    public string ReportedBy { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string RepairSupplier { get; set; } = string.Empty;
    public string Parts { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; } = 0;
    public DateOnly? CompletedOn { get; set; }

    public string? AssetName => Asset?.Name;
}

public class RentalMachine : BaseEntity, IAggregateRoot
{
    public string Number { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RentalRef { get; set; } = string.Empty;
    public int? JobId { get; set; }
    public Job? Job { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public RentalBasis Basis { get; set; }
    public decimal Rate { get; set; }
    public string ConditionOut { get; set; } = string.Empty;
    public string? ConditionIn { get; set; }
    public string Notes { get; set; } = string.Empty;
    public RentalStatus Status { get; set; } = RentalStatus.ACTIVE;

    public string? SupplierName => Supplier?.Name;

    public decimal Cost
    {
        get
        {
            var end = EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var days = Math.Max(1, (end.DayNumber - StartDate.DayNumber) + 1);
            if (Basis == RentalBasis.DAILY)
                return Rate * days;
            var months = (int)Math.Ceiling(days / 30.0);
            return Rate * months;
        }
    }
}

public class RentalTimesheet : BaseEntity, IAggregateRoot
{
    public int RentalId { get; set; }
    public RentalMachine? Rental { get; set; }
    public DateOnly WorkDate { get; set; }
    public decimal Hours { get; set; }
    public string Note { get; set; } = string.Empty;
    public string RecordedBy { get; set; } = string.Empty;
}
