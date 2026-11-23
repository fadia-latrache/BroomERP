using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;

namespace BroomErp.Application.Interfaces.Services;

public interface IInvoiceService
{
    Task<PagedResult<InvoiceDto>> GetInvoicesAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<InvoiceDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InvoiceDto> CreateForJobAsync(int jobId, CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<InvoiceDto> IssueAsync(int id, CancellationToken cancellationToken = default);
    Task<PaymentDto> RecordPaymentAsync(int invoiceId, CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task RequestCancelAsync(int id, string reason, CancellationToken cancellationToken = default);
    Task<PagedResult<PaymentDto>> GetPaymentsAsync(QueryParams query, CancellationToken cancellationToken = default);
}
