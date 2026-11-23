using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;

namespace BroomErp.Application.Interfaces.Services;

public interface IPurchasingService
{
    Task<PagedResult<SupplierDto>> GetSuppliersAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<RequisitionDto>> GetRequisitionsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<RequisitionDto> CreateRequisitionAsync(CreateRequisitionRequest request, CancellationToken cancellationToken = default);
    Task ApproveRequisitionAsync(int id, CancellationToken cancellationToken = default);

    Task<List<SupplierQuotationDto>> GetSupplierQuotationsAsync(int requisitionId, CancellationToken cancellationToken = default);
    Task<SupplierQuotationDto> AddSupplierQuotationAsync(int requisitionId, CreateSupplierQuotationRequest request, CancellationToken cancellationToken = default);
    Task SelectSupplierQuotationAsync(int quotationId, CancellationToken cancellationToken = default);

    Task<PagedResult<LpoDto>> GetLposAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<LpoDto> CreateLpoAsync(int requisitionId, CreateLpoRequest request, CancellationToken cancellationToken = default);
    Task ApproveLpoAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<SupplierInvoiceDto>> GetSupplierInvoicesAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<SupplierInvoiceDto> RecordSupplierInvoiceAsync(CreateSupplierInvoiceRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<VoucherDto>> GetVouchersAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<VoucherDto> CreateVoucherAsync(CreateVoucherRequest request, CancellationToken cancellationToken = default);
    Task ApproveVoucherAsync(int id, CancellationToken cancellationToken = default);
    Task PayVoucherAsync(int id, CancellationToken cancellationToken = default);
}
