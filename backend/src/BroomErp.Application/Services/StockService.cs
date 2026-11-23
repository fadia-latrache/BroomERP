using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Enums;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Application.Services;

public class StockService : IStockService
{
    private readonly IRepository<StockItem> _stockRepo;
    private readonly IRepository<StockMovement> _moveRepo;
    private readonly IRepository<StockCount> _countRepo;
    private readonly IUnitOfWork _uow;
    private readonly ISequenceGenerator _sequences;
    private readonly ICurrentUserContext _userContext;

    public StockService(
        IRepository<StockItem> stockRepo,
        IRepository<StockMovement> moveRepo,
        IRepository<StockCount> countRepo,
        IUnitOfWork uow,
        ISequenceGenerator sequences,
        ICurrentUserContext userContext)
    {
        _stockRepo = stockRepo;
        _moveRepo = moveRepo;
        _countRepo = countRepo;
        _uow = uow;
        _sequences = sequences;
        _userContext = userContext;
    }

    public async Task<PagedResult<StockItemDto>> GetStockItemsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _stockRepo.GetPagedAsync(query,
            s => (string.IsNullOrEmpty(query.Search) || s.Name.ToLower().Contains(query.Search.ToLower()) || s.Category.ToLower().Contains(query.Search.ToLower())),
            cancellationToken);

        return new PagedResult<StockItemDto>
        {
            Items = paged.Items.Select(s => new StockItemDto(s.Id, s.Name, s.Category, s.Unit, s.Qty, s.ReorderLevel, s.Low, s.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<StockItemDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var s = await _stockRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Stock item not found", 404);
        return new StockItemDto(s.Id, s.Name, s.Category, s.Unit, s.Qty, s.ReorderLevel, s.Low, s.CreatedAt);
    }

    public async Task<StockItemDto> CreateAsync(CreateStockItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = new StockItem
        {
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Unit = request.Unit.Trim(),
            ReorderLevel = request.ReorderLevel,
            Qty = 0
        };
        await _stockRepo.AddAsync(item, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new StockItemDto(item.Id, item.Name, item.Category, item.Unit, item.Qty, item.ReorderLevel, item.Low, item.CreatedAt);
    }

    public async Task<StockItemDto> UpdateAsync(int id, UpdateStockItemRequest request, CancellationToken cancellationToken = default)
    {
        var s = await _stockRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Stock item not found", 404);

        if (request.Name != null) s.Name = request.Name.Trim();
        if (request.Category != null) s.Category = request.Category.Trim();
        if (request.Unit != null) s.Unit = request.Unit.Trim();
        if (request.ReorderLevel.HasValue) s.ReorderLevel = request.ReorderLevel.Value;

        await _uow.SaveChangesAsync(cancellationToken);
        return new StockItemDto(s.Id, s.Name, s.Category, s.Unit, s.Qty, s.ReorderLevel, s.Low, s.CreatedAt);
    }

    public async Task<PagedResult<StockMovementDto>> GetMovementsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _moveRepo.GetPagedAsync(query, null, cancellationToken, m => m.Item!);
        return new PagedResult<StockMovementDto>
        {
            Items = paged.Items.Select(m => new StockMovementDto(m.Id, m.Number, m.ItemId, m.ItemName, m.Type, m.Qty, m.JobId, m.Recipient, m.CreatedBy, m.Reason, m.Condition, m.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<StockMovementDto> RecordMovementAsync(int itemId, CreateStockMovementRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _stockRepo.GetByIdAsync(itemId, cancellationToken)
            ?? throw new DomainException("Stock item not found", 404);

        if (request.Type == MovementType.ISSUE && item.Qty < request.Qty)
            throw new DomainException($"Cannot issue {request.Qty} {item.Unit}. Current stock is only {item.Qty}");

        var number = await _sequences.NextAsync("MV-", cancellationToken);
        var move = new StockMovement
        {
            Number = number,
            ItemId = itemId,
            Type = request.Type,
            Qty = request.Qty,
            JobId = request.JobId,
            Reason = request.Reason,
            Recipient = request.Recipient,
            CreatedBy = _userContext.Email ?? "store",
            Condition = request.Condition
        };

        if (request.Type == MovementType.RECEIVE || request.Type == MovementType.RETURN)
            item.Qty += request.Qty;
        else if (request.Type == MovementType.ISSUE)
            item.Qty -= request.Qty;
        else if (request.Type == MovementType.ADJUST)
            item.Qty = request.Qty;

        await _moveRepo.AddAsync(move, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new StockMovementDto(move.Id, move.Number, move.ItemId, item.Name, move.Type, move.Qty, move.JobId, move.Recipient, move.CreatedBy, move.Reason, move.Condition, move.CreatedAt);
    }

    public async Task<PagedResult<StockCountDto>> GetStockCountsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _countRepo.GetPagedAsync(query, null, cancellationToken, c => c.Item!);
        return new PagedResult<StockCountDto>
        {
            Items = paged.Items.Select(c => new StockCountDto(c.Id, c.Number, c.ItemId, c.ItemName, c.SystemQty, c.CountedQty, c.Difference, c.Reason, c.CountedBy, c.Status, c.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<StockCountDto> RecordCountAsync(CreateStockCountRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _stockRepo.GetByIdAsync(request.ItemId, cancellationToken)
            ?? throw new DomainException("Stock item not found", 404);

        var diff = request.CountedQty - item.Qty;
        var number = await _sequences.NextAsync("SC-", cancellationToken);
        var count = new StockCount
        {
            Number = number,
            ItemId = request.ItemId,
            SystemQty = item.Qty,
            CountedQty = request.CountedQty,
            Difference = diff,
            Reason = request.Reason,
            CountedBy = _userContext.Email ?? "store",
            Status = diff == 0 ? CountStatus.MATCHED : CountStatus.PENDING_APPROVAL
        };

        await _countRepo.AddAsync(count, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new StockCountDto(count.Id, count.Number, count.ItemId, item.Name, count.SystemQty, count.CountedQty, count.Difference, count.Reason, count.CountedBy, count.Status, count.CreatedAt);
    }

    public async Task ApplyCountAdjustmentAsync(int countId, CancellationToken cancellationToken = default)
    {
        var count = await _countRepo.GetByIdAsync(countId, cancellationToken, c => c.Item!)
            ?? throw new DomainException("Stock count not found", 404);

        if (count.Status != CountStatus.PENDING_APPROVAL)
            throw new DomainException("Only pending count adjustments can be applied");

        var item = count.Item ?? await _stockRepo.GetByIdAsync(count.ItemId, cancellationToken)
            ?? throw new DomainException("Item not found", 404);

        item.Qty = count.CountedQty;
        count.Status = CountStatus.APPLIED;

        var moveNumber = await _sequences.NextAsync("MV-", cancellationToken);
        var move = new StockMovement
        {
            Number = moveNumber,
            ItemId = count.ItemId,
            Type = MovementType.ADJUST,
            Qty = count.CountedQty,
            Reason = $"Stock count #{count.Number} adjustment: {count.Reason}",
            CreatedBy = _userContext.Email ?? "store"
        };
        await _moveRepo.AddAsync(move, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
