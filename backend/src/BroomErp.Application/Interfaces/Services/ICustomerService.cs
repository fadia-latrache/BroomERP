using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;

namespace BroomErp.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetCustomersAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<CustomerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task ToggleActiveAsync(int id, bool active, CancellationToken cancellationToken = default);
    Task<CustomerHistoryDto> GetHistoryAsync(int id, CancellationToken cancellationToken = default);
    Task<List<AddressDto>> GetAddressesAsync(int customerId, CancellationToken cancellationToken = default);
    Task<AddressDto> AddAddressAsync(int customerId, CreateAddressRequest request, CancellationToken cancellationToken = default);
}
