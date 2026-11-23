using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;

namespace BroomErp.Application.Interfaces.Services;

public interface IAssetService
{
    Task<PagedResult<AssetDto>> GetAssetsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<AssetDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AssetDto> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken = default);
    Task IssueAsync(int id, IssueAssetRequest request, CancellationToken cancellationToken = default);
    Task ReturnAsync(int id, ReturnAssetRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<MaintenanceRequestDto>> GetMaintenanceRequestsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<MaintenanceRequestDto> CreateMaintenanceRequestAsync(CreateMaintenanceRequest request, CancellationToken cancellationToken = default);
    Task StartMaintenanceAsync(int id, StartMaintenanceRequest request, CancellationToken cancellationToken = default);
    Task CompleteRepairAsync(int id, RepairMaintenanceRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<RentalDto>> GetRentalsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<RentalDto> CreateRentalAsync(CreateRentalRequest request, CancellationToken cancellationToken = default);
    Task ReturnRentalAsync(int id, ReturnRentalRequest request, CancellationToken cancellationToken = default);
    Task<RentalTimesheetDto> RecordTimesheetAsync(int rentalId, CreateTimesheetRequest request, CancellationToken cancellationToken = default);
}
