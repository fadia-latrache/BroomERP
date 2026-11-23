using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Enums;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Application.Services;

public class AssetService : IAssetService
{
    private readonly IRepository<Asset> _assetRepo;
    private readonly IRepository<AssetAssignment> _assignRepo;
    private readonly IRepository<MaintenanceRequest> _maintRepo;
    private readonly IRepository<RentalMachine> _rentalRepo;
    private readonly IRepository<RentalTimesheet> _timesheetRepo;
    private readonly IUnitOfWork _uow;
    private readonly ISequenceGenerator _sequences;
    private readonly ICurrentUserContext _userContext;

    public AssetService(
        IRepository<Asset> assetRepo,
        IRepository<AssetAssignment> assignRepo,
        IRepository<MaintenanceRequest> maintRepo,
        IRepository<RentalMachine> rentalRepo,
        IRepository<RentalTimesheet> timesheetRepo,
        IUnitOfWork uow,
        ISequenceGenerator sequences,
        ICurrentUserContext userContext)
    {
        _assetRepo = assetRepo;
        _assignRepo = assignRepo;
        _maintRepo = maintRepo;
        _rentalRepo = rentalRepo;
        _timesheetRepo = timesheetRepo;
        _uow = uow;
        _sequences = sequences;
        _userContext = userContext;
    }

    public async Task<PagedResult<AssetDto>> GetAssetsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _assetRepo.GetPagedAsync(query,
            a => (string.IsNullOrEmpty(query.Search) || a.Name.ToLower().Contains(query.Search.ToLower()) || a.Tag.ToLower().Contains(query.Search.ToLower())),
            cancellationToken);

