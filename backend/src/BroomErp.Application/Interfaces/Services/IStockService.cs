using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;

namespace BroomErp.Application.Interfaces.Services;

public interface IStockService
{
    Task<PagedResult<StockItemDto>> GetStockItemsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<StockItemDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<StockItemDto> CreateAsync(CreateStockItemRequest request, CancellationToken cancellationToken = default);
    Task<StockItemDto> UpdateAsync(int id, UpdateStockItemRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<StockMovementDto>> GetMovementsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<StockMovementDto> RecordMovementAsync(int itemId, CreateStockMovementRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<StockCountDto>> GetStockCountsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<StockCountDto> RecordCountAsync(CreateStockCountRequest request, CancellationToken cancellationToken = default);
    Task ApplyCountAdjustmentAsync(int countId, CancellationToken cancellationToken = default);
}
