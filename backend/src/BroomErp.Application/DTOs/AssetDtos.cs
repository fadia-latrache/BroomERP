using BroomErp.Domain.Enums;

namespace BroomErp.Application.DTOs;

public record AssetDto(
    int Id,
    string Tag,
    string Name,
    string Category,
    AssetStatus Status,
    string? Custodian,
    Condition Condition,
    string SerialNumber,
    string Description,
    string PurchaseInfo,
    int? CurrentJobId,
    DateTime CreatedAt
);

public record CreateAssetRequest(
    string Tag,
    string Name,
    string Category = "Machine",
    string SerialNumber = "",
    string Description = "",
    string PurchaseInfo = ""
);

public record IssueAssetRequest(string Custodian, int? JobId = null);
public record ReturnAssetRequest(Condition Condition, string Note = "");

public record MaintenanceRequestDto(
    int Id,
    string Number,
    int AssetId,
    string? AssetName,
    string Problem,
    string Priority,
    string ReportedBy,
    MaintenanceStatus Status,
    string RepairSupplier,
    string Parts,
    decimal EstimatedCost,
    decimal Cost,
    string Notes,
    DateOnly? CompletedOn,
    DateTime CreatedAt
);

public record CreateMaintenanceRequest(
    int AssetId,
    string Problem,
    string Priority = "NORMAL",
    string Notes = ""
);

public record StartMaintenanceRequest(string RepairSupplier = "", string Parts = "", decimal EstimatedCost = 0);
public record RepairMaintenanceRequest(decimal Cost = 0, string Notes = "");

public record RentalDto(
    int Id,
    string Number,
    int SupplierId,
    string? SupplierName,
    string Name,
    string RentalRef,
    int? JobId,
    DateOnly StartDate,
    DateOnly? EndDate,
    RentalBasis Basis,
    decimal Rate,
    decimal Cost,
    string ConditionOut,
    string? ConditionIn,
    RentalStatus Status,
    string Notes,
    DateTime CreatedAt
);

public record CreateRentalRequest(
    int SupplierId,
    string Name,
    DateOnly StartDate,
    RentalBasis Basis,
    decimal Rate,
    string RentalRef = "",
    int? JobId = null,
    string ConditionOut = "",
    string Notes = ""
);

public record ReturnRentalRequest(string ConditionIn, DateOnly? EndDate = null, string Notes = "");

public record RentalTimesheetDto(int Id, int RentalId, DateOnly WorkDate, decimal Hours, string Note, string RecordedBy);
public record CreateTimesheetRequest(DateOnly WorkDate, decimal Hours, string Note = "");