        return new PagedResult<AssetDto>
        {
            Items = paged.Items.Select(a => new AssetDto(a.Id, a.Tag, a.Name, a.Category, a.Status, a.Custodian, a.Condition, a.SerialNumber, a.Description, a.PurchaseInfo, a.CurrentJobId, a.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<AssetDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var a = await _assetRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Asset not found", 404);
        return new AssetDto(a.Id, a.Tag, a.Name, a.Category, a.Status, a.Custodian, a.Condition, a.SerialNumber, a.Description, a.PurchaseInfo, a.CurrentJobId, a.CreatedAt);
    }

    public async Task<AssetDto> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken = default)
    {
        var asset = new Asset
        {
            Tag = request.Tag.Trim(),
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            SerialNumber = request.SerialNumber.Trim(),
            Description = request.Description.Trim(),
            PurchaseInfo = request.PurchaseInfo.Trim(),
            Status = AssetStatus.AVAILABLE,
            Condition = Condition.GOOD
        };
        await _assetRepo.AddAsync(asset, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new AssetDto(asset.Id, asset.Tag, asset.Name, asset.Category, asset.Status, asset.Custodian, asset.Condition, asset.SerialNumber, asset.Description, asset.PurchaseInfo, asset.CurrentJobId, asset.CreatedAt);
    }

    public async Task IssueAsync(int id, IssueAssetRequest request, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Asset not found", 404);

        if (asset.Status != AssetStatus.AVAILABLE)
            throw new DomainException($"Cannot issue asset in '{asset.Status}' status");

        var number = await _sequences.NextAsync("ASG-", cancellationToken);
        var assign = new AssetAssignment
        {
            Number = number,
            AssetId = id,
            JobId = request.JobId,
            Custodian = request.Custodian,
            IssuedBy = _userContext.Email ?? "store"
        };

        asset.Status = AssetStatus.ISSUED;
        asset.Custodian = request.Custodian;
        asset.CurrentJobId = request.JobId;

        await _assignRepo.AddAsync(assign, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task ReturnAsync(int id, ReturnAssetRequest request, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Asset not found", 404);

        var assignments = await _assignRepo.ListAsync(a => a.AssetId == id && a.ReturnedAt == null, cancellationToken);
        var current = assignments.FirstOrDefault();
        if (current != null)
        {
            current.ReturnedAt = DateTime.UtcNow;
            current.ReturnCondition = request.Condition.ToString();
            current.ReturnNote = request.Note;
            current.ReturnedTo = _userContext.Email;
        }

        asset.Condition = request.Condition;
        asset.Status = request.Condition == Condition.DEFECTIVE ? AssetStatus.UNDER_MAINTENANCE : AssetStatus.AVAILABLE;
        asset.Custodian = null;
        asset.CurrentJobId = null;

        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<MaintenanceRequestDto>> GetMaintenanceRequestsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _maintRepo.GetPagedAsync(query, null, cancellationToken, m => m.Asset!);
        return new PagedResult<MaintenanceRequestDto>
        {
            Items = paged.Items.Select(m => new MaintenanceRequestDto(
                m.Id, m.Number, m.AssetId, m.AssetName, m.Problem, m.Priority, m.ReportedBy,
                m.Status, m.RepairSupplier, m.Parts, m.EstimatedCost, m.Cost, m.Notes, m.CompletedOn, m.CreatedAt
            )).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<MaintenanceRequestDto> CreateMaintenanceRequestAsync(CreateMaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepo.GetByIdAsync(request.AssetId, cancellationToken)
            ?? throw new DomainException("Asset not found", 404);

        var number = await _sequences.NextAsync("MNT-", cancellationToken);
        var maint = new MaintenanceRequest
        {
            Number = number,
            AssetId = request.AssetId,
            Problem = request.Problem,
            Priority = request.Priority,
            Notes = request.Notes,
            ReportedBy = _userContext.Email ?? "field",
            Status = MaintenanceStatus.REPORTED
        };

        asset.Status = AssetStatus.UNDER_MAINTENANCE;

        await _maintRepo.AddAsync(maint, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new MaintenanceRequestDto(
            maint.Id, maint.Number, maint.AssetId, asset.Name, maint.Problem, maint.Priority, maint.ReportedBy,
            maint.Status, maint.RepairSupplier, maint.Parts, maint.EstimatedCost, maint.Cost, maint.Notes, maint.CompletedOn, maint.CreatedAt
        );
    }

    public async Task StartMaintenanceAsync(int id, StartMaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        var m = await _maintRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Maintenance request not found", 404);

        m.RepairSupplier = request.RepairSupplier;
        m.Parts = request.Parts;
        m.EstimatedCost = request.EstimatedCost;
        m.Status = MaintenanceStatus.UNDER_MAINTENANCE;

        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteRepairAsync(int id, RepairMaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        var m = await _maintRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Maintenance request not found", 404);

        var asset = await _assetRepo.GetByIdAsync(m.AssetId, cancellationToken)
            ?? throw new DomainException("Asset not found", 404);

        m.Cost = request.Cost;
        m.Notes = string.IsNullOrWhiteSpace(m.Notes) ? request.Notes : $"{m.Notes}\n{request.Notes}";
        m.Status = MaintenanceStatus.REPAIRED;
        m.CompletedOn = DateOnly.FromDateTime(DateTime.UtcNow);

        asset.Status = AssetStatus.AVAILABLE;
        asset.Condition = Condition.GOOD;

        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<RentalDto>> GetRentalsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _rentalRepo.GetPagedAsync(query, null, cancellationToken, r => r.Supplier!);
        return new PagedResult<RentalDto>
        {
            Items = paged.Items.Select(r => new RentalDto(
                r.Id, r.Number, r.SupplierId, r.SupplierName, r.Name, r.RentalRef, r.JobId,
                r.StartDate, r.EndDate, r.Basis, r.Rate, r.Cost, r.ConditionOut, r.ConditionIn,
                r.Status, r.Notes, r.CreatedAt
            )).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<RentalDto> CreateRentalAsync(CreateRentalRequest request, CancellationToken cancellationToken = default)
    {
        var number = await _sequences.NextAsync("RNT-", cancellationToken);
        var rental = new RentalMachine
        {
            Number = number,
            SupplierId = request.SupplierId,
            Name = request.Name.Trim(),
            RentalRef = request.RentalRef.Trim(),
            JobId = request.JobId,
            StartDate = request.StartDate,
            Basis = request.Basis,
            Rate = request.Rate,
            ConditionOut = request.ConditionOut,
            Notes = request.Notes,
            Status = RentalStatus.ACTIVE
        };
        await _rentalRepo.AddAsync(rental, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new RentalDto(
            rental.Id, rental.Number, rental.SupplierId, null, rental.Name, rental.RentalRef, rental.JobId,
            rental.StartDate, rental.EndDate, rental.Basis, rental.Rate, rental.Cost, rental.ConditionOut, rental.ConditionIn,
            rental.Status, rental.Notes, rental.CreatedAt
        );
    }

    public async Task ReturnRentalAsync(int id, ReturnRentalRequest request, CancellationToken cancellationToken = default)
    {
        var rental = await _rentalRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Rental record not found", 404);

        rental.Status = RentalStatus.RETURNED;
        rental.ConditionIn = request.ConditionIn;
        rental.EndDate = request.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        if (!string.IsNullOrWhiteSpace(request.Notes))
            rental.Notes = $"{rental.Notes}\nReturn notes: {request.Notes}";

        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<RentalTimesheetDto> RecordTimesheetAsync(int rentalId, CreateTimesheetRequest request, CancellationToken cancellationToken = default)
    {
        var ts = new RentalTimesheet
        {
            RentalId = rentalId,
            WorkDate = request.WorkDate,
            Hours = request.Hours,
            Note = request.Note,
            RecordedBy = _userContext.Email ?? "field"
        };
        await _timesheetRepo.AddAsync(ts, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new RentalTimesheetDto(ts.Id, ts.RentalId, ts.WorkDate, ts.Hours, ts.Note, ts.RecordedBy);
    }
}
